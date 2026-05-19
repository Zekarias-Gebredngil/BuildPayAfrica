using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Deliveries.Commands;

public sealed record UpdateDeliveryStatusCommand(Guid DeliveryId, DeliveryStatus Status) : IRequest<bool>;

public sealed class UpdateDeliveryStatusCommandValidator : AbstractValidator<UpdateDeliveryStatusCommand>
{
    public UpdateDeliveryStatusCommandValidator()
    {
        RuleFor(v => v.DeliveryId).NotEmpty();
    }
}

public sealed class UpdateDeliveryStatusCommandHandler : IRequestHandler<UpdateDeliveryStatusCommand, bool>
{
    private readonly IBuildPayDbContext _context;

    public UpdateDeliveryStatusCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateDeliveryStatusCommand request, CancellationToken cancellationToken)
    {
        var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == request.DeliveryId, cancellationToken);
        if (delivery is null)
        {
            return false;
        }

        delivery.Status = request.Status;
        if (request.Status == DeliveryStatus.Delivered)
        {
            delivery.DeliveredAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
