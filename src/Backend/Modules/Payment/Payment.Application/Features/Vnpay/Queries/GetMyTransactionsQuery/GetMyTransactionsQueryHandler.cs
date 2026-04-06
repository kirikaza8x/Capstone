using Payments.Application.DTOs.Payment;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Queries.GetMyTransactions;

public class GetMyTransactionsQueryHandler(
    IPaymentTransactionRepository transactionRepository,
    ICurrentUserService currentUserService)
    : IQueryHandler<GetMyTransactionsQuery, GetMyTransactionsResult>
{
    public async Task<Result<GetMyTransactionsResult>> Handle(
        GetMyTransactionsQuery query, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var (transactions, totalCount) = await transactionRepository
            .GetPagedByUserIdAsync(
                userId, query.Page, query.PageSize, cancellationToken);

        var dtos = transactions
            .Select(t => MapToDto(t))
            .ToList();

        return Result.Success(new GetMyTransactionsResult(
            dtos, totalCount, query.Page, query.PageSize));
    }

    private static PaymentTransactionDto MapToDto(PaymentTransaction t) => new PaymentTransactionDto
    {
        Id = t.Id,
        UserId = t.UserId,
        Type = t.Type,
        InternalStatus = t.InternalStatus,
        Amount = t.Amount,
        Currency = t.Currency,
        OrderId = t.OrderId,
        // Items = t.Items.Select(i => new BatchPaymentItemDto(
        //     Id: i.Id,
        //     OrderTicketId: i.OrderTicketId,
        //     EventSessionId: i.EventSessionId,
        //     Amount: i.Amount,
        //     InternalStatus: i.InternalStatus,
        //     RefundedAt: i.RefundedAt,
        //     CreatedAt: i.CreatedAt
        // )).ToList(),
        GatewayTxnRef = t.GatewayTxnRef,
        GatewayTransactionNo = t.GatewayTransactionNo,
        GatewayResponseCode = t.GatewayResponseCode,
        GatewayBankCode = t.GatewayBankCode,
        CreatedAt = t.CreatedAt,
        CompletedAt = t.CompletedAt,
        FailedAt = t.FailedAt,
        RefundedAt = t.RefundedAt
        // Username can be set later
    };
}
