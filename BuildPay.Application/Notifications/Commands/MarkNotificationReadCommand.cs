using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Notifications.Commands;

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest<bool>;

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        RuleFor(v => v.NotificationId).NotEmpty();
    }
}

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, bool>
{
    private readonly IBuildPayDbContext _context;

    public MarkNotificationReadCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
