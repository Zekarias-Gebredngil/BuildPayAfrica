using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BuildPay.ServiceDefaults;

namespace BuildPay.MarketplaceService.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public ProductsController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<object>> List([FromQuery] Guid? categoryId, CancellationToken ct)
    {
        var query = _db.Products.AsNoTracking().Where(p => p.IsActive);
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var rows = await query
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                quantity = p.Quantity,
                categoryId = p.CategoryId,
                imageUrl = p.ImageUrl
            })
            .ToListAsync(ct);

        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Roles = "Supplier,Admin")]
    public async Task<ActionResult<object>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required." });
        }

        if (request.Price <= 0 || request.Quantity < 0)
        {
            return BadRequest(new { message = "Invalid price or quantity." });
        }

        var supplierUserId = GetUserId();
        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Quantity = request.Quantity,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl,
            SupplierUserId = supplierUserId,
            IsActive = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return Ok(new { productId = product.Id });
    }

    private Guid GetUserId()
    {
        return UserContext.GetUserId(User);
    }

    public sealed record CreateProductRequest(string Name, decimal Price, int Quantity, Guid CategoryId, string? ImageUrl);
}

