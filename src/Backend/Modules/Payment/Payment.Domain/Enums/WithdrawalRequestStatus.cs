namespace Payments.Domain.Enums;

public enum WithdrawalRequestStatus
{
    Pending,    // Submitted by user, awaiting admin review
    Approved,   // Admin approved, transfer to be done manually
    Rejected,   // Admin rejected the request
    Completed,  // Admin confirmed the bank transfer is done
    Cancelled   // Cancelled by the user before admin action
}