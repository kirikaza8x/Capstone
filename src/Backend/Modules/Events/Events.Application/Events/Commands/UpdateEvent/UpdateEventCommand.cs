using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.UpdateEvent;

public sealed record UpdateActorImageItem(
    string Name,
    string? Major,
    string? Image);

public sealed record UpdateEventCommand(
    Guid EventId,
    string? Title,
    List<int>? HashtagIds,
    List<int>? CategoryIds,
    string? Location,
    string? MapUrl,
    string? Description,
    List<UpdateActorImageItem>? ActorImages) : ICommand;
