using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Infrastructure.Data;
using Payments.Infrastructure.Persistence.Contexts;

namespace Payments.Infrastructure.Persistence.Repositories;

public class PaymentTransactionRepository
    : RepositoryBase<PaymentTransaction, Guid>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(PaymentModuleDbContext context)
        : base(context) { }

    public async Task<PaymentTransaction?> GetByTxnRefWithItemsAsync(
        string txnRef,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.GatewayTxnRef == txnRef, cancellationToken);

    public async Task<PaymentTransaction?> GetByIdWithItemsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<PaymentTransaction> Items, int TotalCount)>
        GetPagedByUserIdAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-1);

        return await DbSet
            .Where(x => x.InternalStatus == PaymentInternalStatus.AwaitingGateway
                     && x.CreatedAt > cutoff)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // Used by mass refund — all users, all completed items for a given event
    public async Task<IEnumerable<(PaymentTransaction Transaction, BatchPaymentItem Item)>>
        GetAllCompletedItemsByEventIdAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
    {
        var txns = await DbSet
            .Include(x => x.Items)
            .Where(x => x.InternalStatus == PaymentInternalStatus.Completed
                     && x.Items.Any(i => i.EventId == eventId
                                      && i.InternalStatus == PaymentInternalStatus.Completed))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return txns.SelectMany(txn =>
            txn.Items
               .Where(i => i.EventId == eventId
                        && i.InternalStatus == PaymentInternalStatus.Completed)
               .Select(i => (txn, i)));
    }

    // Used by single-user refund request submission
    public async Task<(PaymentTransaction? Transaction, BatchPaymentItem? Item)>
        GetCompletedItemByEventIdAsync(
            Guid eventId,
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var txn = await DbSet
            .Include(x => x.Items)
            .Where(x => x.UserId == userId
                     && x.InternalStatus == PaymentInternalStatus.Completed
                     && x.Items.Any(i => i.EventId == eventId
                                      && i.InternalStatus == PaymentInternalStatus.Completed))
            .OrderByDescending(x => x.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (txn == null) return (null, null);

        var item = txn.Items.First(i =>
            i.EventId == eventId &&
            i.InternalStatus == PaymentInternalStatus.Completed);

        return (txn, item);
    }
}