using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Repositories;

public sealed class GetMyOrganizerProfileQueryHandler
    : IQueryHandler<GetMyOrganizerProfileQuery, OrganizerProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyOrganizerProfileQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<OrganizerProfileDto>> Handle(
        GetMyOrganizerProfileQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync(
            u => u.Id == _currentUserService.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure<OrganizerProfileDto>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.OrganizerProfiles == null)
        {
            return Result.Failure<OrganizerProfileDto>(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found"));
        }

        var dto = _mapper.Map<OrganizerProfileDto>(user.OrganizerProfiles);

        return Result.Success(dto);
    }
}