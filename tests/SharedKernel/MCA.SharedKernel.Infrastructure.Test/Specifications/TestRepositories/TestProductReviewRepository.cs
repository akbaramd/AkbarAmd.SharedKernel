using AkbarAmd.SharedKernel.Infrastructure.Repositories;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestRepositories;

/// <summary>
/// Test repository implementation for product reviews.
/// </summary>
public sealed class TestProductReviewRepository : EfRepository<TestDbContext.TestDbContext, TestProductReview, int>
{
    public TestProductReviewRepository(TestDbContext.TestDbContext dbContext)
        : base(dbContext)
    {
    }
}

