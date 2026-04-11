using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Shared.Infrastructure.Data;
using Shared.Infrastructure.Extensions;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;

namespace Ticketing.Infrastructure.Data.Repositories;

internal sealed class VoucherRepository(TicketingDbContext context)
    : RepositoryBase<Voucher, Guid>(context), IVoucherRepository
{
    private readonly TicketingDbContext _context = context;

    public async Task<Voucher?> GetByCouponCodeAsync(
        string couponCode,
        CancellationToken cancellationToken = default) =>
        await _context.Vouchers
            .FirstOrDefaultAsync(
                v => v.CouponCode == couponCode,
                cancellationToken);

    public async Task<bool> HasUserUsedVoucherAsync(
        Guid voucherId,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _context.OrderVouchers
            .AnyAsync(
                ov => ov.VoucherId == voucherId &&
                      ov.Order.UserId == userId &&
                      ov.Order.Status == OrderStatus.Paid,
                cancellationToken);

    public async Task<bool> IsCouponCodeExistsAsync(
        string couponCode,
        CancellationToken cancellationToken = default) =>
        await _context.Vouchers
            .AnyAsync(v => v.CouponCode == couponCode, cancellationToken);

    public async Task<PagedResult<Voucher>> GetPagedAsync(
        Guid? eventId,
        PagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = _context.Vouchers.AsNoTracking();

        if (eventId.HasValue)
            queryable = queryable.Where(v =>
                v.EventId == eventId.Value || v.EventId == null);

        return await queryable
            .OrderByDescending(v => v.CreatedAt)
            .ToPagedResultAsync(query, cancellationToken);
    }

    public async Task<Dictionary<Guid, Voucher>> GetVoucherMapByIdsAsync(IEnumerable<Guid> voucherIds, CancellationToken cancellationToken = default)
    {
        var vouchers = await _context.Vouchers
            .Where(v => voucherIds.Contains(v.Id))
            .ToListAsync(cancellationToken);

        return vouchers.ToDictionary(v => v.Id);
    }

    public async Task<IReadOnlyList<Voucher>> GetByEventAndCreatorAsync(Guid eventId, Guid createdBy, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .AsNoTracking()
            .Where(v => v.EventId == eventId && v.CreatedBy == createdBy.ToString())
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
