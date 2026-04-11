using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Repositories;
using Shared.Domain.Pagination;
using Shared.Application.Abstractions.Authentication;
using System.Linq.Expressions;
using Marketing.Domain.Entities;

namespace Marketing.Application.Posts.Handlers;

public class GetOrganizerPostsQueryHandler
    : IQueryHandler<GetOrganizerPostsQuery, PagedResult<PostDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetOrganizerPostsQueryHandler(
        IPostRepository postRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PostDto>>> Handle(
    GetOrganizerPostsQuery query,
    CancellationToken cancellationToken)
    {
        Guid? organizerId = _currentUserService.UserId;
        var posts = await _postRepository.GetAllWithPagingAsync(
            query,
            p =>
                // REQUIRED
                p.OrganizerId == organizerId

                // Identity
                && (query.EventId == null || p.EventId == query.EventId)

                // Status
                && (query.Status == null || p.Status == query.Status)

                // Search
                && (
                    string.IsNullOrWhiteSpace(query.Search) ||
                    p.Title.Contains(query.Search) ||
                    (p.Summary != null && p.Summary.Contains(query.Search))
                )

                // Submitted range
                && (
                    query.SubmittedFrom == null ||
                    (p.SubmittedAt != null && p.SubmittedAt >= query.SubmittedFrom)
                )
                && (
                    query.SubmittedTo == null ||
                    (p.SubmittedAt != null && p.SubmittedAt <= query.SubmittedTo)
                )

                // Published range
                && (
                    query.PublishedFrom == null ||
                    (p.PublishedAt != null && p.PublishedAt >= query.PublishedFrom)
                )
                && (
                    query.PublishedTo == null ||
                    (p.PublishedAt != null && p.PublishedAt <= query.PublishedTo)
                )

                // IsPublished
                && (
                    query.IsPublished == null ||
                    (query.IsPublished == true
                        ? p.PublishedAt != null
                        : p.PublishedAt == null)
                )

                // External URL
                // && (
                //     query.HasExternalPostUrl == null ||
                //     (query.HasExternalPostUrl == true
                //         ? p.ExternalPostUrl != null
                //         : p.ExternalPostUrl == null)
                // )
                , includes: new Expression<Func<PostMarketing, object>>[]
                {
                    p => p.ExternalDistributions
                },
                cancellationToken: cancellationToken);

        var dtoItems = _mapper.Map<IReadOnlyList<PostDto>>(posts.Items);

        var dtoPagedResult = new PagedResult<PostDto>(
            dtoItems,
            posts.PageNumber,
            posts.PageSize,
            posts.TotalCount);

        return Result.Success(dtoPagedResult);
    }
}