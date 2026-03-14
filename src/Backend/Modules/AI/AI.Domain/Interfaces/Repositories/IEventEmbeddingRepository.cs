using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories
{
    public interface IEventEmbeddingRepository : IRepository<EventEmbedding, Guid>
    {
        Task<EventEmbedding?> GetByEventIdAsync(
            Guid eventId, CancellationToken ct = default);

        Task<List<Guid>> GetUnembeddedEventIdsAsync(
            int batchSize, CancellationToken ct = default);

        /// <summary>
        /// Cosine similarity search — returns top K event IDs closest to the user vector.
        /// Uses pgvector HNSW index for fast approximate search.
        /// </summary>
        Task<List<(Guid EventId, double Score)>> SearchSimilarAsync(
            float[] userEmbedding,
            int topK = 20,
            CancellationToken ct = default);
    }
}