using Events.PublicApi.Records;

namespace Events.PublicApi.PublicApi;

public interface IEventTicketingPublicApi
{
    Task<EventTicketingItemDto?> GetTicketingItemAsync(
        Guid eventSessionId,
        Guid ticketTypeId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<EventSeatDto?> GetSeatAsync(
        Guid seatId,
        CancellationToken cancellationToken = default);
}