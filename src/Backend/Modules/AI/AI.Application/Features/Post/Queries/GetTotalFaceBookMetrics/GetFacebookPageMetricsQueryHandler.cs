using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Application.Services;
using Marketing.Domain.Errors;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Handlers;

public sealed class GetFacebookPageMetricsQueryHandler
    : IQueryHandler<GetFacebookPageMetricsQuery, FacebookPageMetricsDto>
{
    private readonly IFacebookMetricsService _facebookMetricsService;

    public GetFacebookPageMetricsQueryHandler(
        IFacebookMetricsService facebookMetricsService)
    {
        _facebookMetricsService = facebookMetricsService;
    }

    public async Task<Result<FacebookPageMetricsDto>> Handle(
        GetFacebookPageMetricsQuery query,
        CancellationToken cancellationToken)
    {
        var metrics = await _facebookMetricsService.GetPageTotalsAsync(cancellationToken);

        if (metrics is null)
            return Result.Failure<FacebookPageMetricsDto>(
                MarketingErrors.Distribution.MetricsFetchFailed);

        return Result.Success(metrics);
    }
}
