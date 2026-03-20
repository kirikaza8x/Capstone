using Microsoft.EntityFrameworkCore;
using Payment.Domain.Enums;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence.Contexts;
using Shared.Infrastructure.Data;

namespace Payments.Infrastructure.Persistence.Repositories;

public class PaymentTransactionRepository : RepositoryBase<PaymentTransaction, Guid>, IPaymentTransactionRepository
{
    public PaymentTransactionRepository(PaymentModuleDbContext context) : base(context)
    {
    }

    public async Task<PaymentTransaction?> GetByTxnRefAsync(string txnRef, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.GatewayTxnRef == txnRef, cancellationToken);
    }

    public async Task<PaymentTransaction?> GetByGatewayTransactionNoAsync(string transactionNo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.GatewayTransactionNo == transactionNo, cancellationToken);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentTransaction>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-1);
        return await DbSet
            .Where(x => x.InternalStatus == PaymentInternalStatus.AwaitingGateway
                     && x.CreatedAt > cutoff)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentTransaction?> GetCompletedByEventIdAsync(
    Guid eventId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.EventId == eventId
                     && x.UserId == userId
                     && x.Type == PaymentType.DirectPay
                     && x.InternalStatus == PaymentInternalStatus.Completed)
            .OrderByDescending(x => x.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<IEnumerable<PaymentTransaction>> GetAllCompletedByEventIdAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(x => x.EventId == eventId
                     && x.InternalStatus == PaymentInternalStatus.Completed
                     && (x.Type == PaymentType.DirectPay || x.Type == PaymentType.WalletPay))
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);
}
