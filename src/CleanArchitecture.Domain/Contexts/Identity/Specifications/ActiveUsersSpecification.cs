using System;
using System.Linq.Expressions;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;

namespace CleanArchitecture.Domain.Contexts.Identity.Specifications;

public class ActiveUsersSpecification
{
    public static Expression<Func<UserEntity, bool>> Expression => user => user.IsActive;
} 