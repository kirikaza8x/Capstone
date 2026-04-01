using Shared.Application.Abstractions.Messaging;

namespace Reports.Application.Admin.Queries.GetSalesTrend;

public sealed record GetAdminSalesTrendQuery(int Days = 30) : IQuery<AdminSalesTrendResponse>;

public sealed record AdminSalesTrendResponse(List<AdminSalesTrendPointDto> ChartData);

public sealed record AdminSalesTrendPointDto(
    string DateLabel,
    decimal Revenue,
    int Transactions);
