using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;

public class GetOrganizerDetailQueryHandler
    : IQueryHandler<GetOrganizerDetailQuery, OrganizerProfileResponseDto>
{
    private readonly IOrganizerProfileRepository _repository;
    private readonly IMapper _mapper;

    public GetOrganizerDetailQueryHandler(
        IOrganizerProfileRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<OrganizerProfileResponseDto>> Handle(
        GetOrganizerDetailQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.FirstOrDefaultAsync(
            x => x.Id == query.ProfileId,
            cancellationToken);

        if (profile == null)
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("Organizer.NotFound", "Profile not found"));

        return Result.Success(_mapper.Map<OrganizerProfileResponseDto>(profile));
    }
}