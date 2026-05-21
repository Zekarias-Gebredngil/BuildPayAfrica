using Microsoft.EntityFrameworkCore;

namespace BuildPay.NotificationService;

public sealed class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<UserNotification> Notifications => Set<UserNotification>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserNotification>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).HasMaxLength(100);
            b.Property(x => x.Content).HasMaxLength(2000);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<InboxMessage>(b => b.HasKey(x => x.EventId));
    }
}

public sealed class UserNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Type { get; set; } = "in_app";
    public string Content { get; set; } = string.Empty;
    public bool Read { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class InboxMessage
{
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}

