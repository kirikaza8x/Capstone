using Events.PublicApi.Records;

namespace Events.PublicApi.PublicApi;

public interface IEventTicketingPublicApi
{
    Task<IReadOnlyDictionary<(Guid SessionId, Guid TicketTypeId), EventTicketingItemDto>> GetTicketingItemsBatchAsync(
        IReadOnlyCollection<(Guid SessionId, Guid TicketTypeId)> pairs,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, EventSeatDto>> GetSeatsBatchAsync(
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default);

    Task<TicketCheckInInfoDto?> GetTicketCheckInInfoAsync(
        Guid ticketTypeId,
        Guid eventSessionId,
        Guid? seatId,
        CancellationToken cancellationToken = default);
}
