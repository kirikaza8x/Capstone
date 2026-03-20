namespace Payment.Application.Features.VnPay.Dtos;

public record WalletPayResultDto(
    Guid PaymentTransactionId,
    Guid WalletTransactionId,
    decimal AmountDebited,
    decimal BalanceAfter,
    DateTime PaidAt
);
