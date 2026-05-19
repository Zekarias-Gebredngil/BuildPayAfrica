using System;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Contractors.Commands;

public sealed record SetContractorVerificationStatusCommand(Guid ContractorId, VerificationStatus Status) : IRequest<bool>;

public sealed class SetContractorVerificationStatusCommandValidator : AbstractValidator<SetContractorVerificationStatusCommand>
{
    public SetContractorVerificationStatusCommandValidator()
    {
        RuleFor(v => v.ContractorId).NotEmpty();
    }
}

public sealed class SetContractorVerificationStatusCommandHandler : IRequestHandler<SetContractorVerificationStatusCommand, bool>
{
    private readonly IBuildPayDbContext _context;

    public SetContractorVerificationStatusCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SetContractorVerificationStatusCommand request, CancellationToken cancellationToken)
    {
        var contractor = await _context.Contractors.FirstOrDefaultAsync(c => c.Id == request.ContractorId, cancellationToken);
        if (contractor is null)
        {
            return false;
        }

        contractor.VerificationStatus = request.Status;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
