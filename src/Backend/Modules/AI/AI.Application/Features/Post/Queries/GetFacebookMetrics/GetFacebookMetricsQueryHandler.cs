// Marketing.Application/Posts/Handlers/GetFacebookMetricsQueryHandler.cs

using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Application.Services;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;
using Marketing.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Handlers;

public sealed class GetFacebookMetricsQueryHandler
    : IQueryHandler<GetFacebookMetricsQuery, FacebookMetricsDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IFacebookMetricsService _facebookMetricsService;

    public GetFacebookMetricsQueryHandler(
        IPostRepository postRepository,
        IFacebookMetricsService facebookMetricsService)
    {
        _postRepository = postRepository;
        _facebookMetricsService = facebookMetricsService;
    }

    public async Task<Result<FacebookMetricsDto>> Handle(
        GetFacebookMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithDistributionsAsync(
            query.PostId, cancellationToken);

        if (post is null)
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Post.NotFound(query.PostId));

        var distribution = post.ExternalDistributions
            .FirstOrDefault(d => d.Id == query.DistributionId 
                              && d.Platform == ExternalPlatform.Facebook
                              && d.IsSent());

        if (distribution is null)
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Distribution.NotFound(ExternalPlatform.Facebook));

        if (string.IsNullOrWhiteSpace(distribution.ExternalPostId))
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Distribution.ExternalPostIdMissing);

        var metrics = await _facebookMetricsService.GetMetricsAsync(
            distribution.ExternalPostId,
            distribution.ExternalUrl,
            cancellationToken);

        if (metrics is null)
            return Result.Failure<FacebookMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        return Result.Success(metrics);
    }
}