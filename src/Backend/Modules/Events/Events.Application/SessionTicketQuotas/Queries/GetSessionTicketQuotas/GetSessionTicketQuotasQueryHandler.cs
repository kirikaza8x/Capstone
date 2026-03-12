using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.SessionTicketQuotas.Queries.GetSessionTicketQuotas;

internal sealed class GetSessionTicketQuotasQueryHandler(
    IEventRepository eventRepository,
    ISessionTicketQuotaRepository quotaRepository) : IQueryHandler<GetSessionTicketQuotasQuery, IReadOnlyList<SessionTicketQuotaResponse>>
{
    public async Task<Result<IReadOnlyList<SessionTicketQuotaResponse>>> Handle(
        GetSessionTicketQuotasQuery query,
        CancellationToken cancellationToken)
    {
        var session = await eventRepository.GetEventSessionByIdAsync(query.SessionId, cancellationToken);

        if (session is null || session.EventId != query.EventId)
            return Result.Failure<IReadOnlyList<SessionTicketQuotaResponse>>(
                EventErrors.EventSessionErrors.NotFound(query.SessionId));

        var quotas = await quotaRepository.GetBySessionIdAsync(query.SessionId, cancellationToken);

        var response = quotas.Select(q => new SessionTicketQuotaResponse(
            q.TicketTypeId,
            q.TicketType.Name,
            q.TicketType.Price,
            q.Quantity)).ToList();

        return Result.Success<IReadOnlyList<SessionTicketQuotaResponse>>(response);
    }
}