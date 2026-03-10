using Shared.Domain.DDD;

namespace Shared.Domain.Data.Repositories;

public partial interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}

