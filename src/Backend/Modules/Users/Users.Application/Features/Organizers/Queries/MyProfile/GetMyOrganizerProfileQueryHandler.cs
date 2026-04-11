using AutoMapper;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;
using Users.Domain.Entities;
using Users.Domain.Repositories;

public class GetMyOrganizerProfileQueryHandler
    : IQueryHandler<GetMyOrganizerProfileQuery, MyOrganizerProfileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetMyOrganizerProfileQueryHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<MyOrganizerProfileDto>> Handle(
        GetMyOrganizerProfileQuery query,
        CancellationToken cancellationToken)
    {
        Guid userId = _currentUserService.UserId;
        var user = await _userRepository.GetByIdWithOrganizerProfileAsync(userId, cancellationToken);

        if (user == null)
            return Result.Failure<MyOrganizerProfileDto>(
                Error.NotFound("User.NotFound", "User not found"));

        var profile = user.DraftProfile
                      ?? user.PendingProfile
                      ?? user.PublishedProfile;

        if (profile == null)
            return Result.Failure<MyOrganizerProfileDto>(
                Error.NotFound("Organizer.NotFound", "No profile"));

        var dto = _mapper.Map<MyOrganizerProfileDto>(profile);

        return Result.Success(dto);
    }
}
