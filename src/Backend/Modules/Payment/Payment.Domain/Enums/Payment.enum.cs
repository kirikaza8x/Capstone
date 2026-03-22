namespace Payment.Domain.Enums;

public enum PaymentType
{
    BatchDirectPay,
    BatchWalletPay,
    WalletTopUp,

}

public enum PaymentInternalStatus
{
    AwaitingGateway,
    Completed,
    Failed,
    Refunded
}

public enum VnPayTransactionStatus
{
    Success = 0,
    Incomplete = 1,
    Error = 2,
    Reversal = 4,
    ProcessingRefund = 5,
    BankRefundRequested = 6,
    SuspectedFraud = 7,
    RefundRejected = 9,
    Unknown = -1
}

public enum VnPayResponseCode
{
    Success = 0,
    SuspectedFraud = 7,
    NoInternetBanking = 9,
    AuthFailed = 10,
    Timeout = 11,
    AccountLocked = 12,
    WrongOtp = 13,
    Cancelled = 24,
    InsufficientFunds = 51,
    LimitExceeded = 65,
    BankMaintenance = 75,
    WrongPassword = 79,
    OtherError = 99,
    Unknown = -1
}