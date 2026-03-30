using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payment.Domain.ValueObject;
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

    public async Task<IReadOnlyList<EventRevenue>> GetRevenuePerEventAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                x.EventId != null)
            .GroupBy(x => x.EventId!.Value)
            .Select(g => new EventRevenue(
                g.Key,
                g.Sum(x => x.Amount)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<EventRevenue?> GetRevenueByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var revenue = await DbSet
            .Where(x =>
                x.EventId == eventId &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken);

        if (revenue == null) return null;

        return new EventRevenue(eventId, revenue.Value);
    }

    // public async Task<decimal> GetNetRevenueByEventAsync(
    //     Guid eventId,
    //     CancellationToken cancellationToken = default)
    // {
    //     return await DbSet
    //         .Where(x =>
    //             x.EventId == eventId &&
    //             x.InternalStatus == PaymentInternalStatus.Completed)
    //         .Select(x =>
    //             x.Amount - x.Items
    //                 .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
    //                 .Sum(i => (decimal?)i.Amount) ?? 0)
    //         .SumAsync(cancellationToken);
    // }

    // public async Task<IReadOnlyList<EventRevenue>> GetRevenuePerEventAsync(
    //     CancellationToken cancellationToken = default)
    // {
    //     return await DbSet
    //         .Where(x =>
    //             x.InternalStatus == PaymentInternalStatus.Completed &&
    //             x.EventId != null)
    //         .GroupBy(x => x.EventId!.Value)
    //         .Select(g => new EventRevenue(g.Key, g.Sum(x => x.Amount)))
    //         .ToListAsync(cancellationToken);
    // }

    // public async Task<EventRevenue?> GetRevenueByEventAsync(
    //     Guid eventId,
    //     CancellationToken cancellationToken = default)
    // {
    //     var revenue = await DbSet
    //         .Where(x =>
    //             x.EventId == eventId &&
    //             x.InternalStatus == PaymentInternalStatus.Completed)
    //         .SumAsync(x => (decimal?)x.Amount, cancellationToken);

    //     return revenue is null ? null : new EventRevenue(eventId, revenue.Value);
    // }

    // ──────────────────────────────────────────────
    // NET revenue — gross minus refunded item amounts
    // ──────────────────────────────────────────────

    // FIX: original had a ?? 0 precedence bug — the null-coalescing applied
    // only to the inner Sum, not the outer subtraction, which could produce
    // negative net when all items are refunded. Rewritten to be safe.
    public async Task<decimal> GetNetRevenueByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x =>
                x.EventId == eventId &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .Select(x =>
                x.Amount -
                x.Items
                    .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                    .Sum(i => i.Amount))   // Sum returns 0 on empty — no null needed
            .SumAsync(cancellationToken);
    }

    // NEW: net revenue for ALL events in one query (was missing)
    public async Task<IReadOnlyList<EventRevenue>> GetNetRevenuePerEventAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                x.EventId != null)
            .GroupBy(x => x.EventId!.Value)
            .Select(g => new EventRevenue(
                g.Key,
                g.Sum(x =>
                    x.Amount -
                    x.Items
                        .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                        .Sum(i => i.Amount))))
            .ToListAsync(cancellationToken);
    }

    // ──────────────────────────────────────────────
    // REFUND revenue — what was returned to buyers
    // ──────────────────────────────────────────────

    public async Task<decimal> GetTotalRefundsByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x =>
                x.EventId == eventId &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .SelectMany(x => x.Items)
            .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
            .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0m;
    }

    // ──────────────────────────────────────────────
    // REFUND RATE — refund amount / gross amount
    // ──────────────────────────────────────────────

    public async Task<EventRefundRate?> GetRefundRateByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var txns = await DbSet
            .Include(x => x.Items)
            .Where(x =>
                x.EventId == eventId &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (txns.Count == 0) return null;

        var gross = txns.Sum(x => x.Amount);
        var refunds = txns
            .SelectMany(x => x.Items)
            .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
            .Sum(i => i.Amount);

        return new EventRefundRate(
            eventId,
            gross,
            refunds,
            gross > 0 ? Math.Round(refunds / gross * 100, 2) : 0m);
    }

    // ──────────────────────────────────────────────
    // TRANSACTION SUMMARY — counts and payment type breakdown
    // ──────────────────────────────────────────────

    public async Task<EventTransactionSummary?> GetTransactionSummaryByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var groups = await DbSet
            .Where(x => x.EventId == eventId)
            .GroupBy(x => new { x.InternalStatus, x.Type })
            .Select(g => new
            {
                g.Key.InternalStatus,
                g.Key.Type,
                Count = g.Count(),
                TotalAmount = g.Sum(x => x.Amount)
            })
            .ToListAsync(cancellationToken);

        if (groups.Count == 0) return null;

        return new EventTransactionSummary(
            eventId,
            TotalTransactions: groups.Sum(g => g.Count),
            CompletedCount: groups.Where(g => g.InternalStatus == PaymentInternalStatus.Completed).Sum(g => g.Count),
            FailedCount: groups.Where(g => g.InternalStatus == PaymentInternalStatus.Failed).Sum(g => g.Count),
            RefundedCount: groups.Where(g => g.InternalStatus == PaymentInternalStatus.Refunded).Sum(g => g.Count),
            WalletPayAmount: groups.Where(g => g.Type == PaymentType.BatchWalletPay && g.InternalStatus == PaymentInternalStatus.Completed).Sum(g => g.TotalAmount),
            DirectPayAmount: groups.Where(g => g.Type == PaymentType.BatchDirectPay && g.InternalStatus == PaymentInternalStatus.Completed).Sum(g => g.TotalAmount)
        );
    }

    // ──────────────────────────────────────────────
    // PLATFORM-WIDE SUMMARY — admin / global view
    // ──────────────────────────────────────────────

    public async Task<GlobalRevenueSummary> GetGlobalRevenueSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var completed = DbSet
            .Where(x => x.InternalStatus == PaymentInternalStatus.Completed);

        var gross = await completed.SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;
        var refunds = await completed
            .SelectMany(x => x.Items)
            .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
            .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0m;

        var eventCount = await completed
            .Where(x => x.EventId != null)
            .Select(x => x.EventId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new GlobalRevenueSummary(
            GrossRevenue: gross,
            TotalRefunds: refunds,
            NetRevenue: gross - refunds,
            EventCount: eventCount);
    }

    // ──────────────────────────────────────────────
    // TOP N EVENTS — ranked by gross or net
    // ──────────────────────────────────────────────

    public async Task<IReadOnlyList<EventRevenue>> GetTopEventsByRevenueAsync(
        int topN,
        bool byNet = false,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(x =>
                x.InternalStatus == PaymentInternalStatus.Completed &&
                x.EventId != null)
            .GroupBy(x => x.EventId!.Value)
            .Select(g => new
            {
                EventId = g.Key,
                Gross = g.Sum(x => x.Amount),
                Net = g.Sum(x =>
                    x.Amount -
                    x.Items
                        .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                        .Sum(i => i.Amount))
            });

        var results = byNet
            ? await query.OrderByDescending(x => x.Net).Take(topN).ToListAsync(cancellationToken)
            : await query.OrderByDescending(x => x.Gross).Take(topN).ToListAsync(cancellationToken);

        return results
            .Select(x => new EventRevenue(x.EventId, byNet ? x.Net : x.Gross))
            .ToList();
    }

    // GROSS — per event, filtered to organizer's events
    public async Task<IReadOnlyList<EventRevenue>> GetRevenueByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        if (eventIds.Count == 0) return [];

        return await DbSet
            .Where(x =>
                x.EventId != null &&
                eventIds.Contains(x.EventId.Value) &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .GroupBy(x => x.EventId!.Value)
            .Select(g => new EventRevenue(g.Key, g.Sum(x => x.Amount)))
            .ToListAsync(cancellationToken);
    }

    // NET — per event, filtered to organizer's events
    public async Task<IReadOnlyList<EventRevenue>> GetNetRevenueByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        if (eventIds.Count == 0) return [];

        return await DbSet
            .Where(x =>
                x.EventId != null &&
                eventIds.Contains(x.EventId.Value) &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .GroupBy(x => x.EventId!.Value)
            .Select(g => new EventRevenue(
                g.Key,
                g.Sum(x =>
                    x.Amount -
                    x.Items
                        .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
                        .Sum(i => i.Amount))))
            .ToListAsync(cancellationToken);
    }

    // REFUNDS total across all organizer's events
    public async Task<decimal> GetTotalRefundsByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        if (eventIds.Count == 0) return 0m;

        return await DbSet
            .Where(x =>
                x.EventId != null &&
                eventIds.Contains(x.EventId.Value) &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .SelectMany(x => x.Items)
            .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
            .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0m;
    }

    // SUMMARY — gross, net, refunds, event count in one pass
    public async Task<OrganizerRevenueSummary> GetRevenueSummaryByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds,
        CancellationToken cancellationToken = default)
    {
        if (eventIds.Count == 0)
            return new OrganizerRevenueSummary(0m, 0m, 0m, 0);

        var txns = await DbSet
            .Include(x => x.Items)
            .Where(x =>
                x.EventId != null &&
                eventIds.Contains(x.EventId.Value) &&
                x.InternalStatus == PaymentInternalStatus.Completed)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var gross = txns.Sum(x => x.Amount);
        var refunds = txns
            .SelectMany(x => x.Items)
            .Where(i => i.InternalStatus == PaymentInternalStatus.Refunded)
            .Sum(i => i.Amount);

        return new OrganizerRevenueSummary(
            GrossRevenue: gross,
            TotalRefunds: refunds,
            NetRevenue: gross - refunds,
            EventCount: txns.Select(x => x.EventId).Distinct().Count());
    }
}