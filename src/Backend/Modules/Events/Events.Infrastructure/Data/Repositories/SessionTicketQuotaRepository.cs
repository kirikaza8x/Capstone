using Events.Domain.Entities;
using Events.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Data.Repositories;

internal sealed class SessionTicketQuotaRepository(EventsDbContext context) : ISessionTicketQuotaRepository
{
    public async Task<List<SessionTicketQuota>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.SessionTicketQuotas
            .Include(q => q.TicketType)
            .Where(q => q.EventSessionId == sessionId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<SessionTicketQuota?> GetBySessionAndTicketTypeAsync(Guid sessionId, Guid ticketTypeId, CancellationToken cancellationToken = default)
    {
        return await context.SessionTicketQuotas
            .FirstOrDefaultAsync(q => q.EventSessionId == sessionId && q.TicketTypeId == ticketTypeId, cancellationToken);
    }

    public void Add(SessionTicketQuota quota) => context.SessionTicketQuotas.Add(quota);

    public void Remove(SessionTicketQuota quota) => context.SessionTicketQuotas.Remove(quota);
}