using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

public class GetPublicPostByIdQueryHandler
    : IQueryHandler<GetPublicPostByIdQuery, PostPublicDto>
{
    private readonly IPostRepository _postRepository;

    public GetPublicPostByIdQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PostPublicDto>> Handle(
        GetPublicPostByIdQuery query,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdAsync(query.PostId, cancellationToken);

        if (post == null || post.Status != PostStatus.Published)
        {
            return Result.Failure<PostPublicDto>(
                MarketingErrors.Post.NotFound(query.PostId));
        }

        var dto = new PostPublicDto(
            PostId: post.Id,
            EventId: post.EventId,
            Title: post.Title,
            Body: post.Body,
            ImageUrl: post.ImageUrl,
            PublishedAt: post.PublishedAt!.Value,
            TrackingUrl: BuildTrackingUrl(post.TrackingToken)
        );

        return Result.Success(dto);
    }

    private string BuildTrackingUrl(string token)
    {
        return $"https://your-domain.com/track/{token}";
    }
}