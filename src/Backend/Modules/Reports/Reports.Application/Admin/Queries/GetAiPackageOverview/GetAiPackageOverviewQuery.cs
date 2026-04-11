using Shared.Application.Abstractions.Messaging;

namespace Reports.Application.Admin.Queries.GetAiPackageOverview;

public sealed record GetAiPackageOverviewQuery() : IQuery<AiPackageOverviewResponse>;

public sealed record AiPackageOverviewResponse(
    AiPackageRevenueCardDto TotalRevenue,
    AiPackageMostActivePackageDto MostActivePackage);

public sealed record AiPackageRevenueCardDto(
    decimal Value,
    double MonthlyGrowthRate,
    bool IsPositiveGrowth);

public sealed record AiPackageMostActivePackageDto(
    Guid? PackageId,
    string PackageName,
    int OrganizationsUsing);
