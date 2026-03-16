using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Repositories;

public sealed class GetOrganizerProfileDetailQueryHandler
    : IQueryHandler<GetOrganizerProfileDetailQuery, OrganizerProfileResponseDto>
{
    private readonly IOrganizerProfileRepository _profileRepository;
    private readonly IMapper _mapper;

    public GetOrganizerProfileDetailQueryHandler(
        IOrganizerProfileRepository profileRepository,
        IMapper mapper)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizerProfileResponseDto>> Handle(
        GetOrganizerProfileDetailQuery query,
        CancellationToken cancellationToken)
    {
        var organizerProfile = await _profileRepository.GetByIdAsync(query.Id,
            cancellationToken);

        if (organizerProfile == null)
        {
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("Organizer.NotFound", "Organizer profile not found"));
        }

        var dto = _mapper.Map<OrganizerProfileResponseDto>(organizerProfile);

        return Result.Success(dto);
    }
}