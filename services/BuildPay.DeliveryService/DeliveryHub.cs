using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BuildPay.DeliveryService;

[Authorize]
public sealed class DeliveryHub : Hub
{
    public Task SubscribeToDelivery(string deliveryId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"delivery:{deliveryId}");
}

