using AI.Application.Features.Tracking.Commands;
using AI.IntegrationEvents.IntergrationEvents;
using MediatR;
using Shared.Application.Abstractions.EventBus;

namespace AI.Application.Features.Tracking.Consumers;

public sealed class TrackingActivityIngestHandler
    : IntegrationEventHandler<TrackUserActivityIntegrationEvent>
{
    private readonly ISender _mediator;
    public TrackingActivityIngestHandler(ISender mediator)
    {
        _mediator = mediator;
    }
    public override async Task Handle(
        TrackUserActivityIntegrationEvent @event,
        CancellationToken ct)
    {
        var command = new TrackActivityCommand(
            @event.UserId,
            @event.ActionType,
            @event.TargetId,
            @event.TargetType,
            @event.Metadata
        );

        await _mediator.Send(command, ct);
    }
}
