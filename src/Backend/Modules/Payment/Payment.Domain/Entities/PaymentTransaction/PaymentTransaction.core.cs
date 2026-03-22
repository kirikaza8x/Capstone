using Payment.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities;

public partial class PaymentTransaction : AggregateRoot<Guid>
{
    // --------------------
    // References
    // --------------------
    public Guid UserId { get; private set; }
    public Guid? WalletId { get; private set; }   // set for WalletTopUp + BatchWalletPay
    public PaymentType Type { get; private set; }

    // --------------------
    // Core
    // --------------------
    public decimal Amount { get; private set; }   // total — always sum of items for batch types
    public string Currency { get; private set; } = "VND";

    // --------------------
    // Internal lifecycle
    // --------------------
    public PaymentInternalStatus InternalStatus { get; private set; }

    // --------------------
    // Items
    // WalletTopUp      → always empty
    // BatchDirectPay   → populated, AwaitingGateway until VNPay return
    // BatchWalletPay   → populated, Completed immediately
    // --------------------
    public ICollection<BatchPaymentItem> Items { get; private set; }
        = new List<BatchPaymentItem>();

    // --------------------
    // Gateway fields — null for BatchWalletPay
    // --------------------
    public string? GatewayTransactionNo { get; private set; }
    public string? GatewayResponseCode { get; private set; }
    public string? GatewayStatus { get; private set; }
    public string? GatewayOrderInfo { get; private set; }
    public string? GatewayTxnRef { get; private set; }
    public string? GatewayBankCode { get; private set; }
    public string? GatewayBankTranNo { get; private set; }
    public string? GatewayCardType { get; private set; }
    public string? GatewayPayDate { get; private set; }
    public string? GatewayTmnCode { get; private set; }
    public string? GatewaySecureHash { get; private set; }
    public string? GatewaySecureHashType { get; private set; }
    public string? GatewayLocale { get; private set; }
    public string? GatewayIpAddr { get; private set; }
    public string? GatewayCreateDate { get; private set; }
    public string? GatewayOrderType { get; private set; }
    public string? GatewayMerchant { get; private set; }

    // --------------------
    // Lifecycle timestamps
    // --------------------
    public DateTime? CompletedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    private PaymentTransaction() { }

    // --------------------
    // Factory — WalletTopUp
    // --------------------
    public static PaymentTransaction CreateWalletTopUp(
        Guid userId,
        Guid walletId,
        decimal amount,
        string? gatewayOrderInfo,
        string? gatewayTxnRef)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletId = walletId,
            Type = PaymentType.WalletTopUp,
            Amount = amount,
            InternalStatus = PaymentInternalStatus.AwaitingGateway,
            GatewayOrderInfo = gatewayOrderInfo,
            GatewayTxnRef = gatewayTxnRef,
            CreatedAt = DateTime.UtcNow
        };
    }

    // --------------------
    // Factory — BatchDirectPay
    // --------------------
    public static PaymentTransaction CreateBatchDirectPay(
        Guid userId,
        IEnumerable<(Guid EventId, decimal Amount)> items,
        string? gatewayOrderInfo,
        string? gatewayTxnRef)
    {
        var itemList = items.ToList();

        if (itemList.Count == 0)
            throw new ArgumentException("Batch must contain at least one item.");

        if (itemList.Any(i => i.Amount <= 0))
            throw new ArgumentException("All item amounts must be greater than zero.");

        var txn = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = PaymentType.BatchDirectPay,
            Amount = itemList.Sum(i => i.Amount),
            InternalStatus = PaymentInternalStatus.AwaitingGateway,
            GatewayOrderInfo = gatewayOrderInfo,
            GatewayTxnRef = gatewayTxnRef,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (eventId, amount) in itemList)
            txn.Items.Add(BatchPaymentItem.Create(txn.Id, eventId, amount));

        return txn;
    }

    // --------------------
    // Factory — BatchWalletPay
    // Born Completed — no gateway involved
    // --------------------
    public static PaymentTransaction CreateBatchWalletPay(
        Guid userId,
        Guid walletId,
        IEnumerable<(Guid EventId, decimal Amount)> items,
        string? orderInfo = null)
    {
        var itemList = items.ToList();

        if (itemList.Count == 0)
            throw new ArgumentException("Batch must contain at least one item.");

        if (itemList.Any(i => i.Amount <= 0))
            throw new ArgumentException("All item amounts must be greater than zero.");

        var now = DateTime.UtcNow;

        var txn = new PaymentTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WalletId = walletId,
            Type = PaymentType.BatchWalletPay,
            Amount = itemList.Sum(i => i.Amount),
            InternalStatus = PaymentInternalStatus.Completed,
            GatewayOrderInfo = orderInfo,
            CompletedAt = now,
            CreatedAt = now
        };

        foreach (var (eventId, amount) in itemList)
        {
            var item = BatchPaymentItem.Create(txn.Id, eventId, amount);
            item.MarkCompleted();
            txn.Items.Add(item);
        }

        return txn;
    }

    // --------------------
    // Domain behaviors
    // --------------------
    public void MarkCompleted()
    {
        InternalStatus = PaymentInternalStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        foreach (var item in Items)
            item.MarkCompleted();
    }

    public void MarkFailed(string? reason = null)
    {
        InternalStatus = PaymentInternalStatus.Failed;
        FailedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(reason))
            GatewayOrderInfo = string.IsNullOrEmpty(GatewayOrderInfo)
                ? reason
                : $"{GatewayOrderInfo} | {reason}";

        foreach (var item in Items)
            item.MarkFailed();
    }

    // Only called when ALL items are refunded — use IsFullyRefunded() to check first
    public void MarkRefunded()
    {
        if (InternalStatus != PaymentInternalStatus.Completed)
            throw new InvalidOperationException(
                $"Cannot refund transaction with status {InternalStatus}.");

        InternalStatus = PaymentInternalStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }

    public void UpdateGatewayInfo(
        string? responseCode,
        string? status,
        string? transactionNo,
        string? bankCode,
        string? bankTranNo)
    {
        GatewayResponseCode = responseCode;
        GatewayStatus = status;
        GatewayTransactionNo = transactionNo;
        GatewayBankCode = bankCode;
        GatewayBankTranNo = bankTranNo;
    }

    // True when every item has been individually refunded
    // WalletTopUp has no items so this returns false — it is never "refunded" via items
    public bool IsFullyRefunded()
        => Items.Count > 0
        && Items.All(i => i.InternalStatus == PaymentInternalStatus.Refunded);

    protected override void Apply(IDomainEvent @event) { }
}