using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

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
            .FirstOrDefaultAsync(
                x => x.GatewayTxnRef == txnRef, cancellationToken);

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

    public async Task<(PaymentTransaction Transaction, BatchPaymentItem Item)?>
        GetCompletedItemBySessionIdAsync(
            Guid eventSessionId,
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var txn = await DbSet
            .Include(x => x.Items)
            .Where(x => x.UserId == userId
                     && x.InternalStatus == PaymentInternalStatus.Completed
                     && x.Items.Any(i =>
                         i.EventSessionId == eventSessionId &&
                         i.InternalStatus == PaymentInternalStatus.Completed))
            .OrderByDescending(x => x.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (txn == null) return null;

        var item = txn.Items.First(i =>
            i.EventSessionId == eventSessionId &&
            i.InternalStatus == PaymentInternalStatus.Completed);

        return (txn, item);
    }

    public async Task<IEnumerable<(PaymentTransaction Transaction, BatchPaymentItem Item)>>
        GetAllCompletedItemsBySessionIdAsync(
            Guid eventSessionId,
            CancellationToken cancellationToken = default)
    {
        var txns = await DbSet
            .Include(x => x.Items)
            .Where(x => x.InternalStatus == PaymentInternalStatus.Completed
                     && x.Items.Any(i =>
                         i.EventSessionId == eventSessionId &&
                         i.InternalStatus == PaymentInternalStatus.Completed))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return txns.SelectMany(txn =>
            txn.Items
               .Where(i => i.EventSessionId == eventSessionId
                        && i.InternalStatus == PaymentInternalStatus.Completed)
               .Select(i => (txn, i)));
    }
}
