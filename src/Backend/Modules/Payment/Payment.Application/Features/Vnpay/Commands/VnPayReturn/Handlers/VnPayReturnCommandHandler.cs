using Microsoft.Extensions.Logging;
using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Abstractions;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payment.Application.Features.VnPay.Handlers
{
    public class VnPayReturnCommandHandler : ICommandHandler<VnPayReturnQueriesCommand, VnPayResultDto>
    {
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<VnPayReturnCommandHandler> _logger;

        public VnPayReturnCommandHandler(
            IVnPayService vnPayService,
            ILogger<VnPayReturnCommandHandler> logger)
        {
            _vnPayService = vnPayService;
            _logger = logger;
        }

        public async Task<Result<VnPayResultDto>> Handle(VnPayReturnQueriesCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling VNPay return with TransactionNo: {TransactionNo}, ResponseCode: {ResponseCode}, OrderInfo: {OrderInfo}",
                command.VnpTransactionNo, command.VnpResponseCode, command.VnpOrderInfo);

            if (command.VnpResponseCode == "00")
            {
                _logger.LogInformation("Payment successful for TransactionNo: {TransactionNo}", command.VnpResponseCode);

                return Result.Success(new VnPayResultDto
                {
                    PaymentSuccess = true,
                    PaymentMessage = "Payment successful!",
                    TransactionNo = command.VnpTransactionNo,
                    ResponseCode = command.VnpResponseCode,
                    CheckedOutAt = DateTime.UtcNow
                });
            }

            _logger.LogWarning("Payment failed. Invalid vnp_OrderInfo format: {OrderInfo}", command.VnpOrderInfo);

            return Result.Failure<VnPayResultDto>(
                Error.Failure("VNPay.Failure", $"Invalid vnp_OrderInfo format: {command.VnpOrderInfo}")
            );
        }
    }
}
