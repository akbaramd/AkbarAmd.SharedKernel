using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;
using CleanArchitecture.Domain.SharedKernel.Interfaces;

namespace CleanArchitecture.Domain.Contexts.Identity.Interfaces;

public interface IUserRepository : IRepository<UserEntity,Guid>
{
} 