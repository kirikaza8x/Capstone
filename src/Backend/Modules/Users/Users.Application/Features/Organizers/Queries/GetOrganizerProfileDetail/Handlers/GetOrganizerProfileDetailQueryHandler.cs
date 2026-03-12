using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Repositories;

public sealed class GetOrganizerProfileDetailQueryHandler
    : IQueryHandler<GetOrganizerProfileDetailQuery, OrganizerProfileResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetOrganizerProfileDetailQueryHandler(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizerProfileResponseDto>> Handle(
        GetOrganizerProfileDetailQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.FirstOrDefaultAsync(
            u => u.Id == query.UserId,
            cancellationToken);

        if (user == null)
        {
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.OrganizerProfiles == null)
        {
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found"));
        }
        var dto = _mapper.Map<OrganizerProfileResponseDto>(user.OrganizerProfiles);

        return Result.Success(dto);
    }
}