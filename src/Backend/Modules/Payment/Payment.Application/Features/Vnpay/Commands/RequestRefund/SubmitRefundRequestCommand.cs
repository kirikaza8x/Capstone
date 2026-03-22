using Payments.Application.DTOs.Refund;
using Payments.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Refunds.Commands.SubmitRefundRequest;

public record SubmitRefundRequestCommand(
    Guid PaymentTransactionId,
    RefundRequestScope Scope,
    string UserReason,
    Guid? EventId = null    // required when Scope = SingleItem
) : ICommand<RefundRequestDto>;