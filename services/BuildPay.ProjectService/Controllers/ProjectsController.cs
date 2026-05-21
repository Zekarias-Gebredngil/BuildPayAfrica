using BuildPay.Contracts;
using BuildPay.ServiceDefaults;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var isAdmin = UserContext.IsAdmin(User);
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

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
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

        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
        }

        if (!UserContext.IsAdmin(User))
        {
            if (request.Status == MilestoneStatus.Completed && project.ContractorUserId != caller)
            {
                return Forbid();
            }

            if (request.Status == MilestoneStatus.Approved && project.CustomerUserId != caller)
            {
                return Forbid();
            }
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
        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
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
        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
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

        return Ok(rows.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            budget = p.Budget,
            status = p.Status.ToString(),
            contractorUserId = p.ContractorUserId,
            createdAt = p.CreatedAtUtc
        }));
    }

    // Alias used by the Angular frontend
    [HttpGet("my")]
    [Authorize(Roles = "Customer,Admin")]
    public Task<ActionResult<object>> MyProjectsAlias(CancellationToken ct) => MyProjects(ct);

    [HttpGet("assigned")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult<object>> AssignedProjects(CancellationToken ct)
    {
        var userId = GetUserId();
        var rows = await _db.Projects.AsNoTracking()
            .Where(p => p.ContractorUserId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            budget = p.Budget,
            status = p.Status.ToString(),
            contractorUserId = p.ContractorUserId,
            createdAt = p.CreatedAtUtc
        }));
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> AllProjects(CancellationToken ct)
    {
        var rows = await _db.Projects.AsNoTracking()
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(p => new
        {
            id = p.Id,
            name = p.Name,
            budget = p.Budget,
            status = p.Status.ToString(),
            contractorUserId = p.ContractorUserId,
            customerUserId = p.CustomerUserId,
            createdAt = p.CreatedAtUtc
        }));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<object>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var p = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(p, caller))
        {
            return Forbid();
        }

        return Ok(new
        {
            id = p.Id,
            name = p.Name,
            budget = p.Budget,
            status = p.Status.ToString(),
            contractorUserId = p.ContractorUserId,
            createdAt = p.CreatedAtUtc
        });
    }

    [HttpGet("{id:guid}/milestones")]
    [Authorize]
    public async Task<ActionResult<object>> GetMilestones([FromRoute] Guid id, CancellationToken ct)
    {
        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
        }

        var rows = await _db.Milestones.AsNoTracking()
            .Where(m => m.ProjectId == id)
            .OrderBy(m => m.DueDateUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(m => new
        {
            id = m.Id,
            projectId = m.ProjectId,
            title = m.Title,
            amount = m.Amount,
            dueDate = m.DueDateUtc,
            status = m.Status.ToString()
        }));
    }

    [HttpGet("{id:guid}/expenses")]
    [Authorize]
    public async Task<ActionResult<object>> GetExpenses([FromRoute] Guid id, CancellationToken ct)
    {
        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
        }

        var rows = await _db.Expenses.AsNoTracking()
            .Where(e => e.ProjectId == id)
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(e => new
        {
            id = e.Id,
            projectId = e.ProjectId,
            title = e.Title,
            amount = e.Amount,
            receiptUrl = e.ReceiptUrl,
            createdAt = e.CreatedAtUtc
        }));
    }

    [HttpGet("{id:guid}/photos")]
    [Authorize]
    public async Task<ActionResult<object>> GetPhotos([FromRoute] Guid id, CancellationToken ct)
    {
        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
        }

        var rows = await _db.Photos.AsNoTracking()
            .Where(ph => ph.ProjectId == id)
            .OrderByDescending(ph => ph.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(ph => new
        {
            id = ph.Id,
            projectId = ph.ProjectId,
            url = ph.Url,
            caption = ph.Caption,
            createdAt = ph.CreatedAtUtc
        }));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!UserContext.IsAdmin(User) && project.CustomerUserId != caller) return Forbid();

        if (project.Status == ProjectStatus.Active)
            return BadRequest(new { message = "Cannot delete an active project. Cancel it first." });

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!UserContext.IsAdmin(User) && project.CustomerUserId != caller) return Forbid();

        if (!string.IsNullOrWhiteSpace(request.Name)) project.Name = request.Name.Trim();
        if (request.Budget.HasValue && request.Budget > 0) project.Budget = request.Budget.Value;
        if (request.Status.HasValue) project.Status = request.Status.Value;

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{projectId:guid}/milestones/{milestoneId:guid}")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult> DeleteMilestone([FromRoute] Guid projectId, [FromRoute] Guid milestoneId, CancellationToken ct)
    {
        var milestone = await _db.Milestones.FirstOrDefaultAsync(m => m.Id == milestoneId && m.ProjectId == projectId, ct);
        if (milestone is null) return NotFound();
        if (milestone.Status != MilestoneStatus.Pending)
            return BadRequest(new { message = "Only Pending milestones can be deleted." });

        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!UserContext.IsAdmin(User) && project.CustomerUserId != caller)
        {
            return Forbid();
        }

        _db.Milestones.Remove(milestone);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{projectId:guid}/expenses/{expenseId:guid}")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult> DeleteExpense([FromRoute] Guid projectId, [FromRoute] Guid expenseId, CancellationToken ct)
    {
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == expenseId && e.ProjectId == projectId, ct);
        if (expense is null) return NotFound();

        var project = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct);
        if (project is null) return NotFound();

        var caller = GetUserId();
        if (!CanAccessProject(project, caller))
        {
            return Forbid();
        }

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync(ct);
        return NoContent();
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
        return UserContext.GetUserId(User);
    }

    private bool CanAccessProject(Project project, Guid callerUserId)
    {
        if (UserContext.IsAdmin(User)) return true;
        if (callerUserId == Guid.Empty) return false;
        if (project.CustomerUserId == callerUserId) return true;
        if (project.ContractorUserId.HasValue && project.ContractorUserId.Value == callerUserId) return true;
        return false;
    }

    public sealed record CreateProjectRequest(string Name, decimal Budget);
    public sealed record UpdateProjectRequest(string? Name, decimal? Budget, ProjectStatus? Status);
    public sealed record AssignContractorRequest(Guid ContractorUserId);
    public sealed record AddMilestoneRequest(string Title, decimal Amount, DateTime DueDateUtc);
    public sealed record UpdateMilestoneStatusRequest(MilestoneStatus Status);
    public sealed record AddExpenseRequest(string Title, decimal Amount, string? ReceiptUrl);
    public sealed record AddPhotoRequest(string Url, string? Caption);
}
