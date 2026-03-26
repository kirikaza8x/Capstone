using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Repositories;

namespace Marketing.Application.Posts.Handlers;

public class GetPendingPostsQueryHandler
    : IQueryHandler<GetPendingPostsQuery, IReadOnlyList<PostPendingItemDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetPendingPostsQueryHandler(
        IPostRepository postRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<PostPendingItemDto>>> Handle(
        GetPendingPostsQuery query,
        CancellationToken cancellationToken)
    {
        // ─────────────────────────────────────────────────────────────
        // Fetch pending posts (admin only - auth at endpoint layer)
        // ─────────────────────────────────────────────────────────────
        var posts = await _postRepository.GetPendingQueueAsync(cancellationToken);

        // ─────────────────────────────────────────────────────────────
        // Map base DTOs
        // ─────────────────────────────────────────────────────────────
        var dtos = _mapper.Map<IReadOnlyList<PostPendingItemDto>>(posts);


        return Result.Success<IReadOnlyList<PostPendingItemDto>>(dtos);
    }
}