using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Domain.Repositories;

namespace Users.Application.Features.Organizers.Queries.GetOrganizerProfile;

internal sealed class GetOrganizerProfileQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService,
    IMapper mapper) : IQueryHandler<GetOrganizerProfileQuery, OrganizerProfileResponse>
{
    public async Task<Result<OrganizerProfileResponse>> Handle(
        GetOrganizerProfileQuery query,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdWithOrganizerProfileAsync(
            currentUserService.UserId,
            cancellationToken);

        if (user is null)
            return Result.Failure<OrganizerProfileResponse>(
                Error.NotFound("User.NotFound", "User not found."));

        if (user.OrganizerProfile is null)
            return Result.Failure<OrganizerProfileResponse>(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found."));

        return Result.Success(mapper.Map<OrganizerProfileResponse>(user.OrganizerProfile));
    }
}