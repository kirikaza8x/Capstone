using Payments.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Commands.ReviewRefundRequest;

public record ReviewRefundRequestCommand(
    Guid RefundRequestId,
    bool Approved,
    string ReviewerNote
) : ICommand<ReviewRefundRequestResult>;

public record ReviewRefundRequestResult(
    Guid RefundRequestId,
    RefundRequestStatus Status,
    decimal? AmountCredited,
    decimal? WalletBalanceAfter,
    DateTime ReviewedAt
);
