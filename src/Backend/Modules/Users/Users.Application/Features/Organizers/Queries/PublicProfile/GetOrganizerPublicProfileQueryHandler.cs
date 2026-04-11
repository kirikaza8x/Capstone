using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Organizers.Dtos;
using Users.Application.Features.Organizers.Queries;
using Users.Domain.Repositories;

public class GetOrganizerPublicProfileQueryHandler
    : IQueryHandler<GetOrganizerPublicProfileQuery, OrganizerPublicProfileDto>
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;

    public GetOrganizerPublicProfileQueryHandler(
        IUserRepository repo,
        IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<Result<OrganizerPublicProfileDto>> Handle(
        GetOrganizerPublicProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdWithOrganizerProfileAsync(
            request.UserId,
            cancellationToken);

        if (user == null || user.PublishedProfile == null)
        {
            return Result.Failure<OrganizerPublicProfileDto>(
                Error.NotFound("Organizer.NotFound", "No public profile"));
        }

        return Result.Success(
            _mapper.Map<OrganizerPublicProfileDto>(user.PublishedProfile));
    }
}