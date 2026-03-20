using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Shared.Domain.Data.Repositories;
using Shared.Domain.DDD;

namespace Shared.Infrastructure.Data;

public partial class RepositoryBase<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    public virtual void Upsert(TEntity entity)
    {
        var trackedEntity = DbSet.Local.FirstOrDefault(e => 
            e.Id != null && e.Id.Equals(entity.Id));

        if (trackedEntity != null)
        {
            Context.Entry(trackedEntity).State = EntityState.Detached;
        }

        DbSet.Update(entity);
    }

    public virtual async Task UpsertAsync(
        TEntity entity, 
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default)
    {
        var existing = await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(entity);
        }
        else
        {

            entity.Id = existing.Id; 
            
            DbSet.Update(entity);
        }
    }
}