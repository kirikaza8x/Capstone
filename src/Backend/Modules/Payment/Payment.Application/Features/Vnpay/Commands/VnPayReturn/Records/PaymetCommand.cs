using Payment.Domain.Enums;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Commands.VnPayReturn;

public record VnPayReturnCommand(
    IDictionary<string, string> QueryParams
) : ICommand<VnPayReturnResult>;

public record VnPayReturnResult(
    Guid PaymentTransactionId,
    bool IsSuccess,
    string? Message,
    string? ResponseCode,
    string? TransactionNo,
    PaymentType Type,
    DateTime? CompletedAt
);
