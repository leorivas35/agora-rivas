using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

/// <summary>
/// Applies the query operations defined by a specification to an entity query.
/// </summary>
/// <typeparam name="T">The entity type returned by the source query.</typeparam>
public class SpecificationEvaluator<T> where T : BaseEntity
{
    /// <summary>
    /// Applies the specification's filtering, ordering, paging, distinctness, and related-entity includes to a query.
    /// </summary>
    /// <remarks>
    /// Paging is applied before distinctness. Both expression-based and string-based includes are added to the
    /// query so the requested related entities are loaded with each result.
    /// </remarks>
    public static IQueryable<T> GetQuery(IQueryable<T> query, ISpecification<T> spec)
    {
        if (spec.Criteria is not null)
        {
            query = query.Where(spec.Criteria);
        }
        if (spec.OrderBy is not null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        if (spec.OrderByDescending is not null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }
        if (spec.IsDistinct)
        {
            query = query.Distinct();
        }
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.ThenIncludes.Aggregate(query, (current, include) => current.Include(include));
        return query;
    }

    /// <summary>
    /// Builds a result query by applying the specification's criteria, ordering, projection, distinctness, and paging.
    /// </summary>
    /// <remarks>
    /// Filtering and ordering are applied before projection. If no projection is provided, the source query is cast
    /// to the requested result type; distinctness and paging are applied when that query is already compatible.
    /// </remarks>
    public static IQueryable<TResult> GetQuery<TSpec, TResult>(IQueryable<T> query,
    ISpecification<T, TResult> spec)
    {
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }

        if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        var selectQuery = query as IQueryable<TResult>;

        if (spec.Select != null)
        {
            selectQuery = query.Select(spec.Select);
        }

        // Apply these operations to the projected query when available, otherwise to a compatible source query.
        if (spec.IsDistinct)
        {
            selectQuery = selectQuery?.Distinct();
        }

        if (spec.IsPagingEnabled)
        {
            selectQuery = selectQuery?.Skip(spec.Skip).Take(spec.Take);
        }

        // Without a projection, expose the source entities as the requested result type.
        return selectQuery ?? query.Cast<TResult>();
    }


}