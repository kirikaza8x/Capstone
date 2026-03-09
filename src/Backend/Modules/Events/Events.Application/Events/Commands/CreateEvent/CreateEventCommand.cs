using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.CreateEvent;

public sealed record CreateActorImageItem(
    string Name,
    string? Major,
    string? Image);

public sealed record CreateEventCommand(
    Guid OrganizerId,
    string Title,
    string? BannerUrl,
    List<int> HashtagIds,
    int EventCategoryId,
    string Location,
    string? MapUrl,
    string Description,
    List<CreateActorImageItem> ActorImages) : ICommand<Guid>;