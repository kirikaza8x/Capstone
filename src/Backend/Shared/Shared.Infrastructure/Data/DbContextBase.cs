using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Data;

namespace Shared.Infrastructure.Data;

public abstract class DbContextBase : DbContext, IUnitOfWork
{

    private IDbContextTransaction? currentTransaction;

    protected DbContextBase(DbContextOptions options) : base(options)
    {
    }

    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is not null)
        {
            return; // Already in transaction
        }

        currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        try
        {
            await SaveChangesAsync(cancellationToken);

            await currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is not null)
        {
            try
            {
                await currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    private async Task DisposeTransactionAsync()
    {
        if (currentTransaction is not null)
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }
    }
}
