using BuildPay.Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.DeliveryService;

public sealed class OrderCreatedConsumer(DeliveryDbContext db) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var evt = context.Message;

        if (await db.InboxMessages.AnyAsync(x => x.EventId == evt.EventId, context.CancellationToken))
        {
            return;
        }

        db.InboxMessages.Add(new InboxMessage { EventId = evt.EventId });

        var existing = await db.Deliveries.FirstOrDefaultAsync(d => d.OrderId == evt.OrderId, context.CancellationToken);
        if (existing is null)
        {
            db.Deliveries.Add(new Delivery
            {
                OrderId = evt.OrderId,
                CustomerUserId = evt.CustomerUserId,
                Status = DeliveryStatus.Pending_Assignment
            });
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}

