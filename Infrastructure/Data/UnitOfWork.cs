using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data
{
    public class UnitOfWork(AgoraRivasContext context) : IUnitOfWork
    {
       private readonly ConcurrentDictionary<string, object> _repositories = new();
        public async Task<bool> Complete()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            string type = typeof(TEntity).Name;
            return (IGenericRepository<TEntity>)_repositories.GetOrAdd(type, t =>
            {
                Type repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(TEntity));
                return Activator.CreateInstance(repositoryType, context) ?? throw new InvalidOperationException("Failed to create repository instance for {t}.");
            });
        }
    }
}