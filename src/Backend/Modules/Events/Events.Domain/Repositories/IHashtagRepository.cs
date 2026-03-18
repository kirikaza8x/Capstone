using Events.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Events.Domain.Repositories;

public interface IHashtagRepository : IRepository<Hashtag, int>
{
    Task<bool> IsSlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsInUseAsync(int hashtagId, CancellationToken cancellationToken = default);
    Task<List<string>> GetNamesByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}