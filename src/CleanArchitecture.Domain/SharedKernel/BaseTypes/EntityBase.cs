using System;

namespace CleanArchitecture.Domain.SharedKernel.BaseTypes;

public abstract class EntityBase
{
    public Guid Id { get; protected set; }

    protected EntityBase(Guid id)
    {
        Id = id;
    }

    protected EntityBase() { }
} 