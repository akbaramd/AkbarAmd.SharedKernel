namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support creation tracking.
/// Base interface for all creatable aggregates.
/// </summary>
public interface ICreatableAggregateRoot
{
    /// <summary>
    /// UTC timestamp when the aggregate was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Identifier of the user/system that created the aggregate.
    /// </summary>
    string CreatedBy { get; }

    /// <summary>
    /// UTC timestamp when the aggregate was created (UTC).
    /// </summary>
    DateTimeOffset CreatedAtUtc { get; }
}