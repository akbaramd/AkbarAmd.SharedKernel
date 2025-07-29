using MCA.Identity.Domain.User.Entities;
using MCA.SharedKernel.Domain;
using MCA.SharedKernel.Domain.Contracts;

namespace MCA.Identity.Domain.User.Specifications;

public class ActiveUsersSpecification : ISpecification<UserEntity>
{
    public IQueryable<UserEntity> Query(IQueryable<UserEntity> queryable)
    {
        return queryable.Where(x => x.IsActive);
    }
} 