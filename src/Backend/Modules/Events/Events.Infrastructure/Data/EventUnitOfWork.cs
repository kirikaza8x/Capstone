using Events.Domain.Uow;
using Shared.Infrastructure.Data;

namespace Events.Infrastructure.Data;

public sealed class EventUnitOfWork(EventsDbContext dbContext)
    : UnitOfWorkBase<EventsDbContext>(dbContext), IEventUnitOfWork
{ }
