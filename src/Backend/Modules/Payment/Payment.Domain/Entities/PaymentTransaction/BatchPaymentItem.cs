using Payment.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities;

public class BatchPaymentItem : Entity<Guid>
{
    public Guid PaymentTransactionId { get; private set; }
    public Guid EventId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentInternalStatus InternalStatus { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    private BatchPaymentItem() { }

    public static BatchPaymentItem Create(
        Guid paymentTransactionId,
        Guid eventId,
        decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Item amount must be greater than zero.", nameof(amount));

        return new BatchPaymentItem
        {
            Id = Guid.NewGuid(),
            PaymentTransactionId = paymentTransactionId,
            EventId = eventId,
            Amount = amount,
            InternalStatus = PaymentInternalStatus.AwaitingGateway,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkCompleted()
        => InternalStatus = PaymentInternalStatus.Completed;

    public void MarkFailed()
        => InternalStatus = PaymentInternalStatus.Failed;

    public void MarkRefunded()
    {
        if (InternalStatus != PaymentInternalStatus.Completed)
            throw new InvalidOperationException(
                $"Cannot refund item with status {InternalStatus}.");

        InternalStatus = PaymentInternalStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }
}