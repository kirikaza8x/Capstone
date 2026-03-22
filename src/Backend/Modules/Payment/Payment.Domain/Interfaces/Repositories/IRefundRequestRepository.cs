using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Shared.Domain.Data.Repositories;

namespace Payments.Domain.Repositories;

public interface IRefundRequestRepository : IRepository<RefundRequest, Guid>
{
    Task<(IReadOnlyList<RefundRequest> Items, int TotalCount)> GetPagedByUserIdAsync(
        Guid userId,
        RefundRequestStatus? statusFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<RefundRequest> Items, int TotalCount)> GetPagedAsync(
        RefundRequestStatus? statusFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingRequestAsync(
        Guid paymentTransactionId,
        Guid? EventSessionId,
        CancellationToken cancellationToken = default);
}
