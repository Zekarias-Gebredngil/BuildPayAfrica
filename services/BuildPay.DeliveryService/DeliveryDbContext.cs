using Microsoft.EntityFrameworkCore;

namespace BuildPay.DeliveryService;

public sealed class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : DbContext(options)
{
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryLocationUpdate> LocationUpdates => Set<DeliveryLocationUpdate>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Delivery>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.DeliveryFee).HasColumnType("decimal(18,2)");
            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DeliveryLocationUpdate>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Latitude).HasColumnType("decimal(9,6)");
            b.Property(x => x.Longitude).HasColumnType("decimal(9,6)");
            b.HasIndex(x => x.DeliveryId);
        });

        modelBuilder.Entity<InboxMessage>(b => b.HasKey(x => x.EventId));
    }
}

public enum DeliveryStatus
{
    Pending_Assignment,
    Assigned,
    In_Transit,
    Delivered
}

public sealed class Delivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid CustomerUserId { get; set; }
    public Guid? DriverUserId { get; set; }
    public string? PickupAddress { get; set; }
    public string? DropoffAddress { get; set; }
    public decimal? DeliveryFee { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending_Assignment;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class DeliveryLocationUpdate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DeliveryId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}

