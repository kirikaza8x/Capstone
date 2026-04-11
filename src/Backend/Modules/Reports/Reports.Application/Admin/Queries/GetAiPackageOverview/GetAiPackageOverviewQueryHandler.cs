using AI.PublicApi.PublicApi;
using Payments.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Reports.Application.Admin.Queries.GetAiPackageOverview;

internal sealed class GetAiPackageOverviewQueryHandler(
    IAiPackageRevenuePublicApi aiPackageRevenuePublicApi,
    IAiPackagePublicApi aiPackagePublicApi)
    : IQueryHandler<GetAiPackageOverviewQuery, AiPackageOverviewResponse>
{
    public async Task<Result<AiPackageOverviewResponse>> Handle(
        GetAiPackageOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var metrics = await aiPackageRevenuePublicApi.GetAiPackageRevenueOverviewAsync(cancellationToken);

        string packageName = "N/A";

        if (metrics.TopPackageId.HasValue)
        {
            var packageInfo = await aiPackagePublicApi.GetPackageBasicInfoAsync(
                metrics.TopPackageId.Value,
                cancellationToken);

            packageName = packageInfo?.Name ?? "N/A";
        }

        var response = new AiPackageOverviewResponse(
            TotalRevenue: new AiPackageRevenueCardDto(
                Value: metrics.TotalRevenue,
                MonthlyGrowthRate: metrics.MonthlyGrowthRate,
                IsPositiveGrowth: metrics.IsPositiveGrowth),
            MostActivePackage: new AiPackageMostActivePackageDto(
                PackageId: metrics.TopPackageId,
                PackageName: packageName,
                OrganizationsUsing: metrics.TopPackageOrganizationCount));

        return Result.Success(response);
    }
}
