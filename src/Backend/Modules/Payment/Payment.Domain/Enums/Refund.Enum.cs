namespace Payment.Domain.Enums;

public enum RefundRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public enum RefundRequestScope
{
    SingleItem,
    FullBatch
}
