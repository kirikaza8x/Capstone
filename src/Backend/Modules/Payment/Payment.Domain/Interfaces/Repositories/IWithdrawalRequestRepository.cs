using Payments.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace Payments.Domain.Repositories;

public interface IWithdrawalRequestRepository : IRepository<WithdrawalRequest, Guid>
{
    /// <summary>
    /// Returns the user's current active request (Pending or Approved), or null if none exists.
    /// </summary>
    Task<WithdrawalRequest?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user already has a Pending or Approved request.
    /// Used as a guard before creating a new request.
    /// </summary>
    Task<bool> HasActiveRequestAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}