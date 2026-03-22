using Payments.Domain.Enums;
using Shared.Domain.DDD;

namespace Payments.Domain.Entities;

public class RefundRequest : AggregateRoot<Guid>
{
    // --------------------
    // References
    // --------------------
    public Guid UserId { get; private set; }
    public Guid PaymentTransactionId { get; private set; }
    public Guid? EventId { get; private set; }          // null = FullBatch scope
    public RefundRequestScope Scope { get; private set; }

    // --------------------
    // State
    // --------------------
    public RefundRequestStatus Status { get; private set; }
    public decimal RequestedAmount { get; private set; } // snapshot at submission time

    // --------------------
    // Audit trail
    // --------------------
    public string UserReason { get; private set; } = string.Empty;
    public string? ReviewerNote { get; private set; }
    public Guid? ReviewedByAdminId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    private RefundRequest() { }

    // --------------------
    // Factories
    // --------------------
    public static RefundRequest CreateSingleItem(
        Guid userId,
        Guid paymentTransactionId,
        Guid eventId,
        decimal requestedAmount,
        string userReason)
    {
        EnsureValidReason(userReason);

        return new RefundRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PaymentTransactionId = paymentTransactionId,
            EventId = eventId,
            Scope = RefundRequestScope.SingleItem,
            Status = RefundRequestStatus.Pending,
            RequestedAmount = requestedAmount,
            UserReason = userReason.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public static RefundRequest CreateFullBatch(
        Guid userId,
        Guid paymentTransactionId,
        decimal requestedAmount,
        string userReason)
    {
        EnsureValidReason(userReason);

        return new RefundRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PaymentTransactionId = paymentTransactionId,
            EventId = null,
            Scope = RefundRequestScope.FullBatch,
            Status = RefundRequestStatus.Pending,
            RequestedAmount = requestedAmount,
            UserReason = userReason.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    // --------------------
    // Domain behaviors
    // --------------------
    public void Approve(Guid adminId, string reviewerNote)
    {
        EnsurePending();
        EnsureValidReason(reviewerNote);

        Status = RefundRequestStatus.Approved;
        ReviewedByAdminId = adminId;
        ReviewerNote = reviewerNote.Trim();
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(Guid adminId, string reviewerNote)
    {
        EnsurePending();
        EnsureValidReason(reviewerNote);

        Status = RefundRequestStatus.Rejected;
        ReviewedByAdminId = adminId;
        ReviewerNote = reviewerNote.Trim();
        ReviewedAt = DateTime.UtcNow;
    }

    // --------------------
    // Guards
    // --------------------
    private void EnsurePending()
    {
        if (Status != RefundRequestStatus.Pending)
            throw new InvalidOperationException(
                $"Refund request is already {Status} and cannot be reviewed again.");
    }

    private static void EnsureValidReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required.");
    }

    protected override void Apply(IDomainEvent @event) { }
}