using AutoMapper;
using Events.Application.Events.DTOs;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi;

namespace Events.Application.Events.Queries.GetTicketTypes;

internal sealed class GetTicketTypesQueryHandler(
    IEventRepository eventRepository,
    ITicketingPublicApi ticketingPublicApi,
    IMapper mapper) : IQueryHandler<GetTicketTypesQuery, IReadOnlyList<TicketTypeDto>>
{
    public async Task<Result<IReadOnlyList<TicketTypeDto>>> Handle(
        GetTicketTypesQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithTicketTypesAsync(
            query.EventId,
            cancellationToken);

        if (@event is null)
        {
            return Result.Failure<IReadOnlyList<TicketTypeDto>>(
                EventErrors.Event.NotFound(query.EventId));
        }

        var dtos = mapper.Map<List<TicketTypeDto>>(@event.TicketTypes);

        if (dtos.Count == 0)
        {
            return Result.Success<IReadOnlyList<TicketTypeDto>>(dtos);
        }

        var ticketTypeIds = dtos.Select(tt => tt.Id).ToList();

        // Determine the AreaType
        var eventAreaType = @event.TicketTypes.First().Area?.Type ?? AreaType.Zone;

        // Get sold counts for each ticket type from the Ticketing Public API
        var soldCounts = await ticketingPublicApi.GetSoldCountsAsync(
            query.EventSessionId,
            ticketTypeIds,
            cancellationToken);

        // Get locked counts for each ticket type from the Ticketing Public API
        IReadOnlyDictionary<Guid, int> lockedCounts;
        if (eventAreaType == AreaType.Seat)
        {
            lockedCounts = await ticketingPublicApi.GetSeatLockedCountsByTicketTypeAsync(
                query.EventSessionId,
                ticketTypeIds,
                cancellationToken);
        }
        else
        {
            lockedCounts = await ticketingPublicApi.GetZoneLockedCountsAsync(
                query.EventSessionId,
                ticketTypeIds,
                cancellationToken);
        }

        foreach (var dto in dtos)
        {
            var soldCount = soldCounts.TryGetValue(dto.Id, out var s) ? s : 0;
            var lockedCount = lockedCounts.TryGetValue(dto.Id, out var l) ? l : 0;

            // calculating remaining quantity by subtracting sold and locked counts from the total quantity
            dto.RemainingQuantity = Math.Max(0, dto.Quantity - soldCount - lockedCount);

            dto.SoldQuantity = soldCount;
        }

        return Result.Success<IReadOnlyList<TicketTypeDto>>(dtos);
    }
}
