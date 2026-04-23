using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Application.Services;
using Marketing.Domain.Errors;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Handlers;

public sealed class GetInstagramPageMetricsQueryHandler
    : IQueryHandler<GetInstagramPageMetricsQuery, InstagramPageMetricsDto>
{
    private readonly IInstagramMetricsService _instagramMetricsService;

    public GetInstagramPageMetricsQueryHandler(
        IInstagramMetricsService instagramMetricsService)
    {
        _instagramMetricsService = instagramMetricsService;
    }

    public async Task<Result<InstagramPageMetricsDto>> Handle(
        GetInstagramPageMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var metrics = await _instagramMetricsService.GetPageTotalsAsync(
            query.Period, cancellationToken);

        if (metrics is null)
            return Result.Failure<InstagramPageMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        return Result.Success(metrics);
    }
}