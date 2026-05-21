using BuildPay.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.PaymentService;

public sealed class ProjectMilestoneCompletedConsumer(PaymentDbContext db) : IConsumer<ProjectMilestoneCompleted>
{
    public async Task Consume(ConsumeContext<ProjectMilestoneCompleted> context)
    {
        var evt = context.Message;

        if (await db.InboxMessages.AnyAsync(x => x.EventId == evt.EventId, context.CancellationToken))
        {
            return;
        }

        db.InboxMessages.Add(new InboxMessage { EventId = evt.EventId });

        var escrow = await db.MilestoneEscrows.FirstOrDefaultAsync(x => x.MilestoneId == evt.MilestoneId, context.CancellationToken);
        if (escrow is null)
        {
            escrow = new MilestoneEscrow
            {
                MilestoneId = evt.MilestoneId,
                ProjectId = evt.ProjectId,
                Amount = evt.Amount,
                State = MilestoneEscrowState.Release_Requested
            };
            db.MilestoneEscrows.Add(escrow);
        }
        else
        {
            escrow.State = MilestoneEscrowState.Release_Requested;
            escrow.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}

