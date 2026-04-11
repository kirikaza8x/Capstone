namespace Ticketing.Domain.Uow;

public interface ITicketingUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
