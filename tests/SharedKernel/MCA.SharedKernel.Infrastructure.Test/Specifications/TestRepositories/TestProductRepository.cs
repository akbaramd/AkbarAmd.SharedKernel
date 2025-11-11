using AkbarAmd.SharedKernel.Infrastructure.Repositories;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestDbContext;
using MCA.SharedKernel.Infrastructure.Test.Specifications.TestEntities;

namespace MCA.SharedKernel.Infrastructure.Test.Specifications.TestRepositories;

/// <summary>
/// Test repository implementation for specification testing.
/// </summary>
public sealed class TestProductRepository : EfRepository<TestDbContext.TestDbContext, TestProduct, int>
{
    public TestProductRepository(TestDbContext.TestDbContext dbContext)
        : base(dbContext)
    {
    }
}