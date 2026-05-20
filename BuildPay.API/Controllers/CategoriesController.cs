using BuildPay.Application.Categories.Commands;
using BuildPay.Application.Categories.Queries;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Supplier")]
    public async Task<ActionResult<object>> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateCategoryCommand(request.Name, request.Description), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    public sealed record CreateCategoryRequest(string Name, string? Description);
}

