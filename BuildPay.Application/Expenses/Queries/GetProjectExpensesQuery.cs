using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Expenses.Queries;

public sealed record GetProjectExpensesQuery(Guid ProjectId) : IRequest<List<ExpenseDto>>;

public sealed class GetProjectExpensesQueryHandler : IRequestHandler<GetProjectExpensesQuery, List<ExpenseDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetProjectExpensesQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExpenseDto>> Handle(GetProjectExpensesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Expenses
            .Where(e => e.ProjectId == request.ProjectId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ProjectId = e.ProjectId,
                Title = e.Title,
                Amount = e.Amount,
                ReceiptUrl = e.ReceiptUrl,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
