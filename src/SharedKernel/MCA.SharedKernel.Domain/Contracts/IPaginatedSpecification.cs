namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Specification interface extending <see cref="ISpecification{T}"/> with pagination support.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public interface IPaginatedSpecification<T> : ISpecification<T>
{
    /// <summary>
    /// Gets the number of records to skip in the result set.
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Gets the maximum number of records to take from the result set.
    /// </summary>
    int Take { get; }
}