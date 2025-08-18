namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Specification interface extending <see cref="ISortedSpecification{T}"/> with filtering capabilities.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public interface IFilterableSpecification<T> : ISortedSpecification<T>
{
    /// <summary>
    /// Gets the search string used for filtering the result set.
    /// </summary>
    string Search { get; }
}