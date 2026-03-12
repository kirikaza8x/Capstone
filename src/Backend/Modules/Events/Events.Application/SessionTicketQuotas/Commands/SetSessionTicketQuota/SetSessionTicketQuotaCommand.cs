using Shared.Application.Abstractions.Messaging;

namespace Events.Application.SessionTicketQuotas.Commands.SetSessionTicketQuota;

public sealed record SetSessionTicketQuotaCommand(
    Guid EventId,
    Guid SessionId,
    Guid TicketTypeId,
    int Quantity) : ICommand;