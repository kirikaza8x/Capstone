using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Data;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Repositories;

namespace Ticketing.Infrastructure.Data.Repositories;

internal sealed class VoucherRepository(TicketingDbContext context)
    : RepositoryBase<Voucher, Guid>(context), IVoucherRepository
{
    private readonly TicketingDbContext _context = context;

    public async Task<Voucher?> GetByCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .FirstOrDefaultAsync(x => x.CouponCode == couponCode, cancellationToken);
    }

    public async Task<bool> ExistsCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default)
    {
        return await _context.Vouchers
            .AnyAsync(x => x.CouponCode == couponCode, cancellationToken);
    }
}
