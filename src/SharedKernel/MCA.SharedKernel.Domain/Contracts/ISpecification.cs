/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain Shared Kernel - Specifications
 * Specification interfaces supporting filtering, paging, and sorting with separate fields and directions.
 * Year: 2025
 */

namespace MCA.SharedKernel.Domain.Contracts
{
    /// <summary>
    /// Base specification interface providing query composition for entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
    public interface ISpecification<T>
    {
        /// <summary>
        /// Handles the specification logic using the provided context.
        /// This includes filtering, sorting, paging, and other query customizations.
        /// </summary>
        /// <param name="context">The specification context containing the query to modify.</param>
        void Handle(ISpecificationContext<T> context);
    }
}
