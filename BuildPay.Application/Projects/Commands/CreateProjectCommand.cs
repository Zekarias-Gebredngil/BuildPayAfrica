using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;

namespace BuildPay.Application.Projects.Commands;

public sealed record CreateProjectCommand(Guid CustomerId, string Name, decimal Budget) : IRequest<Guid>;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(v => v.CustomerId).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Budget).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public CreateProjectCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var entity = new Project
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Name = request.Name.Trim(),
            Budget = request.Budget,
            Status = ProjectStatus.Planning
        };

        _context.Projects.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
