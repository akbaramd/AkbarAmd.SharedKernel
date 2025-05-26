using System;
using System.Collections.Generic;
using CleanArchitecture.Domain.SeedWork;
using CleanArchitecture.Domain.SharedKernel.Events;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes;

public abstract class AggregateRoot : EntityBase, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 