using BuildPay.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.NotificationService;

public sealed class ProjectMilestoneCompletedConsumer(NotificationDbContext db, ILogger<ProjectMilestoneCompletedConsumer> logger)
    : IConsumer<ProjectMilestoneCompleted>
{
    public async Task Consume(ConsumeContext<ProjectMilestoneCompleted> context)
    {
        var evt = context.Message;
        if (await db.InboxMessages.AnyAsync(x => x.EventId == evt.EventId, context.CancellationToken))
        {
            return;
        }

        // Without a user directory in this service, we can’t map project->customer here; this is scaffold-level.
        db.InboxMessages.Add(new InboxMessage { EventId = evt.EventId });
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

