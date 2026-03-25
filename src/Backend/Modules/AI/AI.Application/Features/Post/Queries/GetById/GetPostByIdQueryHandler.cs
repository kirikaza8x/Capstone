using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Repositories;
using Marketing.Domain.Errors;

namespace Marketing.Application.Posts.Handlers;

public class GetPostByIdQueryHandler
    : IQueryHandler<GetPostByIdQuery, PostDetailDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<PostDetailDto>> Handle(
        GetPostByIdQuery query,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Fetch aggregate
        // ─────────────────────────────────────────────────────────────
        var post = await _postRepository.GetByIdAsync(query.PostId, cancellationToken);

        if (post == null)
        {
            return Result.Failure<PostDetailDto>(
                MarketingErrors.Post.NotFound(query.PostId));
        }

        // ─────────────────────────────────────────────────────────────
        // Authorization: Only organizer or admin can view details
        // ─────────────────────────────────────────────────────────────
        if (!query.IsAdmin && post.OrganizerId != query.RequesterId)
        {
            return Result.Failure<PostDetailDto>(
                MarketingErrors.Post.NotAuthorized(query.RequesterId));
        }

        // ─────────────────────────────────────────────────────────────
        // Map to DTO
        // ─────────────────────────────────────────────────────────────
        var dto = _mapper.Map<PostDetailDto>(post);

        return Result.Success(dto);
    }
}