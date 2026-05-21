using BuildPay.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BuildPay.PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public sealed class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _db;
    private readonly IPublishEndpoint _publish;

    public PaymentsController(PaymentDbContext db, IPublishEndpoint publish)
    {
        _db = db;
        _publish = publish;
    }

    [HttpPost("escrow/fund")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> Fund([FromBody] FundEscrowRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Funding amount must be greater than zero." });
        }

        var account = await _db.EscrowAccounts.FirstOrDefaultAsync(a => a.ProjectId == request.ProjectId, ct);
        if (account is null)
        {
            account = new EscrowAccount { ProjectId = request.ProjectId, Balance = 0 };
            _db.EscrowAccounts.Add(account);
        }

        account.Balance += request.Amount;
        account.UpdatedAtUtc = DateTime.UtcNow;

        var tx = new WalletTransaction
        {
            ProjectId = request.ProjectId,
            Amount = request.Amount,
            Direction = TransactionDirection.Credit
        };
        _db.WalletTransactions.Add(tx);

        await _db.SaveChangesAsync(ct);
        return Ok(new { transactionId = tx.Id });
    }

    [HttpPost("milestones/{milestoneId:guid}/request-release")]
    [Authorize(Roles = "Contractor,Admin")]
    public async Task<ActionResult> RequestRelease([FromRoute] Guid milestoneId, [FromBody] RequestReleaseRequest request, CancellationToken ct)
    {
        var escrow = await _db.MilestoneEscrows.FirstOrDefaultAsync(m => m.MilestoneId == milestoneId, ct);
        if (escrow is null)
        {
            escrow = new MilestoneEscrow
            {
                MilestoneId = milestoneId,
                ProjectId = request.ProjectId,
                ContractorUserId = GetUserId(),
                Amount = request.Amount,
                State = MilestoneEscrowState.Release_Requested
            };
            _db.MilestoneEscrows.Add(escrow);
        }
        else
        {
            escrow.State = MilestoneEscrowState.Release_Requested;
            escrow.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("milestones/{milestoneId:guid}/approve")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<object>> ApproveRelease([FromRoute] Guid milestoneId, [FromBody] ApproveReleaseRequest request, CancellationToken ct)
    {
        var account = await _db.EscrowAccounts.FirstOrDefaultAsync(a => a.ProjectId == request.ProjectId, ct);
        if (account is null)
        {
            return UnprocessableEntity(new { message = "Insufficient escrow balance to release this milestone." });
        }

        var escrow = await _db.MilestoneEscrows.FirstOrDefaultAsync(m => m.MilestoneId == milestoneId, ct);
        if (escrow is null)
        {
            return NotFound();
        }

        if (account.Balance < escrow.Amount)
        {
            return UnprocessableEntity(new { message = "Insufficient escrow balance to release this milestone." });
        }

        var previousState = escrow.State.ToString();

        account.Balance -= escrow.Amount;
        account.UpdatedAtUtc = DateTime.UtcNow;
        escrow.State = MilestoneEscrowState.Released;
        escrow.UpdatedAtUtc = DateTime.UtcNow;
        escrow.ContractorUserId = request.ContractorUserId;

        var debit = new WalletTransaction
        {
            ProjectId = request.ProjectId,
            MilestoneId = milestoneId,
            Amount = escrow.Amount,
            Direction = TransactionDirection.Debit
        };
        _db.WalletTransactions.Add(debit);

        _db.EscrowAuditLogs.Add(new EscrowAuditLog
        {
            MilestoneId = milestoneId,
            ActingUserId = GetUserId(),
            PreviousState = previousState,
            NewState = escrow.State.ToString(),
            Amount = escrow.Amount
        });

        await _db.SaveChangesAsync(ct);

        await _publish.Publish(PaymentMilestoneReleased.Create("BuildPay.PaymentService", request.ProjectId, milestoneId, request.ContractorUserId, escrow.Amount), ct);

        return Ok(new { transactionId = debit.Id });
    }

    [HttpGet("projects/{projectId:guid}/balance")]
    [Authorize(Roles = "Customer,Contractor,Admin")]
    public async Task<ActionResult<object>> Balance([FromRoute] Guid projectId, CancellationToken ct)
    {
        var account = await _db.EscrowAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.ProjectId == projectId, ct);
        return Ok(new { projectId, balance = account?.Balance ?? 0m });
    }

    [HttpGet("admin/transactions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<object>> Transactions([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken ct)
    {
        var query = _db.WalletTransactions.AsNoTracking().AsQueryable();
        if (fromUtc.HasValue) query = query.Where(t => t.CreatedAtUtc >= fromUtc.Value);
        if (toUtc.HasValue) query = query.Where(t => t.CreatedAtUtc <= toUtc.Value);

        var rows = await query.OrderByDescending(t => t.CreatedAtUtc).Take(500).ToListAsync(ct);
        return Ok(rows.Select(t => new { t.Id, t.ProjectId, t.MilestoneId, t.Amount, t.Direction, t.CreatedAtUtc }));
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    public sealed record FundEscrowRequest(Guid ProjectId, decimal Amount);
    public sealed record RequestReleaseRequest(Guid ProjectId, decimal Amount);
    public sealed record ApproveReleaseRequest(Guid ProjectId, Guid ContractorUserId);
}

