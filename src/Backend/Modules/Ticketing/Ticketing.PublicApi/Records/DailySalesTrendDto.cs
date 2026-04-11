namespace Ticketing.PublicApi.Records;

public sealed record DailySalesTrendDto(
    DateTime Date,
    decimal Revenue,
    int Transactions);
