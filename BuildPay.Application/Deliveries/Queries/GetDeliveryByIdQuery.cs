using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Deliveries.Queries;

public sealed record GetDeliveryByIdQuery(Guid DeliveryId) : IRequest<DeliveryDto?>;

public sealed class GetDeliveryByIdQueryHandler : IRequestHandler<GetDeliveryByIdQuery, DeliveryDto?>
{
    private readonly IBuildPayDbContext _context;

    public GetDeliveryByIdQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<DeliveryDto?> Handle(GetDeliveryByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Deliveries
            .Where(d => d.Id == request.DeliveryId)
            .Select(d => new DeliveryDto
            {
                Id = d.Id,
                OrderId = d.OrderId,
                DriverId = d.DriverId,
                Status = d.Status,
                DeliveryFee = d.DeliveryFee,
                PickupAddress = d.PickupAddress,
                DropoffAddress = d.DropoffAddress,
                AssignedAt = d.AssignedAt,
                DeliveredAt = d.DeliveredAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
