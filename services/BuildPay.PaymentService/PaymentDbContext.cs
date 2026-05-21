using Microsoft.EntityFrameworkCore;

namespace BuildPay.PaymentService;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<EscrowAccount> EscrowAccounts => Set<EscrowAccount>();
    public DbSet<MilestoneEscrow> MilestoneEscrows => Set<MilestoneEscrow>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<EscrowAuditLog> EscrowAuditLogs => Set<EscrowAuditLog>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EscrowAccount>(b =>
        {
            b.HasKey(x => x.ProjectId);
            b.Property(x => x.Balance).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<MilestoneEscrow>(b =>
        {
            b.HasKey(x => x.MilestoneId);
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.State).HasConversion<string>().HasMaxLength(50);
            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<WalletTransaction>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<EscrowAuditLog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.PreviousState).HasMaxLength(50);
            b.Property(x => x.NewState).HasMaxLength(50);
            b.HasIndex(x => x.MilestoneId);
        });

        modelBuilder.Entity<InboxMessage>(b => b.HasKey(x => x.EventId));
    }
}

public enum MilestoneEscrowState
{
    None,
    Funds_Held,
    Release_Requested,
    Released
}

public sealed class EscrowAccount
{
    public Guid ProjectId { get; set; }
    public decimal Balance { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class MilestoneEscrow
{
    public Guid MilestoneId { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ContractorUserId { get; set; }
    public decimal Amount { get; set; }
    public MilestoneEscrowState State { get; set; } = MilestoneEscrowState.None;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum TransactionDirection
{
    Credit,
    Debit
}

public sealed class WalletTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public decimal Amount { get; set; }
    public TransactionDirection Direction { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class EscrowAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MilestoneId { get; set; }
    public Guid ActingUserId { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string PreviousState { get; set; } = string.Empty;
    public string NewState { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}

