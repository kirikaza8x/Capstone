using Payments.Application.DTOs.Payment;
using Shared.Application.Abstractions.Messaging;

namespace Payments.Application.Features.Payments.Queries.GetTransactionById;

public sealed record GetTransactionByIdQuery(Guid Id) : IQuery<PaymentTransactionDetailDto>;
