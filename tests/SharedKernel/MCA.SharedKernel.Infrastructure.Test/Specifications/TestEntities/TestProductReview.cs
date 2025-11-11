using AkbarAmd.SharedKernel.Domain;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

/// <summary>
/// Test entity representing a product review with a relationship to TestProduct.
/// Used for testing includes and relational queries in specifications.
/// </summary>
public sealed class TestProductReview : Entity<int>
{
    public int ProductId { get; private set; }
    public TestProduct Product { get; private set; } = null!;
    public string ReviewerName { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public int Rating { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsApproved { get; private set; }

    private TestProductReview() { }

    public TestProductReview(TestProduct product, string reviewerName, string comment, int rating, bool isApproved = true)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        ProductId = product.Id;
        ReviewerName = reviewerName ?? throw new ArgumentNullException(nameof(reviewerName));
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        
        Rating = rating;
        IsApproved = isApproved;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(int newRating)
    {
        if (newRating < 1 || newRating > 5)
            throw new ArgumentOutOfRangeException(nameof(newRating), "Rating must be between 1 and 5.");
        Rating = newRating;
    }

    public void Approve()
    {
        IsApproved = true;
    }

    public void Reject()
    {
        IsApproved = false;
    }
}

