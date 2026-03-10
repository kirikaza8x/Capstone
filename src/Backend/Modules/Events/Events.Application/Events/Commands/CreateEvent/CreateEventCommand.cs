using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.CreateEvent;

public sealed record CreateActorImageItem(
    string Name,
    string? Major,
    string? Image);

public sealed record CreateEventCommand(
    string Title,
    string? BannerUrl,
    List<int> HashtagIds,
    List<int> CategoryIds,
    string Location,
    string? MapUrl,
    string Description,
    List<CreateActorImageItem> ActorImages,
    List<string> ImageUrls) : ICommand<Guid>;