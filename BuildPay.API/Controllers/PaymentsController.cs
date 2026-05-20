using BuildPay.API.Security;
using BuildPay.Application.Payments.Commands;
using BuildPay.Application.Payments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("projects/{projectId:guid}/fund")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> FundProjectEscrow(Guid projectId, [FromBody] FundEscrowRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var txId = await _mediator.Send(new FundEscrowCommand(userId, projectId, request.Amount), cancellationToken);
        return Ok(new { transactionId = txId });
    }

    [HttpGet("projects/{projectId:guid}/balance")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> GetProjectBalance(Guid projectId, CancellationToken cancellationToken)
    {
        var balance = await _mediator.Send(new GetProjectEscrowBalanceQuery(projectId), cancellationToken);
        return Ok(new { projectId, balance });
    }

    [HttpPost("milestones/{milestoneId:guid}/request-release")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult> RequestRelease(Guid milestoneId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var ok = await _mediator.Send(new RequestMilestoneReleaseCommand(userId, milestoneId), cancellationToken);
        if (!ok)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("milestones/{milestoneId:guid}/approve-release")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> ApproveRelease(Guid milestoneId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var txId = await _mediator.Send(new ApproveMilestoneReleaseCommand(userId, milestoneId), cancellationToken);
        return Ok(new { transactionId = txId });
    }

    public sealed record FundEscrowRequest(decimal Amount);
}

