using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Qdrant;

namespace Shared.Infrastructure.Qdrant;

/// <summary>
/// Ultra-minimal Qdrant wrapper.
///
/// NO domain knowledge. NO shared DTOs. NO search logic.
/// Just exposes raw Qdrant operations for modules to build upon.
///
/// QdrantClient is injected as a singleton — do NOT new it up inside repos.
/// Register once in DI: services.AddSingleton(sp => new QdrantClient(...))
/// </summary>
public abstract class QdrantRepositoryBase : IQdrantRepository
{
    /// <summary>Collection name — defined by each concrete repo.</summary>
    protected abstract string CollectionName { get; }

    /// <summary>
    /// Vector dimensionality — must match the embedding model used by this repo.
    /// e.g. all-MiniLM-L6-v2 = 384, text-embedding-3-small = 1536
    /// </summary>
    protected abstract int VectorSize { get; }
    protected readonly QdrantClient Client;
    protected readonly ILogger Logger;

    /// <param name="client">Singleton QdrantClient injected via DI.</param>
    protected QdrantRepositoryBase(QdrantClient client, ILogger logger)
    {
        Client = client;
        Logger = logger;
    }

    // ─────────────────────────────────────────────────────────────
    // Raw Qdrant Operations — No Domain Logic
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Ensure collection exists with cosine similarity and the repo's VectorSize.
    /// Marked virtual so concrete repos can override to also create payload indexes.
    /// Safe to call on startup — creates only if missing.
    /// </summary>
    public virtual async Task EnsureCollectionAsync(CancellationToken ct = default)
    {
        try
        {
            // Optimistic create — avoids TOCTOU race of ListCollections → CreateCollection
            await Client.CreateCollectionAsync(
                CollectionName,
                new VectorParams
                {
                    Size     = (ulong)VectorSize,
                    Distance = Distance.Cosine
                },
                cancellationToken: ct
            );

            Logger.LogInformation("✅ Created collection: {Collection}", CollectionName);
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            Logger.LogDebug("Collection {Collection} already exists — skipping", CollectionName);
        }
    }

    /// <summary>Get raw embedding vector for a point. Returns null if not found.</summary>
    public async Task<float[]?> GetVectorAsync(Guid pointId, CancellationToken ct = default)
    {
        var points = await Client.RetrieveAsync(
            collectionName: CollectionName,
            ids: new[] { new PointId { Uuid = pointId.ToString() } },
            withVectors: true,
            cancellationToken: ct
        );

        var point = points.FirstOrDefault();
        if (point is null) return null;

        // GetDenseVector() is the non-deprecated accessor since v1.16.0
        // .Data on DenseVector is RepeatedField<float> — not deprecated here
        return point.Vectors?.Vector?.GetDenseVector()?.Data.ToArray();
    }

    /// <summary>Delete a single point by ID.</summary>
    public async Task DeleteAsync(Guid pointId, CancellationToken ct = default)
    {
        await Client.DeleteAsync(
            CollectionName,
            new[] { new PointId { Uuid = pointId.ToString() } },
            cancellationToken: ct
        );
    }

    /// <summary>Get total point count in this collection.</summary>
    public async Task<int> CountAsync(CancellationToken ct = default)
        => (int)await Client.CountAsync(CollectionName, cancellationToken: ct);

    // ─────────────────────────────────────────────────────────────
    // Protected Raw Helpers — Modules Build Their Own Logic On Top
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Create a payload field index for fast filtering.
    /// Idempotent — safe to call every startup (Qdrant ignores if index already exists).
    /// Correct method name for Qdrant.Client v1.17: CreatePayloadIndexAsync.
    /// </summary>
    protected async Task CreatePayloadIndexAsync(
        string            field,
        PayloadSchemaType schemaType,
        CancellationToken ct = default)
    {
        await Client.CreatePayloadIndexAsync(
            collectionName: CollectionName,
            fieldName:      field,
            schemaType:     schemaType,
            cancellationToken: ct
        );
    }

    /// <summary>
    /// Raw single upsert — delegates to batch internally.
    /// For bulk ingestion use UpsertBatchRawAsync directly.
    /// </summary>
    protected async Task UpsertRawAsync(
        Guid pointId,
        float[] vector,
        IDictionary<string, Value> payload,
        CancellationToken ct)
    {
        await UpsertBatchRawAsync(new[] { (pointId, vector, payload) }, ct);
    }

    /// <summary>
    /// Raw batch upsert — significantly more efficient than looping UpsertRawAsync.
    /// Modules should prefer this for any bulk ingestion.
    /// </summary>
    protected async Task UpsertBatchRawAsync(
        IEnumerable<(Guid PointId, float[] Vector, IDictionary<string, Value> Payload)> items,
        CancellationToken ct)
    {
        var points = items.Select(item =>
        {
            var point = new PointStruct
            {
                Id      = new PointId { Uuid = item.PointId.ToString() },
                Vectors = item.Vector,
                Payload = { }
            };

            foreach (var (key, value) in item.Payload)
                point.Payload[key] = value;

            return point;
        }).ToList();

        await Client.UpsertAsync(CollectionName, points, cancellationToken: ct);
    }

    /// <summary>
    /// Raw search within this collection.
    /// Returns Qdrant ScoredPoint — modules map to their own DTOs.
    /// Note: SearchAsync will be deprecated in favour of QueryAsync in future Qdrant versions.
    /// </summary>
    protected async Task<IReadOnlyList<ScoredPoint>> SearchRawAsync(
        float[]           queryVector,
        Filter?           qdrantFilter,
        int               limit,
        CancellationToken ct)
    {
        return await Client.SearchAsync(
            collectionName:  CollectionName,
            vector:          queryVector,
            filter:          qdrantFilter,
            limit:           (ulong)limit,
            payloadSelector: true,
            vectorsSelector: false,
            cancellationToken: ct
        );
    }

    /// <summary>
    /// Raw cross-collection search — for recommendation or hybrid flows
    /// where results need to be pulled from a different collection.
    /// </summary>
    protected async Task<IReadOnlyList<ScoredPoint>> SearchCrossRawAsync(
        string            targetCollection,
        float[]           queryVector,
        Filter?           qdrantFilter,
        int               limit,
        CancellationToken ct)
    {
        return await Client.SearchAsync(
            collectionName:  targetCollection,
            vector:          queryVector,
            filter:          qdrantFilter,
            limit:           (ulong)limit,
            payloadSelector: true,
            vectorsSelector: false,
            cancellationToken: ct
        );
    }

    /// <summary>
    /// Helper: convert simple C# types to Qdrant Value.
    /// Modules can shadow this with their own switch if they need extra types.
    /// </summary>
    protected static Value ToQdrantValue(object value) => value switch
    {
        string s                 => new Value { StringValue = s },
        bool b                   => new Value { BoolValue = b },
        int i                    => new Value { IntegerValue = i },
        long l                   => new Value { IntegerValue = l },
        double d                 => new Value { DoubleValue = d },
        float f                  => new Value { DoubleValue = f },
        Guid g                   => new Value { StringValue = g.ToString() },
        DateTime dt              => new Value { StringValue = dt.ToString("O") }, // ISO 8601
        IEnumerable<string> list => new Value
        {
            ListValue = new ListValue
            {
                Values = { list.Select(s => new Value { StringValue = s }) }
            }
        },
        _ => new Value { StringValue = value?.ToString() ?? "" }
    };
}