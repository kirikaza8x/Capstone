using System.Text.Json;
using Events.Application.Events.Commands.UpdateEventSpec;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.PublicApi.PublicApi;

namespace Events.Application.Events.Queries.GetEventSpec;

internal sealed class GetEventSpecQueryHandler(
    IEventRepository eventRepository,
    ITicketingSeatStatusPublicApi ticketingSeatStatusPublicApi) : IQueryHandler<GetEventSpecQuery, GetEventSpecResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public async Task<Result<GetEventSpecResponse>> Handle(
        GetEventSpecQuery query,
        CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(query.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure<GetEventSpecResponse>(EventErrors.Event.NotFound(query.EventId));

        if (string.IsNullOrWhiteSpace(@event.Spec))
        {
            return Result.Success(new GetEventSpecResponse(
                @event.Id,
                query.EventSessionId,
                @event.Spec));
        }

        SeatMapModel? seatMap;
        try
        {
            seatMap = JsonSerializer.Deserialize<SeatMapModel>(@event.Spec, JsonOptions);
        }
        catch (JsonException ex)
        {
            return Result.Failure<GetEventSpecResponse>(Error.Validation(
                "Event.InvalidSpec",
                $"Spec JSON parse error: {ex.Message}"));
        }

        if (seatMap?.Areas is null || seatMap.Areas.Count == 0)
        {
            return Result.Success(new GetEventSpecResponse(
                @event.Id,
                query.EventSessionId,
                @event.Spec));
        }

        var seatIds = seatMap.Areas
            .Where(a => a.Seats is { Count: > 0 })
            .SelectMany(a => a.Seats!)
            .Select(s => Guid.TryParse(s.Id, out var seatId) ? seatId : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (seatIds.Count > 0)
        {
            var unavailableSeatIds = await ticketingSeatStatusPublicApi.GetUnavailableSeatIdsAsync(
                query.EventSessionId,
                seatIds,
                cancellationToken);

            foreach (var area in seatMap.Areas)
            {
                if (area.Seats is null || area.Seats.Count == 0)
                    continue;

                foreach (var seat in area.Seats)
                {
                    if (!Guid.TryParse(seat.Id, out var seatId))
                        continue;

                    // check if the seat is unavailable for ticketing
                    bool isTicketingBlocked = unavailableSeatIds.Contains(seatId);

                    bool isOrganizerBlocked = seat.ParsedStatus == SeatMapSeatStatus.blocked;

                    seat.Status = isTicketingBlocked || isOrganizerBlocked
                        ? nameof(SeatMapSeatStatus.blocked)
                        : nameof(SeatMapSeatStatus.available);
                }
            }
        }

        var enrichedSpec = JsonSerializer.Serialize(seatMap, JsonOptions);

        return Result.Success(new GetEventSpecResponse(
            @event.Id,
            query.EventSessionId,
            enrichedSpec));
    }
}
