using System.Net;
using System.Net.Http.Json;

namespace BuildPay.MarketplaceService.Services;

public sealed class DeliveryLookupClient
{
    private readonly HttpClient _http;

    public DeliveryLookupClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(HttpStatusCode Status, DeliveryByOrderDto? Delivery)> GetByOrderAsync(Guid orderId, CancellationToken ct)
    {
        var res = await _http.GetAsync($"internal/deliveries/by-order/{orderId}", ct);
        if (!res.IsSuccessStatusCode)
        {
            return (res.StatusCode, null);
        }

        var dto = await res.Content.ReadFromJsonAsync<DeliveryByOrderDto>(cancellationToken: ct);
        return (res.StatusCode, dto);
    }
}

public sealed record DeliveryByOrderDto(Guid DeliveryId, Guid OrderId, Guid? DriverUserId, Guid CustomerUserId, string Status);
