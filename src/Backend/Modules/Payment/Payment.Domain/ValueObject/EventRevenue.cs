namespace Payment.Domain.ValueObject;

public record EventRevenue(
    Guid EventId,
    decimal Revenue
);

// In Payment.Domain.ValueObject
public record EventRefundRate(
    Guid EventId,
    decimal GrossRevenue,
    decimal TotalRefunds,
    decimal RefundRatePercent);

public record EventTransactionSummary(
    Guid EventId,
    int TotalTransactions,
    int CompletedCount,
    int FailedCount,
    int RefundedCount,
    decimal WalletPayAmount,
    decimal DirectPayAmount);

public record GlobalRevenueSummary(
    decimal GrossRevenue,
    decimal TotalRefunds,
    decimal NetRevenue,
    int EventCount);