using Shared.Application.Messaging;

namespace Events.Application.Events.Commands.CreateEvent;

public sealed record CreateEventCommand(
    Guid OrganizerId,
    string Title,
    DateTime TicketSaleStartAt,
    DateTime TicketSaleEndAt,
    DateTime EventStartAt,
    DateTime EventEndAt,
    string Description,
    string? BannerUrl,
    string Location,
    string? MapUrl,
    string Policy,
    string UrlPath,
    int EventTypeId,
    int EventCategoryId) : ICommand<Guid>;