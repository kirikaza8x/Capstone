using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Organizers.Dtos;

namespace Users.Application.Features.Organizers.Queries.GetOrganizerProfile;



public sealed record GetOrganizerProfileQuery : IQuery<OrganizerProfileResponseDto>;