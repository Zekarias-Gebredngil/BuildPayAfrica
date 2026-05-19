using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Projects.Commands;

public sealed record AssignContractorCommand(Guid ProjectId, Guid ContractorId) : IRequest<bool>;

public sealed class AssignContractorCommandValidator : AbstractValidator<AssignContractorCommand>
{
    public AssignContractorCommandValidator()
    {
        RuleFor(v => v.ProjectId).NotEmpty();
        RuleFor(v => v.ContractorId).NotEmpty();
    }
}

public sealed class AssignContractorCommandHandler : IRequestHandler<AssignContractorCommand, bool>
{
    private readonly IBuildPayDbContext _context;

    public AssignContractorCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AssignContractorCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);
        if (project is null)
        {
            return false;
        }

        var contractor = await _context.Contractors.FirstOrDefaultAsync(c => c.Id == request.ContractorId, cancellationToken);
        if (contractor is null)
        {
            throw new ValidationException("Contractor not found.");
        }

        project.ContractorId = contractor.Id;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
