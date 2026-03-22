using Payments.Application.DTOs.Payment;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Queries.GetMyTransactions;

public record GetMyTransactionsQuery(
    int Page = 1,
    int PageSize = 20
) : IQuery<GetMyTransactionsResult>;

public record GetMyTransactionsResult(
    IReadOnlyList<PaymentTransactionDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);