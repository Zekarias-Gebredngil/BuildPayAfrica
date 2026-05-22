using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using BuildPay.ServiceDefaults;
using System.Text.Json;

namespace BuildPay.MarketplaceService.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public sealed class CartController : ControllerBase
{
    private static readonly DistributedCacheEntryOptions CartTtl = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    };

    private readonly IDistributedCache _cache;

    public CartController(IDistributedCache cache)
    {
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<object>> Get(CancellationToken ct)
    {
        var state = await Read(ct);
        return Ok(new { items = state.Items });
    }

    [HttpPost("items")]
    public async Task<ActionResult> Add([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { message = "Quantity must be greater than zero." });
        }

        var state = await Read(ct);
        var existing = state.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existing is null)
        {
            state.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }
        else
        {
            existing.Quantity += request.Quantity;
        }

        await Write(state, ct);
        return NoContent();
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult> Remove([FromRoute] Guid productId, CancellationToken ct)
    {
        var state = await Read(ct);
        state.Items.RemoveAll(i => i.ProductId == productId);
        await Write(state, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> Clear(CancellationToken ct)
    {
        await _cache.RemoveAsync(Key(), ct);
        return NoContent();
    }

    private string Key() => $"cart:{GetUserId()}";

    private async Task<CartState> Read(CancellationToken ct)
    {
        var raw = await _cache.GetStringAsync(Key(), ct);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new CartState();
        }

        return JsonSerializer.Deserialize<CartState>(raw) ?? new CartState();
    }

    private Task Write(CartState state, CancellationToken ct) =>
        _cache.SetStringAsync(Key(), JsonSerializer.Serialize(state), CartTtl, ct);

    private Guid GetUserId()
    {
        return UserContext.GetUserId(User);
    }

    public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

    public sealed class CartState
    {
        public List<CartItem> Items { get; set; } = new();
    }

    public sealed class CartItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

