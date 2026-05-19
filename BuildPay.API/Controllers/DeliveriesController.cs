using BuildPay.Application.Deliveries.Commands;
using BuildPay.Application.Deliveries.Queries;
using BuildPay.Application.DTOs;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public sealed class DeliveriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Supplier")]
    public async Task<ActionResult<object>> Assign([FromBody] AssignDeliveryRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new AssignDeliveryCommand(
                request.OrderId,
                request.DriverId,
                request.DeliveryFee,
                request.PickupAddress,
                request.DropoffAddress),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeliveryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeliveryByIdQuery(id), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var ok = await _mediator.Send(new UpdateDeliveryStatusCommand(id, request.Status), cancellationToken);
        if (!ok)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/location")]
    [Authorize(Roles = "Driver,Admin")]
    public async Task<ActionResult<object>> AddLocation(Guid id, [FromBody] AddLocationRequest request, CancellationToken cancellationToken)
    {
        var updateId = await _mediator.Send(new AddDeliveryLocationUpdateCommand(id, request.Latitude, request.Longitude), cancellationToken);
        return Ok(new { locationUpdateId = updateId });
    }

    public sealed record AssignDeliveryRequest(
        Guid OrderId,
        Guid DriverId,
        decimal? DeliveryFee,
        string? PickupAddress,
        string? DropoffAddress);

    public sealed record UpdateStatusRequest(DeliveryStatus Status);
    public sealed record AddLocationRequest(decimal Latitude, decimal Longitude);
}

