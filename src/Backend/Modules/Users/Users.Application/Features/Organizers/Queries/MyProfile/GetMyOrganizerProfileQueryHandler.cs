using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;
using Users.Domain.Enums;
using Users.Domain.Repositories;

public class GetMyOrganizerProfileQueryHandler
    : IQueryHandler<GetMyOrganizerProfileQuery, MyOrganizerProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetMyOrganizerProfileQueryHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<MyOrganizerProfileDto>> Handle(
        GetMyOrganizerProfileQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return Result.Failure<MyOrganizerProfileDto>(
                Error.NotFound("User.NotFound", "User not found"));

        var profiles = user.OrganizerProfiles;

        if (profiles == null || !profiles.Any())
            return Result.Failure<MyOrganizerProfileDto>(
                Error.NotFound("Organizer.NotFound", "No profile"));

        var profile =
            profiles.FirstOrDefault(x => x.Status == OrganizerStatus.Draft) ??
            profiles.FirstOrDefault(x => x.Status == OrganizerStatus.Pending) ??
            profiles.FirstOrDefault(x => x.Status == OrganizerStatus.Verified);

        var dto = _mapper.Map<MyOrganizerProfileDto>(profile);

        return Result.Success(dto);
    }
}