using BuildPay.Contracts;
using BuildPay.ServiceDefaults;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.DeliveryService.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public sealed class DeliveriesController : ControllerBase
{
    private readonly DeliveryDbContext _db;
    private readonly IPublishEndpoint _publish;
    private readonly IHubContext<DeliveryHub> _hub;

    public DeliveriesController(DeliveryDbContext db, IPublishEndpoint publish, IHubContext<DeliveryHub> hub)
    {
        _db = db;
        _publish = publish;
        _hub = hub;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Supplier,Driver,Customer")]
    public async Task<ActionResult<object>> Get([FromRoute] Guid id, CancellationToken ct)
    {
        var delivery = await _db.Deliveries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
        if (delivery is null)
        {
            return NotFound();
        }

        if (!CanReadDelivery(delivery))
        {
            return Forbid();
        }

        return Ok(new
        {
            id = delivery.Id,
            orderId = delivery.OrderId,
            status = delivery.Status,
            driverUserId = delivery.DriverUserId,
            pickupAddress = delivery.PickupAddress,
            dropoffAddress = delivery.DropoffAddress,
            deliveryFee = delivery.DeliveryFee,
            updatedAtUtc = delivery.UpdatedAtUtc
        });
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin,Supplier")]
    public async Task<ActionResult<object>> Assign([FromRoute] Guid id, [FromBody] AssignDriverRequest request, CancellationToken ct)
    {
        var delivery = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (delivery is null)
        {
            return NotFound();
        }

        delivery.DriverUserId = request.DriverUserId;
        delivery.PickupAddress = request.PickupAddress;
        delivery.DropoffAddress = request.DropoffAddress;
        delivery.DeliveryFee = request.DeliveryFee;
        delivery.Status = DeliveryStatus.Assigned;
        delivery.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(new { deliveryId = delivery.Id });
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var delivery = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (delivery is null)
        {
            return NotFound();
        }

        if (!UserContext.IsAdmin(User))
        {
            var caller = GetUserId();
            if (!delivery.DriverUserId.HasValue || delivery.DriverUserId.Value != caller)
            {
                return Forbid();
            }
        }

        delivery.Status = request.Status;
        delivery.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        if (request.Status == DeliveryStatus.In_Transit)
        {
            await _publish.Publish(DeliveryUpdated.Create("BuildPay.DeliveryService", delivery.Id, request.Status.ToString()), ct);
        }

        if (request.Status == DeliveryStatus.Delivered)
        {
            await _publish.Publish(DeliveryCompleted.Create("BuildPay.DeliveryService", delivery.Id, delivery.CustomerUserId), ct);
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/location")]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<ActionResult> Location([FromRoute] Guid id, [FromBody] LocationUpdateRequest request, CancellationToken ct)
    {
        var delivery = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (delivery is null)
        {
            return NotFound();
        }

        if (!UserContext.IsAdmin(User))
        {
            var caller = GetUserId();
            if (!delivery.DriverUserId.HasValue || delivery.DriverUserId.Value != caller)
            {
                return Forbid();
            }
        }

        var update = new DeliveryLocationUpdate
        {
            DeliveryId = id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            TimestampUtc = DateTime.UtcNow
        };
        _db.LocationUpdates.Add(update);
        await _db.SaveChangesAsync(ct);

        await _hub.Clients.Group($"delivery:{id}")
            .SendAsync("gps", new { deliveryId = id, lat = update.Latitude, lng = update.Longitude, timestampUtc = update.TimestampUtc }, ct);

        return NoContent();
    }

    // Internal service-to-service lookup used for authorization in other services (e.g., Marketplace driver status updates)
    // Not exposed via Nginx because it does not route /internal/*.
    [HttpGet("/internal/deliveries/by-order/{orderId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> ByOrder([FromRoute] Guid orderId, CancellationToken ct)
    {
        var delivery = await _db.Deliveries.AsNoTracking().FirstOrDefaultAsync(d => d.OrderId == orderId, ct);
        if (delivery is null) return NotFound();

        return Ok(new
        {
            deliveryId = delivery.Id,
            orderId = delivery.OrderId,
            driverUserId = delivery.DriverUserId,
            customerUserId = delivery.CustomerUserId,
            status = delivery.Status
        });
    }

    [HttpGet("/internal/stats/deliveries")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> DeliveriesCount(CancellationToken ct)
    {
        var count = await _db.Deliveries.CountAsync(ct);
        return Ok(new { count });
    }

    private Guid GetUserId() => UserContext.GetUserId(User);

    private bool CanReadDelivery(Delivery delivery)
    {
        if (UserContext.IsAdmin(User)) return true;

        var caller = GetUserId();
        if (caller == Guid.Empty) return false;

        // Supplier reads are allowed at scaffold-level (no supplier mapping exists on Delivery).
        if (User.IsInRole("Supplier")) return true;

        if (User.IsInRole("Customer") && delivery.CustomerUserId == caller) return true;
        if (User.IsInRole("Driver") && delivery.DriverUserId.HasValue && delivery.DriverUserId.Value == caller) return true;

        return false;
    }

    public sealed record AssignDriverRequest(Guid DriverUserId, string PickupAddress, string DropoffAddress, decimal? DeliveryFee);
    public sealed record UpdateStatusRequest(DeliveryStatus Status);
    public sealed record LocationUpdateRequest(decimal Latitude, decimal Longitude);
}
