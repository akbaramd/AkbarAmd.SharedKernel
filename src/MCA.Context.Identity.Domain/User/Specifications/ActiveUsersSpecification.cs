using MCA.Context.Identity.Domain.User.Entities;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Context.Identity.Domain.User.Specifications;

public class ActiveUsersSpecification : ISpecification<UserEntity>
{
    public IQueryable<UserEntity> Query(IQueryable<UserEntity> queryable)
    {
        return queryable.Where(x => x.IsActive);
    }
} 