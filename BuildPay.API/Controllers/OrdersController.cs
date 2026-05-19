using BuildPay.API.Security;
using BuildPay.Application.DTOs;
using BuildPay.Application.Orders.Commands;
using BuildPay.Application.Orders.Queries;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var customerId = User.GetUserId();

        var id = await _mediator.Send(
            new CreateOrderCommand(
                customerId,
                request.Items.Select(i => new CreateOrderItem(i.ProductId, i.Quantity)).ToList()),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<OrderDto>>> GetMy(CancellationToken cancellationToken)
    {
        var customerId = User.GetUserId();
        var result = await _mediator.Send(new GetMyOrdersQuery(customerId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin,Supplier,Driver")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await _mediator.Send(new UpdateOrderStatusCommand(id, request.Status), cancellationToken);
        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    public sealed record CreateOrderRequest(List<CreateOrderItemRequest> Items);

    public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

    public sealed record UpdateOrderStatusRequest(OrderStatus Status);
}

