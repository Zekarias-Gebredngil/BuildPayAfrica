using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Deliveries.Commands;

public sealed record AssignDeliveryCommand(
    Guid OrderId,
    Guid DriverId,
    decimal? DeliveryFee,
    string? PickupAddress,
    string? DropoffAddress) : IRequest<Guid>;

public sealed class AssignDeliveryCommandValidator : AbstractValidator<AssignDeliveryCommand>
{
    public AssignDeliveryCommandValidator()
    {
        RuleFor(v => v.OrderId).NotEmpty();
        RuleFor(v => v.DriverId).NotEmpty();
        RuleFor(v => v.PickupAddress).MaximumLength(300);
        RuleFor(v => v.DropoffAddress).MaximumLength(300);
    }
}

public sealed class AssignDeliveryCommandHandler : IRequestHandler<AssignDeliveryCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public AssignDeliveryCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignDeliveryCommand request, CancellationToken cancellationToken)
    {
        var orderExists = await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken);
        if (!orderExists)
        {
            throw new ValidationException("Order not found.");
        }

        var driverExists = await _context.Users.AnyAsync(u => u.Id == request.DriverId, cancellationToken);
        if (!driverExists)
        {
            throw new ValidationException("Driver not found.");
        }

        var entity = new Delivery
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            DriverId = request.DriverId,
            Status = DeliveryStatus.Assigned,
            DeliveryFee = request.DeliveryFee,
            PickupAddress = request.PickupAddress,
            DropoffAddress = request.DropoffAddress,
            AssignedAt = DateTime.UtcNow
        };

        _context.Deliveries.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
