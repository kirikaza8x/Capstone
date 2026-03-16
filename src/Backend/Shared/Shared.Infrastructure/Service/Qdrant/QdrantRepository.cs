// using Qdrant.Client;
// using Qdrant.Client.Grpc;
// using GrpcRange = Qdrant.Client.Grpc.Range;
// using Microsoft.Extensions.Logging;
// using Shared.Application.DTOs;
// using Shared.Infrastructure.Configs.Qdrant;
// using Shared.Application.Abstractions.Qdrant;

// namespace Shared.Infrastructure.Qdrant;

// /// <summary>
// /// Abstract base — owns the QdrantClient and implements everything in IQdrantRepositoryBase.
// /// Also exposes protected helpers (BuildFilter, SearchRawAsync, SearchCrossAsync, ToValue, MapToResult)
// /// so derived classes never re-implement boilerplate.
// ///
// /// Derived classes only need to:
// ///   1. Supply CollectionName
// ///   2. Implement their domain-typed interface methods using the protected helpers
// /// </summary>
// public abstract class QdrantRepositoryBase : IQdrantRepositoryBase
// {
//     protected abstract string CollectionName { get; }

//     protected readonly QdrantClient Client;
//     protected readonly QdrantConfig Options;
//     protected readonly ILogger Logger;

//     protected QdrantRepositoryBase(QdrantConfig options, ILogger logger)
//     {
//         Options = options;
//         Logger  = logger;

//         Client = new QdrantClient(
//             host:   Options.Host,
//             port:   Options.Port,
//             https:  Options.UseHttps,
//             apiKey: string.IsNullOrWhiteSpace(Options.ApiKey) ? null : Options.ApiKey
//         );
//     }

//     // -------------------------------------------------------------------------
//     // IQdrantRepositoryBase — identical for every derived repository
//     // -------------------------------------------------------------------------

//     public async Task EnsureCollectionAsync(CancellationToken ct = default)
//     {
//         var existing = await Client.ListCollectionsAsync(ct);
//         if (existing.Contains(CollectionName)) return;

//         await Client.CreateCollectionAsync(
//             CollectionName,
//             new VectorParams { Size = (ulong)Options.VectorSize, Distance = Distance.Cosine },
//             cancellationToken: ct
//         );

//         Logger.LogInformation("Created Qdrant collection: {Collection}", CollectionName);
//     }

//     public async Task<float[]?> GetEmbeddingAsync(Guid pointId, CancellationToken ct = default)
//     {
//         var points = await Client.RetrieveAsync(
//             collectionName: CollectionName,
//             ids:            [new PointId { Uuid = pointId.ToString() }],
//             withVectors:    true,
//             cancellationToken: ct
//         );

//         return points.FirstOrDefault()?.Vectors?.Vector?.Data?.ToArray();
//     }

//     public async Task DeleteAsync(Guid pointId, CancellationToken ct = default)
//     {
//         await Client.DeleteAsync(CollectionName, ids: new[] { pointId }, cancellationToken: ct);
//         Logger.LogDebug("Deleted {PointId} from '{Collection}'", pointId, CollectionName);
//     }

//     public async Task<int> GetCountAsync(CancellationToken ct = default)
//         => (int)await Client.CountAsync(CollectionName, cancellationToken: ct);

//     // -------------------------------------------------------------------------
//     // Protected helpers — available to all derived repositories
//     // -------------------------------------------------------------------------

//     /// <summary>Upserts a single point into this repository's collection.</summary>
//     protected async Task UpsertPointAsync(
//         Guid pointId,
//         float[] embedding,
//         Dictionary<string, object> payload,
//         CancellationToken ct)
//     {
//         var point = new PointStruct { Id = pointId, Vectors = embedding };

//         foreach (var (key, value) in payload)
//             point.Payload[key] = ToValue(value);

//         await Client.UpsertAsync(CollectionName, new[] { point }, cancellationToken: ct);
//         Logger.LogDebug("Upserted {PointId} into '{Collection}'", pointId, CollectionName);
//     }

//     /// <summary>Searches this repository's own collection.</summary>
//     protected async Task<IReadOnlyList<ScoredPoint>> SearchRawAsync(
//         float[] queryEmbedding,
//         QdrantSearchFilter? filter,
//         int limit,
//         CancellationToken ct)
//         => await Client.SearchAsync(
//             collectionName:    CollectionName,
//             vector:            queryEmbedding,
//             filter:            BuildFilter(filter),
//             limit:             (ulong)limit,
//             cancellationToken: ct
//         );

