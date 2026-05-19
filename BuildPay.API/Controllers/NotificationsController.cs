using BuildPay.API.Security;
using BuildPay.Application.DTOs;
using BuildPay.Application.Notifications.Commands;
using BuildPay.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<NotificationDto>>> GetMy(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetMyNotificationsQuery(userId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var ok = await _mediator.Send(new MarkNotificationReadCommand(userId, id), cancellationToken);
        if (!ok)
        {
            return NotFound();
        }

        return NoContent();
    }
}

