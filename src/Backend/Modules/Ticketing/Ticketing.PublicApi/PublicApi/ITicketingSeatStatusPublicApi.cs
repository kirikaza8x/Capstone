namespace Ticketing.PublicApi.PublicApi;

public interface ITicketingSeatStatusPublicApi
{
    Task<IReadOnlySet<Guid>> GetUnavailableSeatIdsAsync(
        Guid eventSessionId,
        IReadOnlyCollection<Guid> seatIds,
        CancellationToken cancellationToken = default);
}
