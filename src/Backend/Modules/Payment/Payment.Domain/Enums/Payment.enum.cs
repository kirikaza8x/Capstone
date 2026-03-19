
namespace Payment.Domain.Enums;
public enum PaymentType
{
    DirectPay,   // For ticket/event purchase
    WalletTopUp  // For adding funds to wallet
}

public enum VnPayTransactionStatus
{
    Success = 00,
    Incomplete = 01,
    Error = 02,
    Reversal = 04,
    ProcessingRefund = 05,
    BankRefundRequested = 06,
    SuspectedFraud = 07,
    RefundRejected = 09,
    Unknown = -1
}

public enum VnPayResponseCode
{
    Success = 00,
    SuspectedFraud = 07,
    NoInternetBanking = 09,
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
