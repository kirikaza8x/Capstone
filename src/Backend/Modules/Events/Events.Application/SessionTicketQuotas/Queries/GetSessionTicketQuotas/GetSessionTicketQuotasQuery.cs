using Shared.Application.Abstractions.Messaging;

namespace Events.Application.SessionTicketQuotas.Queries.GetSessionTicketQuotas;

public sealed record GetSessionTicketQuotasQuery(Guid EventId, Guid SessionId)
    : IQuery<IReadOnlyList<SessionTicketQuotaResponse>>;

public sealed record SessionTicketQuotaResponse(
    Guid TicketTypeId,
    string TicketTypeName,
    decimal TicketTypePrice,
    int Quantity);