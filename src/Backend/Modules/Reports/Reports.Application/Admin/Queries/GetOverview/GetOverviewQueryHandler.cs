using Events.PublicApi.PublicApi;
using Payment.PublicApi.PublicApi;
using Payments.PublicApi.PublicApi;
using Reports.Domain;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;
using Users.PublicApi.PublicApi;

namespace Reports.Application.Admin.Queries.GetOverview;

internal sealed class GetOverviewQueryHandler(
    IUserPublicApi identityApi,
    IEventPublicApi eventApi,
    ITicketingPublicApi ticketingApi,
    IPaymentPublicApi globalRevenuePublicApi,
    IAiPackageRevenuePublicApi aiPackageRevenuePublicApi)
    : IQueryHandler<GetOverviewQuery, OverviewResponse>
{
    public async Task<Result<OverviewResponse>> Handle(
        GetOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var userTask = identityApi.GetUserMetricsAsync(cancellationToken);
        var eventTask = eventApi.GetEventMetricsAsync(cancellationToken);
        var ticketTask = ticketingApi.GetTicketingMetricsAsync(cancellationToken);

        await Task.WhenAll(userTask, eventTask, ticketTask);

        var globalRevenue = await globalRevenuePublicApi.GetGlobalRevenueOverviewAsync(cancellationToken);
        var aiPackageRevenue = await aiPackageRevenuePublicApi.GetAiPackageRevenueOverviewAsync(cancellationToken);

        var userMetrics = userTask.Result;
        var eventMetrics = eventTask.Result;
        var ticketMetrics = ticketTask.Result;

        if (userMetrics is null || eventMetrics is null || ticketMetrics is null)
        {
            return Result.Failure<OverviewResponse>(ReportErrors.Integration.ModuleDataUnavailable("Source Modules"));
        }

        int totalUsers = userMetrics.TotalAttendees + userMetrics.TotalOrganizers;

        double attendeesPercentage = totalUsers > 0
            ? Math.Round((double)userMetrics.TotalAttendees / totalUsers * 100, 1)
            : 0;

        double organizersPercentage = totalUsers > 0
            ? Math.Round((double)userMetrics.TotalOrganizers / totalUsers * 100, 1)
            : 0;

        var response = new OverviewResponse(
            Kpis: new AdminKpisDto(
                TotalRevenue: new TotalRevenueDto(
                    Value: globalRevenue.NetRevenue,
                    MonthlyGrowthRate: globalRevenue.MonthlyGrowthRate,
                    IsPositiveGrowth: globalRevenue.IsPositiveGrowth),
                ActiveUsers: new ActiveUsersDto(totalUsers),
                Events: new EventsSummaryDto(eventMetrics.TotalEvents, eventMetrics.LiveEventsNow),
                TicketsSold: new TicketsSoldSummaryDto(ticketMetrics.TotalTicketsSold),
                RevenueBreakdown: new RevenueBreakdownDto(
                    TicketRevenue: ticketMetrics.TotalRevenue,
                    AiPackageRevenue: aiPackageRevenue.TotalRevenue)
            ),
            UserDistribution: new UserDistributionDto(
                Attendees: new UserRoleStatDto(userMetrics.TotalAttendees, attendeesPercentage),
                Organizers: new UserRoleStatDto(userMetrics.TotalOrganizers, organizersPercentage),
                GrowthRate: userMetrics.UserGrowthRate
            )
        );

        return Result.Success(response);
    }
}
