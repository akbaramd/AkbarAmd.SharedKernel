namespace AkbarAmd.SharedKernel.Application.Contracts
{
    /// <summary>
    /// Generic strongly-typed mapper for a specific source and destination type.
    /// This interface is bound to a single source–destination pair.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to map to.</typeparam>
    public interface IMapper<in TSource, TDestination>
    {
        /// <summary>
        /// Creates a new TDestination instance by mapping from a TSource instance.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A mapped instance of TDestination.</returns>
        Task<TDestination> MapAsync(TSource source, CancellationToken cancellationToken = default);

        /// <summary>
        /// Maps values from the specified TSource instance into the given existing TDestination instance.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="destination">Existing destination object to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task MapAsync(TSource source, TDestination destination, CancellationToken cancellationToken = default);
    }
}