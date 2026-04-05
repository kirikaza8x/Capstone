using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Reports.Application.Admin.Queries.GetSalesTrend;

internal sealed class GetAdminSalesTrendQueryHandler(
    ITicketingPublicApi ticketingApi,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetAdminSalesTrendQuery, AdminSalesTrendForAllEventResponse>
{
    public async Task<Result<AdminSalesTrendForAllEventResponse>> Handle(
        GetAdminSalesTrendQuery query,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var startDate = now.AddDays(-query.Days);

        // get data from ticketing public api
        var ticketingData = await ticketingApi.GetSalesTrendAsync(startDate, now, cancellationToken);

        var chartData = ticketingData.Select(d => new AdminSalesTrendPointDto(
            DateLabel: d.Date.ToString("yyyy-MM-dd"),
            Revenue: d.Revenue,
            Transactions: d.Transactions
        )).ToList();

        return Result.Success(new AdminSalesTrendForAllEventResponse(chartData));
    }
}
