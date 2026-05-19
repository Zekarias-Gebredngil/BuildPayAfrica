using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Expenses.Commands;

public sealed record AddExpenseCommand(Guid ProjectId, string Title, decimal Amount, string? ReceiptUrl) : IRequest<Guid>;

public sealed class AddExpenseCommandValidator : AbstractValidator<AddExpenseCommand>
{
    public AddExpenseCommandValidator()
    {
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Amount).GreaterThan(0);
        RuleFor(v => v.ReceiptUrl).MaximumLength(1000);
    }
}

public sealed class AddExpenseCommandHandler : IRequestHandler<AddExpenseCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public AddExpenseCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddExpenseCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new ValidationException("Project not found.");
        }

        var entity = new Expense
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Amount = request.Amount,
            ReceiptUrl = string.IsNullOrWhiteSpace(request.ReceiptUrl) ? null : request.ReceiptUrl.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Expenses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
