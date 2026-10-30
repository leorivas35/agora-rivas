using System.Linq.Expressions;
using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class GenericRepository<T>(AgoraRivasContext context) : IGenericRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> dbSet = context.Set<T>();
    
    #region CRUD Operations
    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        dbSet.Add(entity);
    }
    public void Remove(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        dbSet.Remove(entity);
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        dbSet.Update(entity);
    }
    #endregion

    #region Query Operations
    public async Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spec);
        IQueryable<T> query = context.Set<T>().AsQueryable();
        query = spec.ApplyCriteria(query);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbSet.AnyAsync(entity => entity.Id == id, cancellationToken);
    }
    public Task<TResult?> GetEntityWithSpecAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).ToListAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        => SpecificationEvaluator<T>.GetQuery(dbSet.AsQueryable(), spec);

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> spec)
        => SpecificationEvaluator<T>.GetQuery<ISpecification<T, TResult>, TResult>(dbSet.AsQueryable(), spec);

    public Task<T?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> ListAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetEntityWithSpec(ISpecification<T> spec)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        throw new NotImplementedException();
    }

    public Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> spec)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec)
    {
        throw new NotImplementedException();
    }

    public bool Exists(int id)
    {
        throw new NotImplementedException();
    }

    public Task<int> CountAsync(ISpecification<T> spec)
    {
        throw new NotImplementedException();
    }
    #endregion
}