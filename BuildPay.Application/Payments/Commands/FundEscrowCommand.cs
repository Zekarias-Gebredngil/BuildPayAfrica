using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using BuildPay.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Payments.Commands;

public sealed record FundEscrowCommand(Guid ActorUserId, Guid ProjectId, decimal Amount) : IRequest<Guid>;

public sealed class FundEscrowCommandValidator : AbstractValidator<FundEscrowCommand>
{
    public FundEscrowCommandValidator()
    {
        RuleFor(v => v.ActorUserId).NotEmpty();
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.Amount).GreaterThan(0);
    }
}

public sealed class FundEscrowCommandHandler : IRequestHandler<FundEscrowCommand, Guid>
{
    private readonly BuildPayDbContext _context;

    public FundEscrowCommandHandler(BuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(FundEscrowCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new ValidationException("Project not found.");
        }

        if (project.CustomerId != request.ActorUserId)
        {
            throw new ValidationException("You can only fund escrow for your own project.");
        }

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Amount = request.Amount,
            Status = TransactionStatus.Completed,
            TransactionType = "EscrowDeposit",
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(tx);
        await _context.SaveChangesAsync(cancellationToken);
        return tx.Id;
    }
}

