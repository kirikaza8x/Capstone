using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Hashtags.Queries.GetHashtags;

internal sealed class GetHashtagsQueryHandler(
    IHashtagRepository hashtagRepository) : IQueryHandler<GetHashtagsQuery, IReadOnlyList<HashtagResponse>>
{
    public async Task<Result<IReadOnlyList<HashtagResponse>>> Handle(GetHashtagsQuery query, CancellationToken cancellationToken)
    {
        var hashtags = string.IsNullOrWhiteSpace(query.Name)
            ? await hashtagRepository.GetAllAsync(cancellationToken)
            : await hashtagRepository.SearchAsync(h => h.Name, query.Name, query.Take, cancellationToken);

        var response = hashtags
            .Select(h => new HashtagResponse(h.Id, h.Name, h.Slug, h.UsageCount))
            .ToList();

        return Result.Success<IReadOnlyList<HashtagResponse>>(response);
    }
}
