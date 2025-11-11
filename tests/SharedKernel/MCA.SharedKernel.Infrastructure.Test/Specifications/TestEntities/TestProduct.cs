using AkbarAmd.SharedKernel.Domain;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

/// <summary>
/// Test entity for specification testing with SQLite.
/// Represents a simple product with name, price, and category.
/// </summary>
public sealed class TestProduct : Entity<int>
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation property for reviews (one-to-many relationship)
    public ICollection<TestProductReview> Reviews { get; private set; } = new List<TestProductReview>();

    private TestProduct() { }

    public TestProduct(string name, decimal price, string category, bool isActive = true)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Price = price;
        Category = category ?? throw new ArgumentNullException(nameof(category));
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative.");
        Price = newPrice;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

