namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Contract for aggregate roots that support metadata management.
/// </summary>
public interface IMetadataAggregateRoot
{
    /// <summary>
    /// Optional metadata associated with the aggregate.
    /// </summary>
    string? Metadata { get; }

    /// <summary>
    /// Updates the aggregate's metadata and marks it as modified.
    /// </summary>
    /// <param name="metadata">The new metadata value.</param>
    /// <param name="modifiedBy">Identifier of the user/system making the change.</param>
    void UpdateMetadata(string metadata, string modifiedBy);
}