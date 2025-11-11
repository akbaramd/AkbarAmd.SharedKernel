/*
 * Developed by Akbar Ahmadi Saray
 * Clean Architecture Domain - Bulk Update Builder Abstraction
 * Domain-layer abstraction for bulk update operations without EF Core dependencies
 * Year: 2025
 */

using System.Linq.Expressions;

namespace AkbarAmd.SharedKernel.Domain.Contracts.Repositories
{
    /// <summary>
    /// Domain abstraction for building bulk update operations
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IBulkUpdateBuilder<T> where T : class
    {
        /// <summary>
        /// Set a property to a specific value
        /// </summary>
        /// <typeparam name="TProperty">The property type</typeparam>
        /// <param name="propertyExpression">Expression selecting the property</param>
        /// <param name="value">The value to set</param>
        /// <returns>The builder for chaining</returns>
        IBulkUpdateBuilder<T> SetProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty value);

        /// <summary>
        /// Set a property to a value calculated from the entity
        /// </summary>
        /// <typeparam name="TProperty">The property type</typeparam>
        /// <param name="propertyExpression">Expression selecting the property</param>
        /// <param name="valueExpression">Expression calculating the new value</param>
        /// <returns>The builder for chaining</returns>
        IBulkUpdateBuilder<T> SetProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression, Expression<Func<T, TProperty>> valueExpression);
    }
}