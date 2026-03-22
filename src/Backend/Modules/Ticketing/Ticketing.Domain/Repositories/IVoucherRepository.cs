using Shared.Domain.Data.Repositories;
using Ticketing.Domain.Entities;

namespace Ticketing.Domain.Repositories;

public interface IVoucherRepository : IRepository<Voucher, Guid>
{
    Task<Voucher?> GetByCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default);

    Task<bool> HasUserUsedVoucherAsync(
        Guid voucherId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Voucher?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
