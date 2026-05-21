using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildPay.ServiceDefaults;

namespace BuildPay.NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _db;

    public NotificationsController(NotificationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<object>> List(CancellationToken ct)
    {
        var userId = GetUserId();
        var rows = await _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(200)
            .ToListAsync(ct);

        return Ok(rows.Select(n => new { id = n.Id, n.Type, n.Content, read = n.Read, createdAtUtc = n.CreatedAtUtc }));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult> MarkRead([FromRoute] Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var row = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);
        if (row is null)
        {
            return NotFound();
        }

        row.Read = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private Guid GetUserId()
    {
        return UserContext.GetUserId(User);
    }
}

