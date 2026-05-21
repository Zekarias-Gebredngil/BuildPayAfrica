using Microsoft.EntityFrameworkCore;

namespace BuildPay.UserProfileService;

public sealed class UserProfileDbContext(DbContextOptions<UserProfileDbContext> options) : DbContext(options)
{
    public DbSet<UserProfile> Profiles => Set<UserProfile>();
    public DbSet<UserRating> Ratings => Set<UserRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfile>(b =>
        {
            b.HasKey(x => x.UserId);
            b.Property(x => x.DisplayName).HasMaxLength(200);
            b.Property(x => x.Bio).HasMaxLength(2000);
            b.Property(x => x.Role).HasMaxLength(50);
            b.Property(x => x.RatingAverage).HasColumnType("decimal(3,2)");
        });

        modelBuilder.Entity<UserRating>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TargetUserId);
            b.HasIndex(x => x.RaterUserId);
        });
    }
}

public sealed class UserProfile
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
}

public sealed class UserRating
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TargetUserId { get; set; }
    public Guid RaterUserId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

