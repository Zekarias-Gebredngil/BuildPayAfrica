using BuildPay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildPay.Application.Common.Interfaces;

public interface IBuildPayDbContext
{
    DbSet<User> Users { get; }
    DbSet<Contractor> Contractors { get; }
    DbSet<ContractorDocument> ContractorDocuments { get; }
    DbSet<ContractorReview> ContractorReviews { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Project> Projects { get; }
    DbSet<Milestone> Milestones { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<ProjectPhoto> ProjectPhotos { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Delivery> Deliveries { get; }
    DbSet<DeliveryLocationUpdate> DeliveryLocationUpdates { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

