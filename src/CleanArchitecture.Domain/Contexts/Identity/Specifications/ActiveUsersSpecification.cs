using System.Linq.Expressions;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;
using CleanArchitecture.Domain.SharedKernel.Interfaces;

namespace CleanArchitecture.Domain.Contexts.Identity.Specifications;

public class ActiveUsersSpecification : ISpecification<UserEntity>
{
    public IQueryable<UserEntity> Query(IQueryable<UserEntity> queryable)
    {
        return queryable.Where(x => x.IsActive);
    }
} 