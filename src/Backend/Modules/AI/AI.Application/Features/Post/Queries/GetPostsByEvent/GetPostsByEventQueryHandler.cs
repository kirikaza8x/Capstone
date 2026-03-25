using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Entities;
using Marketing.Domain.Repositories;

namespace Marketing.Application.Posts.Handlers;

public class GetPostsByEventQueryHandler
    : IQueryHandler<GetPostsByEventQuery, IReadOnlyList<PostDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetPostsByEventQueryHandler(
        IPostRepository postRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<PostDto>>> Handle(
        GetPostsByEventQuery query,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Build filter based on requester role
        // ─────────────────────────────────────────────────────────────
        IReadOnlyList<PostMarketing> posts;

        if (query.IsOrganizer)
        {
            // Organizer sees all their posts for this event
            posts = await _postRepository.GetByEventAndOrganizerAsync(
                query.EventId,
                query.RequesterId,
                cancellationToken);
        }
        else if (query.IncludeDrafts)
        {
            // Admin/internal: can see drafts
            posts = await _postRepository.FindAsync(
                p => p.EventId == query.EventId,
                cancellationToken);
        }
        else
        {
            // Public: only published posts
            posts = await _postRepository.GetPublishedByEventAsync(
                query.EventId,
                cancellationToken);
        }

        // ─────────────────────────────────────────────────────────────
        // Map to DTOs
        // ─────────────────────────────────────────────────────────────
        var dtos = _mapper.Map<IReadOnlyList<PostDto>>(posts);

        return Result.Success(dtos);
    }
}