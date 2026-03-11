using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Repositories;

public sealed class GetOrganizerPublicProfileQueryHandler
    : IQueryHandler<GetOrganizerPublicProfileQuery, OrganizerPublicProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetOrganizerPublicProfileQueryHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizerPublicProfileDto>> Handle(
        GetOrganizerPublicProfileQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithOrganizerProfileAsync(
            query.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure<OrganizerPublicProfileDto>(
                Error.NotFound(
                    "User.NotFound",
                    "User not found"));
        }

        var publishedProfile = user.PublishedProfile;

        if (publishedProfile == null)
        {
            return Result.Failure<OrganizerPublicProfileDto>(
                Error.NotFound(
                    "Organizer.NotFound",
                    "Organizer profile not found"));
        }

        var dto = _mapper.Map<OrganizerPublicProfileDto>(publishedProfile);

        return Result.Success(dto);
    }
}