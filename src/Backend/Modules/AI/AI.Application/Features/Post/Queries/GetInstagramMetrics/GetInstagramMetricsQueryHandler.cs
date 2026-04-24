using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Application.Services;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Handlers;

public sealed class GetInstagramMetricsQueryHandler
    : IQueryHandler<GetInstagramMetricsQuery, InstagramMetricsDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IInstagramMetricsService _instagramMetricsService;

    public GetInstagramMetricsQueryHandler(
        IPostRepository postRepository,
        IInstagramMetricsService instagramMetricsService)
    {
        _postRepository = postRepository;
        _instagramMetricsService = instagramMetricsService;
    }

    public async Task<Result<InstagramMetricsDto>> Handle(
        GetInstagramMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(
            query.PostId, cancellationToken);

        if (post is null)
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Post.NotFound(query.PostId));

        var distribution = post.ExternalDistributions
            .FirstOrDefault(d => d.Id == query.DistributionId
                              && d.Platform == ExternalPlatform.Instagram
                              && d.IsSent());

        if (distribution is null)
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Distribution.NotFound(ExternalPlatform.Instagram));

        if (string.IsNullOrWhiteSpace(distribution.ExternalPostId))
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Distribution.ExternalPostIdMissing);

        var metrics = await _instagramMetricsService.GetMetricsAsync(
            distribution.ExternalPostId,
            distribution.ExternalUrl,
            cancellationToken);

        if (metrics is null)
            return Result.Failure<InstagramMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        return Result.Success(metrics);
    }
}