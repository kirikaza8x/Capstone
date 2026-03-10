using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Hashtags.Queries.GetHashtags;

public sealed record HashtagResponse(int Id, string Name, string Slug, int UsageCount);

public sealed record GetHashtagsQuery : IQuery<IReadOnlyList<HashtagResponse>>;