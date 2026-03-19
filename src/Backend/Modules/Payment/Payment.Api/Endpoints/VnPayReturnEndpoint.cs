using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Payment.Application.Features.VnPay.Dtos;

namespace Payments.Api.Features
{
    public class VnPayReturnEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/payments/vnpay/return", async (
                string? vnp_Amount,
                string? vnp_BankCode,
                string? vnp_BankTranNo,
                string? vnp_CardType,
                string? vnp_OrderInfo,
                string? vnp_PayDate,
                string? vnp_ResponseCode,
                string? vnp_TmnCode,
                string? vnp_TransactionNo,
                string? vnp_TransactionStatus,
                string? vnp_TxnRef,
                string? vnp_SecureHash,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new VnPayReturnQueriesCommand
                {
                    VnpAmount = vnp_Amount,
                    VnpBankCode = vnp_BankCode,
                    VnpBankTranNo = vnp_BankTranNo,
                    VnpCardType = vnp_CardType,
                    VnpOrderInfo = vnp_OrderInfo,
                    VnpPayDate = vnp_PayDate,
                    VnpResponseCode = vnp_ResponseCode,
                    VnpTmnCode = vnp_TmnCode,
                    VnpTransactionNo = vnp_TransactionNo,
                    VnpTransactionStatus = vnp_TransactionStatus,
                    VnpTxnRef = vnp_TxnRef,
                    VnpSecureHash = vnp_SecureHash
                };

                var result = await sender.Send(command, cancellationToken);
                return result.ToOk();
            })
            .WithTags("Payments")
            .WithName("VnPayReturn")
            .WithSummary("Handle VNPay return callback")
            .WithDescription("""
                Handles VNPay's redirect after payment.
                Validates the callback parameters and returns payment result.
                """)
            .Produces<VnPayResultDto>(StatusCodes.Status200OK);
        }
    }
}
