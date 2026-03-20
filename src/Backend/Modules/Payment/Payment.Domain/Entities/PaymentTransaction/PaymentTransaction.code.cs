using Payment.Domain.Enums;

namespace Payments.Domain.Entities
{
    public partial class PaymentTransaction
    {
        public VnPayTransactionStatus TransactionStatus =>
            Enum.TryParse(GatewayStatus, out VnPayTransactionStatus status)
                ? status
                : VnPayTransactionStatus.Unknown;

        public VnPayResponseCode ResponseCode =>
            Enum.TryParse(GatewayResponseCode, out VnPayResponseCode code)
                ? code
                : VnPayResponseCode.Unknown;

        // Helper methods
        public bool IsSuccessful() =>
            ResponseCode == VnPayResponseCode.Success &&
            TransactionStatus == VnPayTransactionStatus.Success;

        public bool IsRefund() =>
            TransactionStatus is VnPayTransactionStatus.ProcessingRefund
                             or VnPayTransactionStatus.BankRefundRequested;

        public bool IsFraudSuspected() =>
            ResponseCode == VnPayResponseCode.SuspectedFraud ||
            TransactionStatus == VnPayTransactionStatus.SuspectedFraud;

        public bool IsCancelled() =>
            ResponseCode == VnPayResponseCode.Cancelled;
    }


}
