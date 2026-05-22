using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BuildPay.ServiceDefaults;

namespace BuildPay.DeliveryService;

[Authorize]
public sealed class DeliveryHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DeliveryHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SubscribeToDelivery(string deliveryId)
    {
        if (!Guid.TryParse(deliveryId, out var id))
        {
            throw new HubException("Invalid deliveryId.");
        }

        var user = Context.User;
        if (user is null)
        {
            throw new HubException("Authentication required.");
        }

        if (UserContext.IsAdmin(user))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"delivery:{deliveryId}");
            return;
        }

        var caller = UserContext.GetUserId(user);
        if (caller == Guid.Empty)
        {
            throw new HubException("Authentication required.");
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();

        var delivery = await db.Deliveries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        if (delivery is null)
        {
            throw new HubException("Delivery not found.");
        }

        var canJoin =
            (user.IsInRole("Customer") && delivery.CustomerUserId == caller) ||
            (user.IsInRole("Driver") && delivery.DriverUserId.HasValue && delivery.DriverUserId.Value == caller);

        if (!canJoin)
        {
            throw new HubException("You do not have permission to subscribe to this delivery.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"delivery:{deliveryId}");
    }
}