//     /// <summary>
//     /// Searches a DIFFERENT collection using a vector from this one.
//     /// Used by UserProfileRepository to query event_embeddings with a user vector.
//     /// </summary>
//     protected async Task<IReadOnlyList<ScoredPoint>> SearchCrossAsync(
//         string targetCollection,
//         float[] queryEmbedding,
//         QdrantSearchFilter? filter,
//         int limit,
//         CancellationToken ct)
//         => await Client.SearchAsync(
//             collectionName:    targetCollection,
//             vector:            queryEmbedding,
//             filter:            BuildFilter(filter),
//             limit:             (ulong)limit,
//             cancellationToken: ct
//         );

//     /// <summary>Maps a raw ScoredPoint to the shared QdrantSearchResult DTO.</summary>
//     protected static QdrantSearchResult MapToResult(ScoredPoint r) =>
//         new(
//             EventId:     Guid.Parse(r.Id.Uuid),
//             Score:       r.Score,
//             Title:       r.Payload.TryGetValue("title",       out var t)  ? t.StringValue    : string.Empty,
//             Description: r.Payload.TryGetValue("description", out var d)  ? d.StringValue    : string.Empty,
//             Categories:  r.Payload.TryGetValue("categories",  out var c)  ? FromListValue(c) : [],
//             Hashtags:    r.Payload.TryGetValue("hashtags",    out var h)  ? FromListValue(h) : [],
//             IsActive:    r.Payload.TryGetValue("is_active",   out var ia) && ia.BoolValue
//         );

//     protected static Filter? BuildFilter(QdrantSearchFilter? filter)
//     {
//         if (filter is null) return null;

//         var must    = new List<Condition>();
//         var mustNot = new List<Condition>();

//         if (filter.IsActive.HasValue)
//             must.Add(new Condition
//             {
//                 Field = new FieldCondition
//                 {
//                     Key   = "is_active",
//                     Match = new Match { Boolean = filter.IsActive.Value }
//                 }
//             });

//         if (filter.Categories is { Count: > 0 })
//             foreach (var category in filter.Categories)
//                 must.Add(new Condition
//                 {
//                     Field = new FieldCondition
//                     {
//                         Key   = "categories",
//                         Match = new Match { Text = category }
//                     }
//                 });

//         if (filter.MinUpdatedAt.HasValue)
//             must.Add(new Condition
//             {
//                 Field = new FieldCondition
//                 {
//                     Key   = "updated_at",
//                     Range = new GrpcRange
//                     {
//                         Gte = new DateTimeOffset(filter.MinUpdatedAt.Value).ToUnixTimeSeconds()
//                     }
//                 }
//             });

//         if (filter.ExcludeEventIds is { Count: > 0 })
//             mustNot.Add(new Condition
//             {
//                 HasId = new HasIdCondition
//                 {
//                     HasId =
//                     {
//                         filter.ExcludeEventIds.Select(id => new PointId { Uuid = id.ToString() })
//                     }
//                 }
//             });

//         if (must.Count == 0 && mustNot.Count == 0) return null;

//         var qdrantFilter = new Filter();
//         qdrantFilter.Must.AddRange(must);
//         qdrantFilter.MustNot.AddRange(mustNot);
//         return qdrantFilter;
//     }

//     protected static Value ToValue(object value) => value switch
//     {
//         string s                 => new Value { StringValue  = s },
//         bool b                   => new Value { BoolValue    = b },
//         int i                    => new Value { IntegerValue = i },
//         long l                   => new Value { IntegerValue = l },
//         double d                 => new Value { DoubleValue  = d },
//         float f                  => new Value { DoubleValue  = f },
//         IEnumerable<string> list => new Value
//         {
//             ListValue = new ListValue
//             {
//                 Values = { list.Select(s => new Value { StringValue = s }) }
//             }
//         },
//         _ => new Value { StringValue = value.ToString() ?? string.Empty }
//     };

//     protected static List<string> FromListValue(Value v)
//         => v.ListValue.Values.Select(x => x.StringValue).ToList();
// }