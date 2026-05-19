using Microsoft.EntityFrameworkCore;
using BuildPay.Domain.Entities;
using BuildPay.Application.Common.Interfaces;

namespace BuildPay.Persistence;

public class BuildPayDbContext : DbContext, IBuildPayDbContext
{
    public BuildPayDbContext(DbContextOptions<BuildPayDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Contractor> Contractors { get; set; } = null!;
    public DbSet<ContractorDocument> ContractorDocuments { get; set; } = null!;
    public DbSet<ContractorReview> ContractorReviews { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Milestone> Milestones { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<ProjectPhoto> ProjectPhotos { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Delivery> Deliveries { get; set; } = null!;
    public DbSet<DeliveryLocationUpdate> DeliveryLocationUpdates { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Define additional constraints, decimal types, etc.
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Project>()
            .Property(p => p.Budget)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Milestone>()
            .Property(m => m.Amount)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");
            
        modelBuilder.Entity<Contractor>()
            .Property(c => c.Rating)
            .HasColumnType("decimal(3,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.UnitPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(i => i.LineTotal)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<DeliveryLocationUpdate>()
            .Property(l => l.Latitude)
            .HasColumnType("decimal(9,6)");

        modelBuilder.Entity<DeliveryLocationUpdate>()
            .Property(l => l.Longitude)
            .HasColumnType("decimal(9,6)");
            
        // Relationships
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId);
            
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId);
            
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Contractor)
            .WithMany()
            .HasForeignKey(p => p.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId);

        modelBuilder.Entity<OrderItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.ProductId);
            
        modelBuilder.Entity<Milestone>()
            .HasOne(m => m.Project)
            .WithMany()
            .HasForeignKey(m => m.ProjectId);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId);

        modelBuilder.Entity<ProjectPhoto>()
            .HasOne(p => p.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId);
            
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Milestone)
            .WithMany()
            .HasForeignKey(t => t.MilestoneId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ContractorDocument>()
            .HasOne(d => d.Contractor)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ContractorId);

        modelBuilder.Entity<ContractorReview>()
            .HasOne(r => r.Contractor)
            .WithMany()
            .HasForeignKey(r => r.ContractorId);

        modelBuilder.Entity<ContractorReview>()
            .HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Order)
            .WithMany()
            .HasForeignKey(d => d.OrderId);

        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.Driver)
            .WithMany()
            .HasForeignKey(d => d.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryLocationUpdate>()
            .HasOne(l => l.Delivery)
            .WithMany()
            .HasForeignKey(l => l.DeliveryId);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId);
    }
}
