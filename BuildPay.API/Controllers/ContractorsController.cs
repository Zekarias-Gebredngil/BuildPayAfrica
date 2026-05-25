using BuildPay.API.Security;
using BuildPay.Application.Contractors.Commands;
using BuildPay.Application.Contractors.Queries;
using BuildPay.Application.DTOs;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/contractors")]
[Authorize]
public sealed class ContractorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ContractorDto>>> GetApproved(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetApprovedContractorsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("verifications/me")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult<VerificationStatusDto>> GetMyVerification(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetMyVerificationQuery(userId), cancellationToken);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("verifications/pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<PendingVerificationDto>>> GetPendingVerifications(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPendingVerificationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("verifications")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult<object>> SubmitVerification([FromBody] SubmitVerificationRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var contractorId = await _mediator.Send(
            new SubmitContractorVerificationCommand(
                userId,
                request.LicenceNumber,
                request.DocumentUrls.Select(url => new SubmitContractorDocument(ContractorDocumentType.License, url)).ToList()),
            cancellationToken);

        return Ok(new { contractorId });
    }

    [HttpPut("verifications/{contractorId:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SetStatus(Guid contractorId, [FromBody] SetVerificationStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new SetContractorVerificationStatusCommand(contractorId, request.Status), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    public sealed record SubmitVerificationRequest(string LicenceNumber, List<string> DocumentUrls);

    public sealed record SetVerificationStatusRequest(VerificationStatus Status, string RejectionReason = "");
}

