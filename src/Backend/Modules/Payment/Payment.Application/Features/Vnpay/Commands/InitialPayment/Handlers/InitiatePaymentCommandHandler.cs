using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Abstractions;
using Payments.Application.Features.Commands.InitiatePayment;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandHandler : ICommandHandler<InitiatePaymentCommand, InitiatePaymentResponseDto>
{
    private readonly IVnPayService _vnPayService;

    public InitiatePaymentCommandHandler(IVnPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    public Task<Result<InitiatePaymentResponseDto>> Handle(InitiatePaymentCommand command, CancellationToken cancellationToken)
    {


        if (string.IsNullOrWhiteSpace(command.OrderId))
        {
            return Task.FromResult(Result.Failure<InitiatePaymentResponseDto>(
                Error.Validation("Payment.MissingOrderId", "OrderId is required")));
        }

        try
        {
            var url = _vnPayService.CreatePaymentUrl(
                command.Amount,
                command.OrderId,
                "Order payment",
                command.IpAddress
            );

            var dto = new InitiatePaymentResponseDto(url);
            return Task.FromResult(Result.Success(dto));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<InitiatePaymentResponseDto>(
                Error.Failure("Payment.Error", $"Failed to create payment URL: {ex.Message}")));
        }
    }
}
