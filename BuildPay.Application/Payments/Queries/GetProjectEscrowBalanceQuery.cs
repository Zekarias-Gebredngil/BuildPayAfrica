using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Domain.Enums;
using BuildPay.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Payments.Queries;

public sealed record GetProjectEscrowBalanceQuery(Guid ProjectId) : IRequest<decimal>;

public sealed class GetProjectEscrowBalanceQueryHandler : IRequestHandler<GetProjectEscrowBalanceQuery, decimal>
{
    private readonly BuildPayDbContext _context;

    public GetProjectEscrowBalanceQueryHandler(BuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> Handle(GetProjectEscrowBalanceQuery request, CancellationToken cancellationToken)
    {
        var deposits = await _context.Transactions
            .Where(t => t.ProjectId == request.ProjectId && t.Status == TransactionStatus.Completed && t.TransactionType == "EscrowDeposit")
            .SumAsync(t => t.Amount, cancellationToken);

        var releases = await _context.Transactions
            .Where(t => t.ProjectId == request.ProjectId && t.Status == TransactionStatus.Completed && t.TransactionType == "MilestoneRelease")
            .SumAsync(t => t.Amount, cancellationToken);

        return deposits - releases;
    }
}

