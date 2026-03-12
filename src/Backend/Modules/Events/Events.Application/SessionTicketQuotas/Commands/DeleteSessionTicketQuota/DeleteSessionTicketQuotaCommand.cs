using Shared.Application.Abstractions.Messaging;

namespace Events.Application.SessionTicketQuotas.Commands.DeleteSessionTicketQuota;

public sealed record DeleteSessionTicketQuotaCommand(
    Guid EventId,
    Guid SessionId,
    Guid TicketTypeId) : ICommand;