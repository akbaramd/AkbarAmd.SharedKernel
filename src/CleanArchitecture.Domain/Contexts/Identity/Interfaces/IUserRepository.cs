using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.Entities;
using CleanArchitecture.Domain.Contexts.Identity.Aggregates.User.ValueObjects;

namespace CleanArchitecture.Domain.Contexts.Identity.Interfaces;

public interface IUserRepository
{
    Task<UserEntity> GetByIdAsync(Guid id);
    Task<UserEntity> GetByEmailAsync(Email email);
    Task AddAsync(UserEntity user);
    bool ExistsByEmail(Email email);
} 