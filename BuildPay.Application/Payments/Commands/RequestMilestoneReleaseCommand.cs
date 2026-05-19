using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Domain.Enums;
using BuildPay.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Payments.Commands;

public sealed record RequestMilestoneReleaseCommand(Guid ActorUserId, Guid MilestoneId) : IRequest<bool>;

public sealed class RequestMilestoneReleaseCommandValidator : AbstractValidator<RequestMilestoneReleaseCommand>
{
    public RequestMilestoneReleaseCommandValidator()
    {
        RuleFor(v => v.ActorUserId).NotEmpty();
        RuleFor(v => v.MilestoneId).NotEmpty();
    }
}

public sealed class RequestMilestoneReleaseCommandHandler : IRequestHandler<RequestMilestoneReleaseCommand, bool>
{
    private readonly BuildPayDbContext _context;

    public RequestMilestoneReleaseCommandHandler(BuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RequestMilestoneReleaseCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Project)
            .ThenInclude(p => p.Contractor)
            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

        if (milestone is null)
        {
            return false;
        }

        if (milestone.Project.Contractor is null)
        {
            throw new ValidationException("Project has no assigned contractor.");
        }

        if (milestone.Project.Contractor.UserId != request.ActorUserId)
        {
            throw new ValidationException("Only the assigned contractor can request payment release.");
        }

        milestone.Status = MilestoneStatus.Completed;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
