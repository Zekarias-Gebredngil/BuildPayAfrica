using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Milestones.Commands;

public sealed record AddMilestoneCommand(Guid ProjectId, string Title, decimal Amount, DateTime DueDate) : IRequest<Guid>;

public sealed class AddMilestoneCommandValidator : AbstractValidator<AddMilestoneCommand>
{
    public AddMilestoneCommandValidator()
    {
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Amount).GreaterThan(0);
    }
}

public sealed class AddMilestoneCommandHandler : IRequestHandler<AddMilestoneCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public AddMilestoneCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddMilestoneCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new ValidationException("Project not found.");
        }

        var existing = await _context.Milestones
            .Where(m => m.ProjectId == request.ProjectId)
            .SumAsync(m => m.Amount, cancellationToken);

        if (existing + request.Amount > project.Budget)
        {
            throw new ValidationException("Total milestone amounts exceed project budget.");
        }

        var entity = new Milestone
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Title = request.Title.Trim(),
            Amount = request.Amount,
            Status = MilestoneStatus.Pending,
            DueDate = request.DueDate
        };

        _context.Milestones.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
