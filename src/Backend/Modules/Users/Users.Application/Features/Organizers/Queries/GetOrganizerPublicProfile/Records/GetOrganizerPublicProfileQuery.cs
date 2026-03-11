using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Organizers.Dtos;

public sealed record GetOrganizerPublicProfileQuery(Guid UserId)
    : IQuery<OrganizerPublicProfileDto>;