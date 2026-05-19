using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Milestones.Commands;

public sealed record UpdateMilestoneStatusCommand(Guid MilestoneId, MilestoneStatus Status) : IRequest<bool>;

public sealed class UpdateMilestoneStatusCommandValidator : AbstractValidator<UpdateMilestoneStatusCommand>
{
    public UpdateMilestoneStatusCommandValidator()
    {
        RuleFor(v => v.MilestoneId).NotEmpty();
    }
}

public sealed class UpdateMilestoneStatusCommandHandler : IRequestHandler<UpdateMilestoneStatusCommand, bool>
{
    private readonly IBuildPayDbContext _context;

    public UpdateMilestoneStatusCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateMilestoneStatusCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _context.Milestones.FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);
        if (milestone is null)
        {
            return false;
        }

        milestone.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
