using Shared.Domain.Data.Repositories;
using Ticketing.Domain.Entities;

namespace Ticketing.Domain.Repositories;

public interface IVoucherRepository : IRepository<Voucher, Guid>
{
    Task<Voucher?> GetByCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default);
    Task<bool> ExistsCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default);
}