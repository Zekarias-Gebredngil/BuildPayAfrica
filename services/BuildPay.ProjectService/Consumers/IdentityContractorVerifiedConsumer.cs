using BuildPay.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.ProjectService;

public sealed class IdentityContractorVerifiedConsumer(ProjectDbContext db) : IConsumer<IdentityContractorVerified>
{
    public async Task Consume(ConsumeContext<IdentityContractorVerified> context)
    {
        var evt = context.Message;

        var already = await db.InboxMessages.AnyAsync(x => x.EventId == evt.EventId, context.CancellationToken);
        if (already)
        {
            return;
        }

        db.InboxMessages.Add(new InboxMessage { EventId = evt.EventId });

        var record = await db.VerifiedContractors.FindAsync([evt.UserId], context.CancellationToken);
        if (record is null)
        {
            db.VerifiedContractors.Add(new VerifiedContractor
            {
                UserId = evt.UserId,
                ContractorVerificationId = evt.ContractorVerificationId
            });
        }
        else
        {
            record.ContractorVerificationId = evt.ContractorVerificationId;
            record.VerifiedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}

