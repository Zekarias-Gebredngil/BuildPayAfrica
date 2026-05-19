using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Deliveries.Commands;

public sealed record AddDeliveryLocationUpdateCommand(Guid DeliveryId, decimal Latitude, decimal Longitude) : IRequest<Guid>;

public sealed class AddDeliveryLocationUpdateCommandValidator : AbstractValidator<AddDeliveryLocationUpdateCommand>
{
    public AddDeliveryLocationUpdateCommandValidator()
    {
        RuleFor(v => v.DeliveryId).NotEmpty();
        RuleFor(v => v.Latitude).InclusiveBetween(-90, 90);
        RuleFor(v => v.Longitude).InclusiveBetween(-180, 180);
    }
}

public sealed class AddDeliveryLocationUpdateCommandHandler : IRequestHandler<AddDeliveryLocationUpdateCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public AddDeliveryLocationUpdateCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddDeliveryLocationUpdateCommand request, CancellationToken cancellationToken)
    {
        var deliveryExists = await _context.Deliveries.AnyAsync(d => d.Id == request.DeliveryId, cancellationToken);
        if (!deliveryExists)
        {
            throw new ValidationException("Delivery not found.");
        }

        var entity = new DeliveryLocationUpdate
        {
            Id = Guid.NewGuid(),
            DeliveryId = request.DeliveryId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        _context.DeliveryLocationUpdates.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
