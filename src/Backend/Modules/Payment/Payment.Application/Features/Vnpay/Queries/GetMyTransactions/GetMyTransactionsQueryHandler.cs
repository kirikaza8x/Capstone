using Payments.Application.DTOs.Payment;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Queries.GetMyTransactions;

public class GetMyTransactionsQueryHandler(
    ICurrentUserService currentUser,
    IPaymentTransactionRepository transactionRepository)
    : IQueryHandler<GetMyTransactionsQuery, GetMyTransactionsResult>
{
    public async Task<Result<GetMyTransactionsResult>> Handle(
        GetMyTransactionsQuery query, CancellationToken cancellationToken)
    {
        var (transactions, totalCount) = await transactionRepository
            .GetPagedByUserIdAsync(currentUser.UserId, query.Page, query.PageSize, cancellationToken);

        var dtos = transactions.Select(MapToDto).ToList();

        return Result.Success(new GetMyTransactionsResult(
            dtos, totalCount, query.Page, query.PageSize));
    }

    private static PaymentTransactionDto MapToDto(PaymentTransaction t) => new(
        Id: t.Id,
        Type: t.Type,
        InternalStatus: t.InternalStatus,
        Amount: t.Amount,
        Currency: t.Currency,
        Items: t.Items.Select(i => new BatchPaymentItemDto(
            Id: i.Id,
            EventId: i.EventId,
            Amount: i.Amount,
            InternalStatus: i.InternalStatus,
            RefundedAt: i.RefundedAt,
            CreatedAt: i.CreatedAt     
        )).ToList(),
        GatewayTxnRef: t.GatewayTxnRef,
        GatewayTransactionNo: t.GatewayTransactionNo,
        GatewayResponseCode: t.GatewayResponseCode,
        GatewayBankCode: t.GatewayBankCode,
        CreatedAt: t.CreatedAt,         
        CompletedAt: t.CompletedAt,
        FailedAt: t.FailedAt,
        RefundedAt: t.RefundedAt);
}