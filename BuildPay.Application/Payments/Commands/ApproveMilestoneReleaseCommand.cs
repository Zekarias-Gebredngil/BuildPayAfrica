using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using BuildPay.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Payments.Commands;

public sealed record ApproveMilestoneReleaseCommand(Guid ActorUserId, Guid MilestoneId) : IRequest<Guid>;

public sealed class ApproveMilestoneReleaseCommandValidator : AbstractValidator<ApproveMilestoneReleaseCommand>
{
    public ApproveMilestoneReleaseCommandValidator()
    {
        RuleFor(v => v.ActorUserId).NotEmpty();
        RuleFor(v => v.MilestoneId).NotEmpty();
    }
}

public sealed class ApproveMilestoneReleaseCommandHandler : IRequestHandler<ApproveMilestoneReleaseCommand, Guid>
{
    private readonly BuildPayDbContext _context;

    public ApproveMilestoneReleaseCommandHandler(BuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ApproveMilestoneReleaseCommand request, CancellationToken cancellationToken)
    {
        var milestone = await _context.Milestones
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == request.MilestoneId, cancellationToken);

        if (milestone is null)
        {
            throw new ValidationException("Milestone not found.");
        }

        if (milestone.Project.CustomerId != request.ActorUserId)
        {
            throw new ValidationException("Only the project owner can approve milestone release.");
        }

        if (milestone.Status != MilestoneStatus.Completed && milestone.Status != MilestoneStatus.Approved)
        {
            throw new ValidationException("Milestone must be marked completed before approval.");
        }

        var balance = await GetProjectEscrowBalance(_context, milestone.ProjectId, cancellationToken);
        if (balance < milestone.Amount)
        {
            throw new ValidationException("Insufficient escrow balance for this milestone.");
        }

        milestone.Status = MilestoneStatus.Paid;

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            ProjectId = milestone.ProjectId,
            MilestoneId = milestone.Id,
            Amount = milestone.Amount,
            Status = TransactionStatus.Completed,
            TransactionType = "MilestoneRelease",
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(tx);
        await _context.SaveChangesAsync(cancellationToken);
        return tx.Id;
    }

    private static async Task<decimal> GetProjectEscrowBalance(BuildPayDbContext context, Guid projectId, CancellationToken cancellationToken)
    {
        var deposits = await context.Transactions
            .Where(t => t.ProjectId == projectId && t.Status == TransactionStatus.Completed && t.TransactionType == "EscrowDeposit")
            .SumAsync(t => t.Amount, cancellationToken);

        var releases = await context.Transactions
            .Where(t => t.ProjectId == projectId && t.Status == TransactionStatus.Completed && t.TransactionType == "MilestoneRelease")
            .SumAsync(t => t.Amount, cancellationToken);

        return deposits - releases;
    }
}

