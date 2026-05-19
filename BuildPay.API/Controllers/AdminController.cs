using BuildPay.Domain.Enums;
using BuildPay.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController : ControllerBase
{
    private readonly BuildPayDbContext _context;

    public AdminController(BuildPayDbContext context)
    {
        _context = context;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<object>> Overview(CancellationToken cancellationToken)
    {
        var users = await _context.Users.CountAsync(cancellationToken);
        var contractorsPending = await _context.Contractors.CountAsync(c => c.VerificationStatus == VerificationStatus.Pending, cancellationToken);
        var products = await _context.Products.CountAsync(cancellationToken);
        var orders = await _context.Orders.CountAsync(cancellationToken);
        var projects = await _context.Projects.CountAsync(cancellationToken);
        var deliveries = await _context.Deliveries.CountAsync(cancellationToken);

        return Ok(new
        {
            users,
            contractorsPending,
            products,
            orders,
            projects,
            deliveries
        });
    }

    [HttpGet("contractors/pending")]
    public async Task<ActionResult<object>> PendingContractors(CancellationToken cancellationToken)
    {
        var pending = await _context.Contractors
            .Where(c => c.VerificationStatus == VerificationStatus.Pending)
            .Select(c => new
            {
                c.Id,
                c.UserId,
                c.LicenseNumber,
                c.Rating,
                c.VerificationStatus
            })
            .ToListAsync(cancellationToken);

        return Ok(pending);
    }
}

