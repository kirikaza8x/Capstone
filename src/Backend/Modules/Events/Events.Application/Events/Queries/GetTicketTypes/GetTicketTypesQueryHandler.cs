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
    ITicketingPublicApi ticketingInventoryPublicApi,
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
            return Result.Failure<IReadOnlyList<TicketTypeDto>>(
                EventErrors.Event.NotFound(query.EventId));

        var dtos = mapper.Map<List<TicketTypeDto>>(@event.TicketTypes);

        if (dtos.Count == 0)
            return Result.Success<IReadOnlyList<TicketTypeDto>>(dtos);

        var ticketTypeIds = @event.TicketTypes.Select(tt => tt.Id).ToList();

        // Determine area type
        var areaType = @event.TicketTypes.First().Area?.Type;

        IReadOnlyDictionary<Guid, int> lockedCounts = areaType switch
        {
            AreaType.Zone => await ticketingInventoryPublicApi.GetZoneLockedCountsAsync(
                query.EventSessionId,
                ticketTypeIds,
                cancellationToken),

            AreaType.Seat => await ticketingInventoryPublicApi.GetSeatLockedCountsByTicketTypeAsync(
                query.EventSessionId,
                ticketTypeIds,
                cancellationToken),

            _ => new Dictionary<Guid, int>()
        };

        foreach (var dto in dtos)
        {
            var lockedCount = lockedCounts.TryGetValue(dto.Id, out var count) ? count : 0;
            dto.RemainingQuantity = Math.Max(0, dto.Quantity - dto.SoldQuantity - lockedCount);
        }

        return Result.Success<IReadOnlyList<TicketTypeDto>>(dtos);
    }
}
