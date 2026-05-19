using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildPay.Application.Common.Interfaces;
using BuildPay.Application.DTOs;
using BuildPay.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Contractors.Queries;

public sealed record GetApprovedContractorsQuery : IRequest<List<ContractorDto>>;

public sealed class GetApprovedContractorsQueryHandler : IRequestHandler<GetApprovedContractorsQuery, List<ContractorDto>>
{
    private readonly IBuildPayDbContext _context;

    public GetApprovedContractorsQueryHandler(IBuildPayDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContractorDto>> Handle(GetApprovedContractorsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Contractors
            .Where(c => c.VerificationStatus == VerificationStatus.Approved)
            .OrderByDescending(c => c.Rating)
            .Select(c => new ContractorDto
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User.FullName,
                LicenseNumber = c.LicenseNumber,
                Rating = c.Rating,
                VerificationStatus = c.VerificationStatus
            })
            .ToListAsync(cancellationToken);
    }
}
