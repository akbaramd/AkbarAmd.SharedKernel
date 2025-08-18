using System.Linq.Expressions;

namespace MCA.SharedKernel.Domain.Contracts;

/// <summary>
/// Context for specification operations, providing query manipulation capabilities.
/// </summary>
/// <typeparam name="T">The type of entity to which the specification applies.</typeparam>
public class SpecificationContext<T>
{
    public IQueryable<T> Query { get; set; }

    public SpecificationContext(IQueryable<T> query)
    {
        Query = query;
    }

    /// <summary>
    /// Adds a filter condition to the query.
    /// </summary>
    public void AddFilter(Expression<Func<T, bool>> predicate)
    {
        Query = Query.Where(predicate);
    }

    /// <summary>
    /// Adds ordering to the query.
    /// </summary>
    public void AddOrderBy<TKey>(Expression<Func<T, TKey>> keySelector, bool ascending = true)
    {
        if (ascending)
            Query = Query.OrderBy(keySelector);
        else
            Query = Query.OrderByDescending(keySelector);
    }

    /// <summary>
    /// Adds pagination to the query.
    /// </summary>
    public void AddPagination(int skip, int take)
    {
        Query = Query.Skip(skip).Take(take);
    }
}