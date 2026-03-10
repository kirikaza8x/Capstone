using Events.Domain.Entities;
using Events.Domain.Enums;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using FluentValidation;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using System.Text.Json;

namespace Events.Application.Events.Commands.UpdateEventSpec;

public sealed class UpdateEventSpecCommandValidator : AbstractValidator<UpdateEventSpecCommand>
{
    public UpdateEventSpecCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Event ID is required.");

        RuleFor(x => x.Spec)
            .NotNull().WithMessage("Spec is required.");
    }
}

internal sealed class UpdateEventSpecCommandHandler(
    IEventRepository eventRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<UpdateEventSpecCommand>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<Result> Handle(UpdateEventSpecCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdWithAreasAndSeatsAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var specJson = command.Spec.RootElement.GetRawText();

        SeatMapModel? seatMap;
        try
        {
            seatMap = JsonSerializer.Deserialize<SeatMapModel>(specJson, JsonOptions);
        }
        catch (JsonException ex)
        {
            return Result.Failure(Error.Validation("Event.InvalidSpec", $"Spec JSON parse error: {ex.Message}"));
        }

        if (seatMap?.Areas is null || seatMap.Areas.Count == 0)
            return Result.Failure(EventErrors.Event.SpecHasNoAreas);

        @event.UpdateSpec(specJson);
        @event.ClearAreasAndSeats();

        foreach (var specArea in seatMap.Areas)
        {
            var hasSeats = specArea.Seats is { Count: > 0 };
            var areaType = hasSeats ? AreaType.Seat : AreaType.Zone;
            var capacity = hasSeats ? specArea.Seats!.Count : 0;

            var area = Area.Create(
                eventId: @event.Id,
                name: specArea.Name,
                capacity: capacity,
                type: areaType);

            if (hasSeats)
            {
                foreach (var specSeat in specArea.Seats!)
                {
                    var seat = Seat.Create(
                        areaId: area.Id,
                        seatCode: $"{specSeat.Row}{specSeat.Number}",
                        rowLabel: specSeat.Row,
                        columnLabel: specSeat.Number.ToString(),
                        x: specSeat.X,
                        y: specSeat.Y);

                    if (specSeat.ParsedStatus == SeatMapSeatStatus.blocked)
                        seat.Deactivate();

                    area.AddSeat(seat);
                }
            }

            @event.AddArea(area);
        }

        eventRepository.Update(@event);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}