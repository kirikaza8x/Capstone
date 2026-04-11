using AutoMapper;
using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;
using Users.Domain.Entities;
using Users.Domain.Repositories;

public class GetOrganizerDetailQueryHandler
    : IQueryHandler<GetOrganizerDetailQuery, OrganizerProfileResponseDto>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrganizerDetailQueryHandler> _logger;

    public GetOrganizerDetailQueryHandler(
        IUserRepository repository,
        IMapper mapper,
        ILogger<GetOrganizerDetailQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrganizerProfileResponseDto>> Handle(
        GetOrganizerDetailQuery query,
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdWithOrganizerProfileAsync(query.ProfileId,
            cancellationToken);

        var profile = user?.DraftProfile
                      ?? user?.PendingProfile
                      ?? user?.PublishedProfile;
        if (profile == null)
        {
            _logger.LogWarning("Organizer profile with Id {ProfileId} not found", query.ProfileId);
            return Result.Failure<OrganizerProfileResponseDto>(
                Error.NotFound("Organizer.NotFound", "Profile not found"));
        }


        var dto = _mapper.Map<OrganizerProfileResponseDto>(profile);


        return Result.Success(dto);
    }
}
