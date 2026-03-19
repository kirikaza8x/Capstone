
namespace Payments.Domain.Enums;

public enum WalletStatus
{
    Active,
    Suspended,
    Closed
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Adjustment
}

public enum TransactionDirection
{
    In,
    Out
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed
}