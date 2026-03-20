using Payment.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities
{


    public partial class PaymentTransaction : AggregateRoot<Guid>
    {
        // --------------------
        // References
        // --------------------
        public Guid UserId { get; private set; }        // internal user reference
        public Guid? EventId { get; private set; }      // set if DirectPay
        public Guid? WalletId { get; private set; }     // set if WalletTopUp
        public PaymentType Type { get; private set; }   // flag for flow type

        // --------------------
        // Core transaction info
        // --------------------
        public decimal Amount { get; private set; }     // vnp_Amount
        public string Currency { get; private set; } = "VND"; // vnp_CurrCode

        public string? GatewayTransactionNo { get; private set; } // vnp_TransactionNo
        public string? GatewayResponseCode { get; private set; }  // vnp_ResponseCode
        public string? GatewayStatus { get; private set; }        // vnp_TransactionStatus
        public string? GatewayOrderInfo { get; private set; }     // vnp_OrderInfo
        public string? GatewayTxnRef { get; private set; }        // vnp_TxnRef
        public string? GatewayBankCode { get; private set; }      // vnp_BankCode
        public string? GatewayBankTranNo { get; private set; }    // vnp_BankTranNo
        public string? GatewayCardType { get; private set; }      // vnp_CardType
        public string? GatewayPayDate { get; private set; }       // vnp_PayDate
        public string? GatewayTmnCode { get; private set; }       // vnp_TmnCode
        public string? GatewaySecureHash { get; private set; }    // vnp_SecureHash
        public string? GatewaySecureHashType { get; private set; }// vnp_SecureHashType
        public string? GatewayLocale { get; private set; }        // vnp_Locale
        public string? GatewayIpAddr { get; private set; }        // vnp_IpAddr
        public string? GatewayCreateDate { get; private set; }    // vnp_CreateDate
        public string? GatewayOrderType { get; private set; }     // vnp_OrderType
        public string? GatewayMerchant { get; private set; }      // vnp_Merchant (optional)

        // --------------------
        // Lifecycle
        // --------------------
        public DateTime? CompletedAt { get; private set; }

        private PaymentTransaction() { }

        // --------------------
        // Factory methods
        // --------------------
        public static PaymentTransaction CreateDirectPay(
            Guid userId,
            Guid eventId,
            decimal amount,
            string? gatewayTransactionNo,
            string? gatewayResponseCode,
            string? gatewayStatus,
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
                GatewayTransactionNo = gatewayTransactionNo,
                GatewayResponseCode = gatewayResponseCode,
                GatewayStatus = gatewayStatus,
                GatewayOrderInfo = gatewayOrderInfo,
                GatewayTxnRef = gatewayTxnRef,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static PaymentTransaction CreateWalletTopUp(
            Guid userId,
            Guid walletId,
            decimal amount,
            string? gatewayTransactionNo,
            string? gatewayResponseCode,
            string? gatewayStatus,
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
                GatewayTransactionNo = gatewayTransactionNo,
                GatewayResponseCode = gatewayResponseCode,
                GatewayStatus = gatewayStatus,
                GatewayOrderInfo = gatewayOrderInfo,
                GatewayTxnRef = gatewayTxnRef,
                CreatedAt = DateTime.UtcNow
            };
        }

        // --------------------
        // Domain behaviors
        // --------------------
        public void MarkCompleted() => CompletedAt = DateTime.UtcNow;

        public void MarkFailed(string? reason = null)
        {
            GatewayStatus = "Failed";
            GatewayResponseCode = "99"; // fallback error
            if (!string.IsNullOrWhiteSpace(reason))
                GatewayOrderInfo = $"{GatewayOrderInfo} | Failed: {reason}";
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
