using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.MarketplaceService.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly MarketplaceDbContext _db;

    public CategoriesController(MarketplaceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<object>> List(CancellationToken ct)
    {
        var rows = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        return Ok(rows.Select(c => new { id = c.Id, name = c.Name }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Supplier")]
    public async Task<ActionResult<object>> Create([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Name is required." });
        }

        var category = new Category { Name = request.Name.Trim() };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return Ok(new { categoryId = category.Id });
    }

    public sealed record CreateCategoryRequest(string Name);
}

