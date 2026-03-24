using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Organizers.Dtos;

namespace Users.Application.Features.Organizers.Queries;
public sealed record GetMyOrganizerProfileQuery(Guid UserId)
    : IQuery<MyOrganizerProfileDto>;