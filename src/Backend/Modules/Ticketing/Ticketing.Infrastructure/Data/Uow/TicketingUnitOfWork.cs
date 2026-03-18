using Shared.Infrastructure.Data;
using Ticketing.Domain.Uow;

namespace Ticketing.Infrastructure.Data.Uow;

public sealed class TicketingUnitOfWork(TicketingDbContext dbContext)
    : UnitOfWorkBase<TicketingDbContext>(dbContext), ITicketingUnitOfWork
{ }