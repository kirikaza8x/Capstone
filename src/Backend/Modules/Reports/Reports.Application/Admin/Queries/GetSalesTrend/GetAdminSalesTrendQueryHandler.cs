using Payment.PublicApi.PublicApi;
using Payments.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;

namespace Reports.Application.Admin.Queries.GetSalesTrend;

internal sealed class GetAdminSalesTrendQueryHandler(
    IPaymentPublicApi paymentRevenuePublicApi,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetAdminSalesTrendQuery, AdminSalesTrendForAllEventResponse>
{
    public async Task<Result<AdminSalesTrendForAllEventResponse>> Handle(
        GetAdminSalesTrendQuery query,
        CancellationToken cancellationToken)
    {
        var now = dateTimeProvider.UtcNow;
        var startDate = now.AddDays(-query.Days);

        var paymentData = await paymentRevenuePublicApi.GetGlobalSalesTrendAsync(
            startDate,
            now,
            cancellationToken);

        var chartData = paymentData.Select(d => new AdminSalesTrendPointDto(
            DateLabel: d.Date.ToString("yyyy-MM-dd"),
            Revenue: d.Revenue,
            Transactions: d.Transactions
        )).ToList();

        return Result.Success(new AdminSalesTrendForAllEventResponse(chartData));
    }
}
