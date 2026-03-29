using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

namespace Payments.Infrastructure.Persistence.Repositories;

public class WithdrawalRequestRepository
    : RepositoryBase<WithdrawalRequest, Guid>, IWithdrawalRequestRepository
{
    private static readonly WithdrawalRequestStatus[] ActiveStatuses =
    [
        WithdrawalRequestStatus.Pending,
        WithdrawalRequestStatus.Approved,
    ];

    public WithdrawalRequestRepository(PaymentModuleDbContext context)
        : base(context) { }

    public async Task<WithdrawalRequest?> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .FirstOrDefaultAsync(
                r => r.UserId == userId && ActiveStatuses.Contains(r.Status),
                cancellationToken);

    public async Task<bool> HasActiveRequestAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .AnyAsync(
                r => r.UserId == userId && ActiveStatuses.Contains(r.Status),
                cancellationToken);
}