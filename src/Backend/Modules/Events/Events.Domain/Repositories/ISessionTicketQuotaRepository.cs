using Events.Domain.Entities;

namespace Events.Domain.Repositories;

public interface ISessionTicketQuotaRepository
{
    Task<List<SessionTicketQuota>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<SessionTicketQuota?> GetBySessionAndTicketTypeAsync(Guid sessionId, Guid ticketTypeId, CancellationToken cancellationToken = default);
    void Add(SessionTicketQuota quota);
    void Remove(SessionTicketQuota quota);
}