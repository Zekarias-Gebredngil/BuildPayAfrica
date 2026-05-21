using Microsoft.EntityFrameworkCore;

namespace BuildPay.IdentityService;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<ContractorVerification> ContractorVerifications => Set<ContractorVerification>();
    public DbSet<ContractorVerificationDocument> ContractorVerificationDocuments => Set<ContractorVerificationDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractorVerification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.LicenceNumber).HasMaxLength(200);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            b.HasMany(x => x.Documents)
                .WithOne()
                .HasForeignKey(d => d.ContractorVerificationId);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<ContractorVerificationDocument>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Url).HasMaxLength(2048);
        });
    }
}

public enum ContractorVerificationStatus
{
    Pending,
    Approved,
    Rejected
}

public sealed class ContractorVerification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string LicenceNumber { get; set; } = string.Empty;
    public ContractorVerificationStatus Status { get; set; } = ContractorVerificationStatus.Pending;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public List<ContractorVerificationDocument> Documents { get; set; } = new();
}

public sealed class ContractorVerificationDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ContractorVerificationId { get; set; }
    public string Url { get; set; } = string.Empty;
}

