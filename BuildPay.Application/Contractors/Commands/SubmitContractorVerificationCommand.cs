using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Domain.Entities;
using BuildPay.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Contractors.Commands;

public sealed record SubmitContractorVerificationCommand(
    Guid UserId,
    string LicenseNumber,
    List<SubmitContractorDocument> Documents) : IRequest<Guid>;

public sealed record SubmitContractorDocument(ContractorDocumentType DocumentType, string FileUrl);

public sealed class SubmitContractorVerificationCommandValidator : AbstractValidator<SubmitContractorVerificationCommand>
{
    public SubmitContractorVerificationCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        RuleFor(v => v.LicenseNumber).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Documents).NotEmpty();
        RuleForEach(v => v.Documents).ChildRules(d =>
        {
            d.RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(1000);
        });
    }
}

public sealed class SubmitContractorVerificationCommandHandler : IRequestHandler<SubmitContractorVerificationCommand, Guid>
{
    private readonly IBuildPayDbContext _context;

    public SubmitContractorVerificationCommandHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubmitContractorVerificationCommand request, CancellationToken cancellationToken)
    {
        var contractor = await _context.Contractors
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (contractor is null)
        {
            contractor = new Contractor
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                LicenseNumber = request.LicenseNumber.Trim(),
                Rating = 0,
                VerificationStatus = VerificationStatus.Pending
            };
            _context.Contractors.Add(contractor);
        }
        else
        {
            contractor.LicenseNumber = request.LicenseNumber.Trim();
            contractor.VerificationStatus = VerificationStatus.Pending;
        }

        contractor.Documents.Clear();
        contractor.Documents.AddRange(request.Documents.Select(d => new ContractorDocument
        {
            Id = Guid.NewGuid(),
            ContractorId = contractor.Id,
            DocumentType = d.DocumentType,
            FileUrl = d.FileUrl.Trim(),
            CreatedAt = DateTime.UtcNow
        }));

        await _context.SaveChangesAsync(cancellationToken);
        return contractor.Id;
    }
}
