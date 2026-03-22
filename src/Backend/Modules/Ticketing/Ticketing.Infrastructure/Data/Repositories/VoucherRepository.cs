using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Ticketing.Domain.Entities;
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
                      ov.Order.UserId == userId,
                cancellationToken);

    public async Task<Voucher?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
}
