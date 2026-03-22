using Payment.Domain.Enums;

namespace Payments.Domain.Entities;

public partial class PaymentTransaction
{
    public VnPayTransactionStatus VnPayStatus =>
        int.TryParse(GatewayStatus, out var i)
        && Enum.IsDefined(typeof(VnPayTransactionStatus), i)
            ? (VnPayTransactionStatus)i
            : VnPayTransactionStatus.Unknown;

    public VnPayResponseCode VnPayResponseCode =>
        int.TryParse(GatewayResponseCode, out var i)
        && Enum.IsDefined(typeof(VnPayResponseCode), i)
            ? (VnPayResponseCode)i
            : VnPayResponseCode.Unknown;

    public bool IsGatewaySuccess()
        => VnPayResponseCode == VnPayResponseCode.Success
        && VnPayStatus == VnPayTransactionStatus.Success;

    public bool IsFraudSuspected()
        => VnPayResponseCode == VnPayResponseCode.SuspectedFraud
        || VnPayStatus == VnPayTransactionStatus.SuspectedFraud;

    public bool IsCancelled()
        => VnPayResponseCode == VnPayResponseCode.Cancelled;

    public IReadOnlyList<Guid> EventIds
        => Items.Select(i => i.EventId).ToList();
}