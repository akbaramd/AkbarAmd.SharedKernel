namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support soft delete functionality.
/// Inherits from IModifiableAggregateRoot and adds deletion capabilities.
/// </summary>
public interface IDeletableAggregateRoot 
{
    /// <summary>
    /// UTC timestamp when the aggregate was soft deleted.
    /// </summary>
    DateTime? DeletedAt { get; }

    /// <summary>
    /// Identifier of the user/system that deleted the aggregate.
    /// </summary>
    string? DeletedBy { get; }

    /// <summary>
    /// Indicates whether the aggregate has been soft deleted.
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Soft deletes the aggregate by setting IsDeleted to true.
    /// </summary>
    /// <param name="deletedBy">Identifier of the user/system performing the deletion.</param>
    void Delete(string deletedBy);

    /// <summary>
    /// Restores a soft-deleted aggregate by setting IsDeleted to false.
    /// </summary>
    /// <param name="restoredBy">Identifier of the user/system performing the restoration.</param>
    void Restore(string restoredBy);
}