using Microsoft.EntityFrameworkCore;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;

/// <summary>
/// Test DbContext for SQLite in-memory database testing.
/// </summary>
public sealed class TestDbContext : DbContext
{
    public DbSet<TestProduct> Products { get; set; } = null!;
    public DbSet<TestProductReview> ProductReviews { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Name);
            
            // Configure one-to-many relationship with reviews
            entity.HasMany(e => e.Reviews)
                  .WithOne(e => e.Product)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestProductReview>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReviewerName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Rating);
            entity.HasIndex(e => e.IsApproved);
        });
    }
}

