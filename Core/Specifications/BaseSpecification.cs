using System.Linq.Expressions;
using Core.Interfaces;

namespace Core.Specifications;
/// <summary>
/// Base class for specifications that define criteria, ordering, includes, and pagination for querying entities of type T.
/// </summary>
/// <typeparam name="T">Represents the type of the entity being queried.</typeparam>
public class BaseSpecification<T>(Expression<Func<T, bool>>? criteria) : ISpecification<T>
{
    public BaseSpecification() : this(null){}
    public Expression<Func<T, bool>>? Criteria {get;} = criteria;
    public Expression<Func<T, object>>? OrderBy {get; private set;}
    public Expression<Func<T, object>>? OrderByDescending {get; private set;}
    public List<Expression<Func<T, object>>> Includes {get;} = [];

    public List<string> ThenIncludes {get; } = [];
    public bool IsDistinct { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }   

    public IQueryable<T> ApplyCriteria(IQueryable<T> query)
    {
        if (Criteria != null)
        {
            query = query.Where(Criteria);
        }
        return query;
    }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    protected void AddInclude(string includeString)
    {
        ThenIncludes.Add(includeString);
    }
    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression ?? throw new ArgumentNullException(nameof(orderByExpression));
    }
    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression ?? throw new ArgumentNullException(nameof(orderByDescendingExpression));
    }
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
    protected void ApplyDistinct()
    {
        IsDistinct = true;
    }
}