using Events.Application.Hashtags.Queries.GetHashtags;
using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Hashtags.Queries.GetHashtagById;

public sealed record GetHashtagByIdQuery(int HashtagId) : IQuery<HashtagResponse>;
