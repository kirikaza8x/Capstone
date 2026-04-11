using Payment.Domain.Enums;
using Payments.Application.DTOs.Refund;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;

public record SubmitRefundRequestCommand(
    Guid PaymentTransactionId,
    RefundRequestScope Scope,
    string UserReason,
    Guid? EventSessionId = null    // required when Scope = SingleItem
) : ICommand<RefundRequestDto>;
