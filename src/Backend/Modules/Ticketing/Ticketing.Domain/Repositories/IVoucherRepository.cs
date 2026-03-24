using Shared.Domain.Data.Repositories;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;
using Ticketing.Domain.Entities;

namespace Ticketing.Domain.Repositories;

public interface IVoucherRepository : IRepository<Voucher, Guid>
{
    Task<Voucher?> GetByCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default);

    Task<bool> HasUserUsedVoucherAsync(
        Guid voucherId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> IsCouponCodeExistsAsync(
        string couponCode,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Voucher>> GetPagedAsync(
        Guid? eventId,
        PagedQuery query,
        CancellationToken cancellationToken = default);
}
