using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class GenericRepository<T>(AgoraRivasContext context) : IGenericRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> dbSet = context.Set<T>();
        
    public async Task<T?> GetByIdAsync(int id) 
    {
        return await context.Set<T>().FindAsync(id);
    }
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

    public async Task<IReadOnlyList<T>> ListAllAsync()
    {
        return await context.Set<T>().ToListAsync();
    }

    public async Task<T?> GetEntityWithSpec(ISpecification<T> spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).ToListAsync();
    }

    public async Task<TResult?> GetEntityWithSpec<TResult>(ISpecification<T, TResult> spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<T, TResult> spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).ToListAsync();
    }

    public async Task<int> CountAsync(ISpecification<T> spec)
    {
        ArgumentNullException.ThrowIfNull(spec);
        return await ApplySpecification(spec).CountAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await dbSet.AnyAsync(entity => entity.Id == id);
    }
    
    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        => SpecificationEvaluator<T>.GetQuery(dbSet.AsQueryable(), spec);

    private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> spec)
        => SpecificationEvaluator<T>.GetQuery<ISpecification<T, TResult>, TResult>(dbSet.AsQueryable(), spec);
    
}