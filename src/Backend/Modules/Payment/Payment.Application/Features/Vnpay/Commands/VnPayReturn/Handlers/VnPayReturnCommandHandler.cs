
using Payment.Application.Features.VnPay.Dtos;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payment.Application.Features.VnPay.Handlers
{
    public class VnPayReturnCommandHandler : ICommandHandler<VnPayReturnQueriesCommand, VnPayResultDto>
    {
        private readonly IVnPayService _vnPayService;

        public VnPayReturnCommandHandler(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        public async Task<Result<VnPayResultDto>> Handle(VnPayReturnQueriesCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.VnpOrderInfo))
            {
                return Result.Failure<VnPayResultDto>(
                    Error.NotFound("VNPay.MissingOrderInfo", "Missing vnp_OrderInfo in VNPay callback.")
                );
            }

            if (!Guid.TryParse(command.VnpOrderInfo, out var cardId))
            {
                return Result.Failure<VnPayResultDto>(
                    Error.Validation("VNPay.InvalidOrderInfo", $"Invalid vnp_OrderInfo format: {command.VnpOrderInfo}")
                );
            }


            return Result.Success(new VnPayResultDto
            {
                PaymentSuccess = true,
                PaymentMessage = "Payment successful!",
                TransactionNo = command.VnpTransactionNo,
                ResponseCode = command.VnpResponseCode,
                CheckedOutAt = DateTime.UtcNow
            });
        }


    }
}

