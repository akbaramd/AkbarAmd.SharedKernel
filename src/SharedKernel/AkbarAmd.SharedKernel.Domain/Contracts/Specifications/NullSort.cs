namespace AkbarAmd.SharedKernel.Domain.Contracts.Specifications;

/// <summary>
/// Specifies how null values should be ordered in sorting operations.
/// </summary>
public enum NullSort
{
    /// <summary>
    /// No explicit null ordering policy. Database default behavior applies.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Null values are ordered first (before non-null values).
    /// </summary>
    NullsFirst = 1,

    /// <summary>
    /// Null values are ordered last (after non-null values).
    /// </summary>
    NullsLast = 2
}

