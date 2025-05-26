using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Domain.SharedKernel.BaseTypes;

namespace CleanArchitecture.Domain.SeedWork.Interfaces;

public interface IRepository<T> where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
} 