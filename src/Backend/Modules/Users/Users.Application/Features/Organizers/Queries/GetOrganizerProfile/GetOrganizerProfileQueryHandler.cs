using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Organizers.Queries.GetOrganizerProfile;

internal sealed class GetOrganizerProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IMapper mapper) : IQueryHandler<GetOrganizerProfileQuery, OrganizerProfileResponseDto>
{
    public async Task<Result<OrganizerProfileResponseDto>> Handle(
        GetOrganizerProfileQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithOrganizerProfileAsync(
            currentUserService.UserId,
            cancellationToken);

        if (user is null)
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("User.NotFound", "User not found."));

        var organizerProfile = user.OrganizerProfiles?.FirstOrDefault();

        if (organizerProfile is null)
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found."));

        return Result.Success(mapper.Map<OrganizerProfileResponseDto>(organizerProfile));
    }
}
