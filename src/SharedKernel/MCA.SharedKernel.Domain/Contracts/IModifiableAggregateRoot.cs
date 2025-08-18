namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support modification tracking.
/// Inherits from ICreatableAggregateRoot and adds modification capabilities.
/// </summary>
public interface IModifiableAggregateRoot 
{
    /// <summary>
    /// UTC timestamp when the aggregate was last modified.
    /// </summary>
    DateTime? ModifiedAt { get; }

    /// <summary>
    /// Identifier of the user/system that last modified the aggregate.
    /// </summary>
    string? ModifiedBy { get; }

    /// <summary>
    /// UTC timestamp of last modification (UTC).
    /// </summary>
    DateTimeOffset LastModifiedUtc { get; }

    /// <summary>
    /// Marks the aggregate as modified, updating timestamps.
    /// </summary>
    /// <param name="modifiedBy">Identifier of the user/system making the modification.</param>
    void MarkModified(string modifiedBy);
}