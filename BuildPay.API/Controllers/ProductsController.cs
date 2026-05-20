using BuildPay.API.Security;
using BuildPay.Application.DTOs;
using BuildPay.Application.Products.Commands;
using BuildPay.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Supplier,Admin")]
    public async Task<ActionResult<object>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var supplierId = User.GetUserId();

        var id = await _mediator.Send(
            new CreateProductCommand(
                supplierId,
                request.Name,
                request.Price,
                request.Quantity,
                request.CategoryId,
                request.ImageUrl ?? string.Empty),
            cancellationToken);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    public sealed record CreateProductRequest(
        string Name,
        decimal Price,
        int Quantity,
        Guid CategoryId,
        string? ImageUrl);
}

