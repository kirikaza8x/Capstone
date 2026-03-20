using Payment.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities
{
    public partial class PaymentTransaction : AggregateRoot<Guid>
    {
        // --------------------
        // References
        // --------------------
        public Guid UserId { get; private set; }
        public Guid? EventId { get; private set; }
        public Guid? WalletId { get; private set; }
        public PaymentType Type { get; private set; }

        // --------------------
        // Core transaction info
        // --------------------
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } = "VND";

        // --------------------
        // Internal lifecycle
        // --------------------
        public PaymentInternalStatus InternalStatus { get; private set; }

        // --------------------
        // Gateway fields (null for WalletPay — no gateway involved)
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
        // Factory methods
        // --------------------
        public static PaymentTransaction CreateDirectPay(
            Guid userId,
            Guid eventId,
            decimal amount,
            string? gatewayOrderInfo,
            string? gatewayTxnRef)
        {
            return new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventId = eventId,
                Type = PaymentType.DirectPay,
                Amount = amount,
                InternalStatus = PaymentInternalStatus.AwaitingGateway,
                GatewayOrderInfo = gatewayOrderInfo,
                GatewayTxnRef = gatewayTxnRef,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static PaymentTransaction CreateWalletTopUp(
            Guid userId,
            Guid walletId,
            decimal amount,
            string? gatewayOrderInfo,
            string? gatewayTxnRef)
        {
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

        // No gateway fields — wallet pay is internal, completes immediately
        public static PaymentTransaction CreateWalletPay(
            Guid userId,
            Guid eventId,
            Guid walletId,
            decimal amount,
            string? orderInfo = null)
        {
            return new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EventId = eventId,
                WalletId = walletId,
                Type = PaymentType.WalletPay,
                Amount = amount,
                InternalStatus = PaymentInternalStatus.Completed, // immediate — no gateway roundtrip
                GatewayOrderInfo = orderInfo,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
        }

        // --------------------
        // Domain behaviors
        // --------------------
        public void MarkCompleted()
        {
            InternalStatus = PaymentInternalStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void MarkFailed(string? reason = null)
        {
            InternalStatus = PaymentInternalStatus.Failed;
            FailedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(reason))
                GatewayOrderInfo = string.IsNullOrEmpty(GatewayOrderInfo)
                    ? reason
                    : $"{GatewayOrderInfo} | {reason}";
        }

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

        protected override void Apply(IDomainEvent @event) { }
    }
}
