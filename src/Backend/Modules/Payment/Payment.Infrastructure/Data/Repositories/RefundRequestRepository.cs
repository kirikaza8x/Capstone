using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Shared.Infrastructure.Data;
using Payments.Infrastructure.Persistence.Contexts;

namespace Payments.Infrastructure.Persistence.Repositories;

public class RefundRequestRepository
    : RepositoryBase<RefundRequest, Guid>, IRefundRequestRepository
{
    public RefundRequestRepository(PaymentModuleDbContext context)
        : base(context) { }

    public async Task<(IReadOnlyList<RefundRequest> Items, int TotalCount)>
        GetPagedByUserIdAsync(
            Guid userId,
            RefundRequestStatus? statusFilter,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(x => x.Status == statusFilter.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<RefundRequest> Items, int TotalCount)> GetPagedAsync(
        RefundRequestStatus? statusFilter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (statusFilter.HasValue)
            query = query.Where(x => x.Status == statusFilter.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.CreatedAt)   // oldest first — review queue
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<bool> HasPendingRequestAsync(
        Guid paymentTransactionId,
        Guid? eventId,
        CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            x => x.PaymentTransactionId == paymentTransactionId
              && x.EventId == eventId
              && x.Status == RefundRequestStatus.Pending,
            cancellationToken);
}