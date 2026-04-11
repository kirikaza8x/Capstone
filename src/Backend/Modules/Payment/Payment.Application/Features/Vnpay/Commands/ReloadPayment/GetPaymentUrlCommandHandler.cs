using Microsoft.Extensions.Logging;
using Payment.Domain.Enums;
using Payments.Application.Abstractions;
using Payments.Domain.Repositories;
using Payments.Domain.UOW;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Payments.Application.Features.Payments.Commands.GetPaymentUrl;

public class GetPaymentUrlCommandHandler(
    IVnPayService vnPayService,
    IPaymentTransactionRepository transactionRepository,
    ICurrentUserService currentUserService,
    IPaymentUnitOfWork unitOfWork,
    ILogger<GetPaymentUrlCommandHandler> logger)
    : ICommandHandler<GetPaymentUrlCommand, GetPaymentUrlResult>
{
    private const int GatewaySessionMinutes = 15;

    public async Task<Result<GetPaymentUrlResult>> Handle(
        GetPaymentUrlCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var ipAddress = currentUserService.IpAddress ?? "127.0.0.1";

        // 1. Load transaction — no items needed
        var txn = await transactionRepository
            .GetByIdAsync(command.PaymentTransactionId, cancellationToken);

        if (txn == null)
            return Result.Failure<GetPaymentUrlResult>(
                Error.NotFound("PaymentUrl.NotFound",
                    "Transaction not found."));

        // 2. Ownership check
        if (txn.UserId != userId)
            return Result.Failure<GetPaymentUrlResult>(
                Error.Forbidden("PaymentUrl.Forbidden",
                    "You do not have access to this transaction."));

        // 3. BatchWalletPay has no payment page
        if (txn.Type == PaymentType.BatchWalletPay)
            return Result.Failure<GetPaymentUrlResult>(
                Error.Failure("PaymentUrl.NotApplicable",
                    "Wallet payments do not have a payment page."));

        // 4. Terminal state guards
        if (txn.InternalStatus == PaymentInternalStatus.Completed)
            return Result.Failure<GetPaymentUrlResult>(
                Error.Conflict("PaymentUrl.AlreadyCompleted",
                    "This payment has already been completed."));

        if (txn.InternalStatus == PaymentInternalStatus.Failed)
            return Result.Failure<GetPaymentUrlResult>(
                Error.Conflict("PaymentUrl.Failed",
                    "This payment has failed. Please initiate a new payment."));

        if (txn.InternalStatus == PaymentInternalStatus.Refunded)
            return Result.Failure<GetPaymentUrlResult>(
                Error.Conflict("PaymentUrl.Refunded",
                    "This payment has been refunded."));

        // 5. Must have TxnRef
        if (string.IsNullOrEmpty(txn.GatewayTxnRef))
            return Result.Failure<GetPaymentUrlResult>(
                Error.Failure("PaymentUrl.MissingTxnRef",
                    "Transaction has no gateway reference."));

        // 6. Check VNPay session expiry
        if (!string.IsNullOrEmpty(txn.GatewayCreateDate))
        {
            if (IsSessionExpired(txn.GatewayCreateDate))
            {
                logger.LogWarning(
                    "VNPay session expired: TxnRef={TxnRef}, CreateDate={CreateDate}",
                    txn.GatewayTxnRef, txn.GatewayCreateDate);

                txn.MarkFailed("VNPay session expired after 15 minutes.");
                transactionRepository.Update(txn);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Failure<GetPaymentUrlResult>(
                    Error.Conflict("PaymentUrl.SessionExpired",
                        "The payment session has expired (15 min limit). " +
                        "Please initiate a new payment."));
            }
        }
        else if (txn.CreatedAt.HasValue && IsCreatedAtExpired(txn.CreatedAt.Value))
        {
            // Fallback for transactions missing GatewayCreateDate
            logger.LogWarning(
                "VNPay session likely expired (no CreateDate): TxnRef={TxnRef}",
                txn.GatewayTxnRef);

            txn.MarkFailed("VNPay session expired after 15 minutes.");
            transactionRepository.Update(txn);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<GetPaymentUrlResult>(
                Error.Conflict("PaymentUrl.SessionExpired",
                    "The payment session has expired (15 min limit). " +
                    "Please initiate a new payment."));
        }

        // 7. Regenerate URL — same TxnRef, no new transaction on VNPay side
        try
        {
            var url = vnPayService.CreatePaymentUrl(
                amount: txn.Amount,
                txnRef: txn.GatewayTxnRef,
                orderDescription: txn.GatewayOrderInfo ?? "Payment",
                ipAddress: ipAddress);

            logger.LogInformation(
                "Payment URL regenerated: TxnRef={TxnRef}, UserId={UserId}",
                txn.GatewayTxnRef, userId);

            return Result.Success(new GetPaymentUrlResult(
                PaymentTransactionId: txn.Id,
                PaymentUrl: url,
                Amount: txn.Amount,
                InternalStatus: txn.InternalStatus));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to regenerate URL TxnRef={TxnRef}", txn.GatewayTxnRef);

            return Result.Failure<GetPaymentUrlResult>(
                Error.Failure("PaymentUrl.GatewayError",
                    "Could not generate payment URL."));
        }
    }

    // --------------------
    // Helpers
    // --------------------

    // GatewayCreateDate is Vietnam time stored as yyyyMMddHHmmss
    private static bool IsSessionExpired(string gatewayCreateDate)
    {
        if (!DateTime.TryParseExact(
                gatewayCreateDate,
                "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var createDate))
            return false;

        return GetVietnamNow() - createDate >
               TimeSpan.FromMinutes(GatewaySessionMinutes);
    }

    // Fallback — CreatedAt is UTC, duration comparison is timezone-safe
    private static bool IsCreatedAtExpired(DateTime createdAtUtc)
        => DateTime.UtcNow - createdAtUtc >
           TimeSpan.FromMinutes(GatewaySessionMinutes);

    private static DateTime GetVietnamNow()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                return TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.UtcNow.AddHours(7);
            }
        }
    }
}
