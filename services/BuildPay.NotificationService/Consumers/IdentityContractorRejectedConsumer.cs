using BuildPay.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.NotificationService;

public sealed class IdentityContractorRejectedConsumer(NotificationDbContext db, ILogger<IdentityContractorRejectedConsumer> logger)
    : IConsumer<IdentityContractorRejected>
{
    public async Task Consume(ConsumeContext<IdentityContractorRejected> context)
    {
        var evt = context.Message;
        if (await db.InboxMessages.AnyAsync(x => x.EventId == evt.EventId, context.CancellationToken))
        {
            return;
        }

        db.InboxMessages.Add(new InboxMessage { EventId = evt.EventId });
        db.Notifications.Add(new UserNotification
        {
            UserId = evt.UserId,
            Type = "in_app",
            Content = $"Your contractor verification was rejected: {evt.Reason}",
            Read = false
        });

        await db.SaveChangesAsync(context.CancellationToken);
        await RetryDispatchAsync(logger, context.CancellationToken);
    }

    private static async Task RetryDispatchAsync(ILogger logger, CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Notification dispatch failed (attempt {Attempt})", attempt);
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, ct);
            }
        }
    }
}

