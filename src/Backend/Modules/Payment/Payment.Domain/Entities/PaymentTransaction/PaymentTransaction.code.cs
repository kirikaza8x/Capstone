using Payment.Domain.Enums;

namespace Payments.Domain.Entities
{
    public partial class PaymentTransaction
    {
        // Parsed views of raw gateway strings
        public VnPayTransactionStatus VnPayStatus =>
            int.TryParse(GatewayStatus, out var i) && Enum.IsDefined(typeof(VnPayTransactionStatus), i)
                ? (VnPayTransactionStatus)i
                : VnPayTransactionStatus.Unknown;

        public VnPayResponseCode VnPayResponseCode =>
            int.TryParse(GatewayResponseCode, out var i) && Enum.IsDefined(typeof(VnPayResponseCode), i)
                ? (VnPayResponseCode)i
                : VnPayResponseCode.Unknown;

        // Helpers — use InternalStatus for your own logic,
        // use these only when you need to inspect VNPay specifics
        public bool IsSuccessful() =>
            InternalStatus == PaymentInternalStatus.Completed &&
            VnPayResponseCode == VnPayResponseCode.Success &&
            VnPayStatus == VnPayTransactionStatus.Success;

        public bool IsRefund() =>
            VnPayStatus is VnPayTransactionStatus.ProcessingRefund
                       or VnPayTransactionStatus.BankRefundRequested;

        public bool IsFraudSuspected() =>
            VnPayResponseCode == VnPayResponseCode.SuspectedFraud ||
            VnPayStatus == VnPayTransactionStatus.SuspectedFraud;

        public bool IsCancelled() =>
            VnPayResponseCode == VnPayResponseCode.Cancelled;
    }
}
