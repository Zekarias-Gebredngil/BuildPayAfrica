using BuildPay.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BuildPay.ProjectService.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly ProjectDbContext _db;
    private readonly IPublishEndpoint _publish;

    public ProjectsController(ProjectDbContext db, IPublishEndpoint publish)
    {
        _db = db;
        _publish = publish;
    }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required." });
        }

        var project = new Project
        {
            CustomerUserId = GetUserId(),
            Name = request.Name.Trim(),
            Budget = request.Budget,
            Status = ProjectStatus.Draft
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);
        return Ok(new { projectId = project.Id, status = project.Status });
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult> AssignContractor([FromRoute] Guid id, [FromBody] AssignContractorRequest request, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
        {
            return NotFound();
        }

        var caller = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && project.CustomerUserId != caller)
        {
            return Forbid();
        }

        project.ContractorUserId = request.ContractorUserId;
        project.Status = ProjectStatus.Active;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/milestones")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> AddMilestone([FromRoute] Guid id, [FromBody] AddMilestoneRequest request, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null)
        {
            return NotFound();
        }

        if (project.Status != ProjectStatus.Active)
        {
            return BadRequest(new { message = "Project must be Active to add milestones." });
        }

        var milestone = new Milestone
        {
            ProjectId = project.Id,
            Title = request.Title,
            Amount = request.Amount,
            DueDateUtc = request.DueDateUtc,
            Status = MilestoneStatus.Pending
        };

        _db.Milestones.Add(milestone);
        await _db.SaveChangesAsync(ct);
        return Ok(new { milestoneId = milestone.Id, status = milestone.Status });
    }

    [HttpPut("{projectId:guid}/milestones/{milestoneId:guid}/status")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult> UpdateMilestoneStatus([FromRoute] Guid projectId, [FromRoute] Guid milestoneId, [FromBody] UpdateMilestoneStatusRequest request, CancellationToken ct)
    {
        var milestone = await _db.Milestones.FirstOrDefaultAsync(m => m.Id == milestoneId && m.ProjectId == projectId, ct);
        if (milestone is null)
        {
            return NotFound();
        }

        milestone.Status = request.Status;
        await _db.SaveChangesAsync(ct);

        if (request.Status == MilestoneStatus.Completed)
        {
            await _publish.Publish(ProjectMilestoneCompleted.Create("BuildPay.ProjectService", projectId, milestoneId, milestone.Amount), ct);
        }

        if (request.Status == MilestoneStatus.Approved)
        {
            await _publish.Publish(ProjectMilestoneApproved.Create("BuildPay.ProjectService", projectId, milestoneId), ct);
        }

        return NoContent();
    }

    [HttpPost("{projectId:guid}/expenses")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> AddExpense([FromRoute] Guid projectId, [FromBody] AddExpenseRequest request, CancellationToken ct)
    {
        var exists = await _db.Projects.AnyAsync(p => p.Id == projectId, ct);
        if (!exists)
        {
            return NotFound();
        }

        var expense = new ProjectExpense
        {
            ProjectId = projectId,
            Title = request.Title,
            Amount = request.Amount,
            ReceiptUrl = request.ReceiptUrl
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync(ct);
        return Ok(new { expenseId = expense.Id });
    }

    [HttpPost("{projectId:guid}/photos")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> AddPhoto([FromRoute] Guid projectId, [FromBody] AddPhotoRequest request, CancellationToken ct)
    {
        var exists = await _db.Projects.AnyAsync(p => p.Id == projectId, ct);
        if (!exists)
        {
            return NotFound();
        }

        var photo = new ProjectPhoto
        {
            ProjectId = projectId,
            Url = request.Url,
            Caption = request.Caption
        };

        _db.Photos.Add(photo);
        await _db.SaveChangesAsync(ct);
        return Ok(new { photoId = photo.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> MyProjects(CancellationToken ct)
    {
        var userId = GetUserId();
        var rows = await _db.Projects.AsNoTracking()
            .Where(p => p.CustomerUserId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(p => new { id = p.Id, p.Name, p.Budget, p.Status, p.ContractorUserId }));
    }

    [HttpPost("{projectId:guid}/bids")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult> Bid([FromRoute] Guid projectId, CancellationToken ct)
    {
        var contractorId = GetUserId();
        var verified = await _db.VerifiedContractors.AnyAsync(v => v.UserId == contractorId, ct);
        if (!verified && !User.IsInRole("Admin"))
        {
            return StatusCode(403, new { message = "Contractor verification is required before bidding." });
        }

        var exists = await _db.Projects.AnyAsync(p => p.Id == projectId, ct);
        return exists ? NoContent() : NotFound();
    }

    [HttpGet("/internal/stats/projects")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> ProjectsCount(CancellationToken ct)
    {
        var count = await _db.Projects.CountAsync(ct);
        return Ok(new { count });
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    public sealed record CreateProjectRequest(string Name, decimal Budget);
    public sealed record AssignContractorRequest(Guid ContractorUserId);
    public sealed record AddMilestoneRequest(string Title, decimal Amount, DateTime DueDateUtc);
    public sealed record UpdateMilestoneStatusRequest(MilestoneStatus Status);
    public sealed record AddExpenseRequest(string Title, decimal Amount, string? ReceiptUrl);
    public sealed record AddPhotoRequest(string Url, string? Caption);
}
