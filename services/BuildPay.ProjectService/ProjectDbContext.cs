using Microsoft.EntityFrameworkCore;

namespace BuildPay.ProjectService;

public sealed class ProjectDbContext(DbContextOptions<ProjectDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<ProjectExpense> Expenses => Set<ProjectExpense>();
    public DbSet<ProjectPhoto> Photos => Set<ProjectPhoto>();
    public DbSet<VerifiedContractor> VerifiedContractors => Set<VerifiedContractor>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.Budget).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.CustomerUserId);
        });

        modelBuilder.Entity<Milestone>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<ProjectExpense>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<ProjectPhoto>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Url).HasMaxLength(2048);
            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<VerifiedContractor>(b =>
        {
            b.HasKey(x => x.UserId);
        });

        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.HasKey(x => x.EventId);
        });
    }
}

public enum ProjectStatus
{
    Draft,
    Active,
    Completed,
    Cancelled
}

public enum MilestoneStatus
{
    Pending,
    Completed,
    Approved
}

public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerUserId { get; set; }
    public Guid? ContractorUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class Milestone
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDateUtc { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
}

public sealed class ProjectExpense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ProjectPhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class VerifiedContractor
{
    public Guid UserId { get; set; }
    public Guid ContractorVerificationId { get; set; }
    public DateTime VerifiedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}

