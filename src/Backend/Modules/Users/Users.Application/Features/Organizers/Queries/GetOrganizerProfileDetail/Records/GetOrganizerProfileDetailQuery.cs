using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Organizers.Dtos;

public sealed record GetOrganizerProfileDetailQuery(Guid Id)
    : IQuery<OrganizerProfileResponseDto>;
