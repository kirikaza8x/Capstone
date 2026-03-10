using Events.Application.Hashtags.Queries.GetHashtags;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Hashtags.Queries.GetHashtagById;

internal sealed class GetHashtagByIdQueryHandler(
    IHashtagRepository hashtagRepository) : IQueryHandler<GetHashtagByIdQuery, HashtagResponse>
{
    public async Task<Result<HashtagResponse>> Handle(GetHashtagByIdQuery query, CancellationToken cancellationToken)
    {
        var hashtag = await hashtagRepository.GetByIdAsync(query.HashtagId, cancellationToken);
        if (hashtag is null)
            return Result.Failure<HashtagResponse>(EventErrors.HashtagErrors.NotFound(query.HashtagId));

        return Result.Success(new HashtagResponse(hashtag.Id, hashtag.Name, hashtag.Slug, hashtag.UsageCount));
    }
}