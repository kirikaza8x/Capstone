using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories;

public interface IMarketingContentRepository : IRepository<MarketingContent, Guid>
{
    Task<IReadOnlyList<MarketingContent>> GetByPublisherAsync(Guid publisherId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MarketingContent>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MarketingContent>> GetPublishedAsync(CancellationToken cancellationToken = default);
}