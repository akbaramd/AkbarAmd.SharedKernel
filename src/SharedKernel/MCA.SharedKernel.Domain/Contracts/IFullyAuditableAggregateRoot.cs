namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Comprehensive contract for fully auditable aggregate roots.
/// Combines all audit capabilities: creation, modification, deletion, and metadata.
/// </summary>
public interface IFullyAuditableAggregateRoot : 
    IDeletableAggregateRoot, 
    IMetadataAggregateRoot,
    IVersionableAggregateRoot
{
    /// <summary>
    /// UTC timestamp when the aggregate was last accessed (read).
    /// </summary>
    DateTime? LastAccessedAt { get; }

    /// <summary>
    /// Identifier of the user/system that last accessed the aggregate.
    /// </summary>
    string? LastAccessedBy { get; }

    /// <summary>
    /// Number of times the aggregate has been accessed.
    /// </summary>
    long AccessCount { get; }

    /// <summary>
    /// UTC timestamp when the aggregate was last backed up.
    /// </summary>
    DateTime? LastBackupAt { get; }

    /// <summary>
    /// UTC timestamp when the aggregate was last archived.
    /// </summary>
    DateTime? LastArchivedAt { get; }

    /// <summary>
    /// Marks the aggregate as accessed.
    /// </summary>
    /// <param name="accessedBy">Identifier of the user/system accessing the aggregate.</param>
    void MarkAccessed(string accessedBy);

    /// <summary>
    /// Marks the aggregate as backed up.
    /// </summary>
    void MarkBackedUp();

    /// <summary>
    /// Marks the aggregate as archived.
    /// </summary>
    void MarkArchived();
}