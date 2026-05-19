using BuildPay.API.Security;
using BuildPay.Application.DTOs;
using BuildPay.Application.Expenses.Commands;
using BuildPay.Application.Expenses.Queries;
using BuildPay.Application.Milestones.Commands;
using BuildPay.Application.Milestones.Queries;
using BuildPay.Application.ProjectPhotos.Commands;
using BuildPay.Application.ProjectPhotos.Queries;
using BuildPay.Application.Projects.Commands;
using BuildPay.Application.Projects.Queries;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> Create([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var customerId = User.GetUserId();
        var id = await _mediator.Send(new CreateProjectCommand(customerId, request.Name, request.Budget), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<ProjectDto>>> GetMy(CancellationToken cancellationToken)
    {
        var customerId = User.GetUserId();
        var result = await _mediator.Send(new GetMyProjectsQuery(customerId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}/contractor")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult> AssignContractor(Guid id, [FromBody] AssignContractorRequest request, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new AssignContractorCommand(id, request.ContractorId), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/milestones")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> AddMilestone(Guid id, [FromBody] AddMilestoneRequest request, CancellationToken cancellationToken)
    {
        var milestoneId = await _mediator.Send(new AddMilestoneCommand(id, request.Title, request.Amount, request.DueDate), cancellationToken);
        return CreatedAtAction(nameof(GetMilestones), new { id }, new { id = milestoneId });
    }

    [HttpGet("{id:guid}/milestones")]
    public async Task<ActionResult<List<MilestoneDto>>> GetMilestones(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectMilestonesQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("milestones/{milestoneId:guid}/status")]
    [Authorize(Roles = "Contractor,Customer,Admin")]
    public async Task<ActionResult> UpdateMilestoneStatus(Guid milestoneId, [FromBody] UpdateMilestoneStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new UpdateMilestoneStatusCommand(milestoneId, request.Status), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/expenses")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> AddExpense(Guid id, [FromBody] AddExpenseRequest request, CancellationToken cancellationToken)
    {
        var expenseId = await _mediator.Send(new AddExpenseCommand(id, request.Title, request.Amount, request.ReceiptUrl), cancellationToken);
        return CreatedAtAction(nameof(GetExpenses), new { id }, new { id = expenseId });
    }

    [HttpGet("{id:guid}/expenses")]
    public async Task<ActionResult<List<ExpenseDto>>> GetExpenses(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectExpensesQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/photos")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> AddPhoto(Guid id, [FromBody] AddProjectPhotoRequest request, CancellationToken cancellationToken)
    {
        var photoId = await _mediator.Send(new AddProjectPhotoCommand(id, request.Url, request.Caption), cancellationToken);
        return CreatedAtAction(nameof(GetPhotos), new { id }, new { id = photoId });
    }

    [HttpGet("{id:guid}/photos")]
    public async Task<ActionResult<List<ProjectPhotoDto>>> GetPhotos(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProjectPhotosQuery(id), cancellationToken);
        return Ok(result);
    }

    public sealed record CreateProjectRequest(string Name, decimal Budget);
    public sealed record AssignContractorRequest(Guid ContractorId);
    public sealed record AddMilestoneRequest(string Title, decimal Amount, DateTime DueDate);
    public sealed record UpdateMilestoneStatusRequest(MilestoneStatus Status);
    public sealed record AddExpenseRequest(string Title, decimal Amount, string? ReceiptUrl);
    public sealed record AddProjectPhotoRequest(string Url, string? Caption);
}

