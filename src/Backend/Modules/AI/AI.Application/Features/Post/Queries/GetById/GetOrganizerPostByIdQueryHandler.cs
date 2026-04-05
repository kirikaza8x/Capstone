using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.DTOs;

namespace Marketing.Application.Posts.Handlers;

public class GetPostByIdQueryHandler
    : IQueryHandler<GetOrganizerPostByIdQuery, PostDetailDto>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<PostDetailDto>> Handle(
        GetOrganizerPostByIdQuery query,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Fetch aggregate
        // ─────────────────────────────────────────────────────────────
        CurrentUserDto? currentUser = _currentUserService.GetCurrentUser();
        Guid requesterId = currentUser?.UserId ?? Guid.Empty;
        var post = await _postRepository.GetByIdWithDistributionsAsync(query.PostId, cancellationToken);

        if (post == null)
        {
            return Result.Failure<PostDetailDto>(
                MarketingErrors.Post.NotFound(query.PostId));
        }

        // Determine if requester is admin
        bool isAdmin = currentUser?.Roles.Contains("Admin") ?? false;

        // Authorization check
        if (post.OrganizerId != requesterId && !isAdmin)
        {
            return Result.Failure<PostDetailDto>(
                MarketingErrors.Post.NotAuthorized(requesterId));
        }

        // ─────────────────────────────────────────────────────────────
        // Map to DTO
        // ─────────────────────────────────────────────────────────────
        var dto = _mapper.Map<PostDetailDto>(post);

        return Result.Success(dto);
    }
}
