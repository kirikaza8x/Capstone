using Shared.Application.Abstractions.Messaging;

namespace Events.Application.Events.Commands.CreateEvent;

public sealed record CreateEventCommand(
    Guid OrganizerId,
    string Title,
    string? BannerUrl,
    List<int> HashtagIds,
    int EventCategoryId,
    string Location,
    string? MapUrl,
    string Description
) : ICommand<Guid>;