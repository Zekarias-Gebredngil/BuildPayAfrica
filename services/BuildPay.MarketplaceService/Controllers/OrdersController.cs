using BuildPay.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace BuildPay.MarketplaceService.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public sealed class OrdersController : ControllerBase
{
    private readonly MarketplaceDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publish;

    public OrdersController(MarketplaceDbContext db, IDistributedCache cache, IPublishEndpoint publish)
    {
        _db = db;
        _cache = cache;
        _publish = publish;
    }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> Place(CancellationToken ct)
    {
        var userId = GetUserId();
        var cart = await ReadCart(ct);
        if (cart.Items.Count == 0)
        {
            return BadRequest(new { message = "Cart is empty." });
        }

        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        var productById = products.ToDictionary(p => p.Id, p => p);

        foreach (var item in cart.Items)
        {
            if (!productById.TryGetValue(item.ProductId, out var product) || product.Quantity < item.Quantity)
            {
                var name = product?.Name ?? item.ProductId.ToString();
                return UnprocessableEntity(new { message = $"Insufficient stock for: {name}" });
            }
        }

        var order = new Order
        {
            CustomerUserId = userId,
            Status = OrderStatus.Pending
        };

        foreach (var item in cart.Items)
        {
            var p = productById[item.ProductId];
            p.Quantity -= item.Quantity;

            var lineTotal = p.Price * item.Quantity;
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = p.Id,
                ProductName = p.Name,
                Quantity = item.Quantity,
                UnitPrice = p.Price,
                LineTotal = lineTotal
            });
            order.TotalAmount += lineTotal;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CartKey(userId), ct);

        await _publish.Publish(OrderCreated.Create("BuildPay.MarketplaceService", order.Id, userId), ct);

        return Ok(new { orderId = order.Id });
    }

    [HttpGet("history")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> History(CancellationToken ct)
    {
        var userId = GetUserId();
        var rows = await _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.CustomerUserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

        return Ok(rows.Select(o => new
        {
            id = o.Id,
            status = o.Status,
            totalAmount = o.TotalAmount,
            createdAtUtc = o.CreatedAtUtc,
            items = o.Items.Select(i => new { i.ProductId, i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal }).ToList()
        }));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Supplier,Driver")]
    public async Task<ActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return NotFound();
        }

        order.Status = request.Status;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("/internal/stats/products")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> ProductsCount(CancellationToken ct)
    {
        var count = await _db.Products.CountAsync(ct);
        return Ok(new { count });
    }

    [HttpGet("/internal/stats/orders")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> OrdersCount(CancellationToken ct)
    {
        var count = await _db.Orders.CountAsync(ct);
        return Ok(new { count });
    }

    private sealed class CartState
    {
        public List<CartItem> Items { get; set; } = new();
    }

    private sealed class CartItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    private async Task<CartState> ReadCart(CancellationToken ct)
    {
        var raw = await _cache.GetStringAsync(CartKey(GetUserId()), ct);
        return string.IsNullOrWhiteSpace(raw) ? new CartState() : (JsonSerializer.Deserialize<CartState>(raw) ?? new CartState());
    }

    private static string CartKey(Guid userId) => $"cart:{userId}";

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    public sealed record UpdateStatusRequest(OrderStatus Status);
}
