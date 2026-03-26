using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Repositories;
using Marketing.Domain.Enums;
using Marketing.Domain.Errors;

namespace Marketing.Application.Posts.Handlers;

public class GetPostsByOrganizerQueryHandler
    : IQueryHandler<GetPostsByOrganizerQuery, IReadOnlyList<PostDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetPostsByOrganizerQueryHandler(
        IPostRepository postRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<PostDto>>> Handle(
        GetPostsByOrganizerQuery query,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Authorization: Only organizer can view their own posts
        // ─────────────────────────────────────────────────────────────
        if (query.OrganizerId != query.RequesterId)
        {
            return Result.Failure<IReadOnlyList<PostDto>>(
                MarketingErrors.Post.NotAuthorized(query.RequesterId));
        }

        // ─────────────────────────────────────────────────────────────
        // Fetch with optional status filter
        // ─────────────────────────────────────────────────────────────
        PostStatus? statusFilter = null;

        if (!string.IsNullOrWhiteSpace(query.StatusFilter) &&
            Enum.TryParse<PostStatus>(query.StatusFilter, true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        var posts = await _postRepository.GetByOrganizerAsync(
            query.OrganizerId,
            statusFilter,
            cancellationToken);

        // ─────────────────────────────────────────────────────────────
        // Map to DTOs
        // ─────────────────────────────────────────────────────────────
        var dtos = _mapper.Map<IReadOnlyList<PostDto>>(posts);

        return Result.Success(dtos);
    }
}