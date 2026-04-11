using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Repositories;
using Shared.Domain.Pagination;
using System.Linq.Expressions;
using Marketing.Domain.Entities;

namespace Marketing.Application.Posts.Handlers;

public class GetAdminPostsQueryHandler
    : IQueryHandler<GetAdminPostsQuery, PagedResult<PostDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetAdminPostsQueryHandler(
        IPostRepository postRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PostDto>>> Handle(
    GetAdminPostsQuery query,
    CancellationToken cancellationToken)
    {
        var posts = await _postRepository.GetAllWithPagingAsync(
            query,
            p =>
                // ─────────────────────────────
                // Identity
                // ─────────────────────────────
                (query.EventId == null || p.EventId == query.EventId)
                &&
                (query.OrganizerId == null || p.OrganizerId == query.OrganizerId)

                // ─────────────────────────────
                // Content
                // ─────────────────────────────
                && (
                string.IsNullOrWhiteSpace(query.Search)
                || p.Title.Contains(query.Search)
                || p.Slug.Contains(query.Search)
                || (p.Summary != null && p.Summary.Contains(query.Search))
                )

                // ─────────────────────────────
                // Status
                // ─────────────────────────────
                && (query.Status == null || p.Status == query.Status)

                // ─────────────────────────────
                // Moderation
                // ─────────────────────────────
                && (query.ReviewedBy == null || p.ReviewedBy == query.ReviewedBy)

                && (
                    query.IsRejected == null ||
                    (query.IsRejected == true
                        ? p.RejectionReason != null
                        : p.RejectionReason == null)
                )

                && (
                    query.ReviewedFrom == null ||
                    (p.ReviewedAt != null && p.ReviewedAt >= query.ReviewedFrom)
                )
                && (
                    query.ReviewedTo == null ||
                    (p.ReviewedAt != null && p.ReviewedAt <= query.ReviewedTo)
                )

                // ─────────────────────────────
                // Submitted
                // ─────────────────────────────
                && (
                    query.SubmittedFrom == null ||
                    (p.SubmittedAt != null && p.SubmittedAt >= query.SubmittedFrom)
                )
                && (
                    query.SubmittedTo == null ||
                    (p.SubmittedAt != null && p.SubmittedAt <= query.SubmittedTo)
                )

                // ─────────────────────────────
                // Published
                // ─────────────────────────────
                && (
                    query.PublishedFrom == null ||
                    (p.PublishedAt != null && p.PublishedAt >= query.PublishedFrom)
                )
                && (
                    query.PublishedTo == null ||
                    (p.PublishedAt != null && p.PublishedAt <= query.PublishedTo)
                )

                // ─────────────────────────────
                // External
                // ─────────────────────────────
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

        return Result.Success(new PagedResult<PostDto>(
            dtoItems,
            posts.PageNumber,
            posts.PageSize,
            posts.TotalCount));
    }
}
