using Payment.Domain.Enums;
using Payments.Application.Features.Payments.Commands.InitiatePayment;
using Shared.Application.Abstractions.Messaging;

namespace Payment.Application.Features.Vnpay.Commands.InitialPackagePayment;

public sealed record InitiatePackagePaymentCommand(
    Guid PackageId,
    PaymentType Method,
    string? Description = null) : ICommand<InitiatePaymentResult>;
