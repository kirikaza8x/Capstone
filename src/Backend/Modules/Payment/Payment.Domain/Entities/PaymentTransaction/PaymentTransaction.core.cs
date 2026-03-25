using Payment.Domain.Enums;
using Payments.Domain.Events;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities;

public partial class PaymentTransaction : AggregateRoot<Guid>
{
    // --------------------
    // References
    // --------------------
    public Guid UserId { get; private set; }
    public Guid? WalletId { get; private set; }
    public Guid? OrderId { get; private set; }    // links to Order in Ticketing module
    public PaymentType Type { get; private set; }

    // --------------------
    // Core
    // --------------------
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "VND";

    // --------------------
    // Internal lifecycle
    // --------------------
    public PaymentInternalStatus InternalStatus { get; private set; }

    // --------------------
    // Items
    // WalletTopUp    → always empty
    // BatchDirectPay → populated, AwaitingGateway until VNPay return
    // BatchWalletPay → populated, Completed immediately
    // --------------------
    public ICollection<BatchPaymentItem> Items { get; private set; }
        = new List<BatchPaymentItem>();

    // --------------------
    // Gateway fields
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
    // Factory — WalletTopUp (unchanged)
    // --------------------
    public static PaymentTransaction CreateWalletTopUp(
        Guid userId,
        Guid walletId,
        decimal amount,
        string? gatewayOrderInfo,
        string? gatewayTxnRef,
        string? ipAddress = null)
    {
        if (amount <= 0)
            throw new ArgumentException(
                "Amount must be greater than zero.", nameof(amount));

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
            GatewayCreateDate = GetVietnamCreateDate(),
            GatewayLocale = "vn",
            GatewayIpAddr = NormalizeIp(ipAddress),
            GatewayOrderType = "other",
            CreatedAt = DateTime.UtcNow
        };
    }

    // --------------------
    // Factory — BatchDirectPay
    // --------------------
    public static PaymentTransaction CreateBatchDirectPay(
        Guid userId,
        Guid orderId,
        IEnumerable<(Guid OrderTicketId, Guid EventSessionId, decimal Amount)> items,
        string? gatewayOrderInfo,
        string? gatewayTxnRef,
        string? ipAddress = null)
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
            OrderId = orderId,
            Type = PaymentType.BatchDirectPay,
            Amount = itemList.Sum(i => i.Amount),
            InternalStatus = PaymentInternalStatus.AwaitingGateway,
            GatewayOrderInfo = gatewayOrderInfo,
            GatewayTxnRef = gatewayTxnRef,
            GatewayCreateDate = GetVietnamCreateDate(),
            GatewayLocale = "vn",
            GatewayIpAddr = NormalizeIp(ipAddress),
            GatewayOrderType = "other",
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (orderTicketId, eventSessionId, amount) in itemList)
            txn.Items.Add(BatchPaymentItem.Create(
                txn.Id, orderTicketId, eventSessionId, amount));

        return txn;
    }

    // --------------------
    // Factory — BatchWalletPay
    // --------------------
    public static PaymentTransaction CreateBatchWalletPay(
    Guid userId,
    Guid walletId,
    Guid orderId,
    IEnumerable<(Guid OrderTicketId, Guid EventSessionId, decimal Amount)> items,
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
            OrderId = orderId,
            Type = PaymentType.BatchWalletPay,
            Amount = itemList.Sum(i => i.Amount),
            GatewayOrderInfo = orderInfo,
            CreatedAt = now
        };

        foreach (var (orderTicketId, eventSessionId, amount) in itemList)
        {
            var item = BatchPaymentItem.Create(
                txn.Id, orderTicketId, eventSessionId, amount);

            txn.Items.Add(item);
        }

        txn.MarkCompleted();

        return txn;
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

    public void MarkRefunded()
    {
        if (InternalStatus != PaymentInternalStatus.Completed)
            throw new InvalidOperationException(
                $"Cannot refund transaction with status {InternalStatus}.");

        InternalStatus = PaymentInternalStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (InternalStatus == PaymentInternalStatus.Completed)
            return;

        InternalStatus = PaymentInternalStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        foreach (var item in Items)
            item.MarkCompleted();

        if (OrderId.HasValue)
        {
            RaiseDomainEvent(new PaymentSucceededDomainEvent(
                PaymentTransactionId: Id,
                OrderId: OrderId.Value,
                Amount: Amount,
                CompletedAtUtc: CompletedAt.Value));
        }
    }

    public void MarkItemRefunded(BatchPaymentItem item, Guid userId)
    {
        item.MarkRefunded();

        RaiseDomainEvent(new RefundIssuedDomainEvent(
            PaymentTransactionId: Id,
            OrderId: OrderId ?? Guid.Empty,
            OrderTicketId: item.OrderTicketId,
            EventSessionId: item.EventSessionId,
            UserId: userId,
            Amount: item.Amount,
            RefundedAtUtc: item.RefundedAt!.Value));

        if (IsFullyRefunded())
            MarkRefunded();
    }

    public void UpdateGatewayInfo(
        string? responseCode,
        string? status,
        string? transactionNo,
        string? bankCode,
        string? bankTranNo,
        string? cardType,
        string? payDate,
        string? tmnCode,
        string? secureHash,
        string? orderInfo,
        string? secureHashType = null,
        string? locale = null)
    {
        GatewayResponseCode = responseCode;
        GatewayStatus = status;
        GatewayTransactionNo = transactionNo;
        GatewayBankCode = bankCode;
        GatewayBankTranNo = bankTranNo;
        GatewayCardType = cardType;
        GatewayPayDate = payDate;
        GatewayTmnCode = tmnCode;
        GatewaySecureHash = secureHash;
        GatewaySecureHashType = secureHashType;

        if (!string.IsNullOrWhiteSpace(locale))
            GatewayLocale = locale;

        if (!string.IsNullOrWhiteSpace(orderInfo))
            GatewayOrderInfo = orderInfo;
    }

    public bool IsFullyRefunded()
        => Items.Count > 0
        && Items.All(i => i.InternalStatus == PaymentInternalStatus.Refunded);

    // --------------------
    // Private helpers
    // --------------------
    private static string GetVietnamCreateDate()
    {
        try
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi)
                               .ToString("yyyyMMddHHmmss");
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                var tzi = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi)
                                   .ToString("yyyyMMddHHmmss");
            }
            catch (TimeZoneNotFoundException)
            {
                return DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss");
            }
        }
    }

    private static string NormalizeIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return "127.0.0.1";
        if (ip is "::1" or "[::1]") return "127.0.0.1";

        ip = ip.Trim('[', ']');

        if (ip.Contains(':'))
            return ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase)
                ? ip[7..]
                : "127.0.0.1";

        return ip.Trim();
    }

    protected override void Apply(IDomainEvent @event) { }
}
