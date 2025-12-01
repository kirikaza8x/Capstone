using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain.Common.DDD;
using Shared.Application.Abstractions.Authentication;

namespace Infrastructure.Data.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;

        public AuditableEntityInterceptor(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        private void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            var name = _currentUser.Name ?? "System";

            foreach (var entry in context.ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = name;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                }

                if (
                    //entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.HasChangeOwnedEntities()
                    )
                {
                    entry.Entity.ModifiedBy = name;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    public static class EntryExtensions
    {
        public static bool HasChangeOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added ||
                 r.TargetEntry.State == EntityState.Modified ||
                 r.TargetEntry.HasChangeOwnedEntities()));
    }
}
