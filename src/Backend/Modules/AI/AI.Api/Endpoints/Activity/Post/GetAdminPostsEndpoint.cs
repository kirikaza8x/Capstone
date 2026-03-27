using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Marketing.Application.Posts.Dtos;
using Marketing.Application.Posts.Queries;
using Marketing.Domain.Enums;
using System.ComponentModel;

namespace Marketing.Api.Posts;

public class GetAdminPostsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/posts", async (
            [AsParameters] GetAdminPostsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAdminPostsQuery(
                request.EventId,
                request.OrganizerId,
                request.Search,
                request.AiModel,
                request.MinAiTokensUsed,
                request.MaxAiTokensUsed,
                request.MinAiCost,
                request.MaxAiCost,
                request.Status,
                request.ReviewedBy,
                request.IsRejected,
                request.ReviewedFrom,
                request.ReviewedTo,
                request.SubmittedFrom,
                request.SubmittedTo,
                request.PublishedFrom,
                request.PublishedTo,
                request.HasExternalPostUrl)
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                SortColumn = request.SortColumn,
                SortOrder = request.SortOrder?.ToLower() == "asc"
                    ? Shared.Domain.Queries.SortOrder.Ascending
                    : Shared.Domain.Queries.SortOrder.Descending
            };

            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetAdminPosts")
        .WithTags("Posts")
        .WithDescription("Get posts with advanced filtering for admin users")
        .Produces<PagedResult<PostDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetAdminPostsRequestDto
{
    // Paging
    [DefaultValue(1)]
    public int PageNumber { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;

    // Sorting
    [DefaultValue("CreatedAt")]
    public string SortColumn { get; init; } = "CreatedAt";

    [DefaultValue("desc")]
    public string SortOrder { get; init; } = "desc";

    // Identity
    public Guid? EventId { get; init; }
    public Guid? OrganizerId { get; init; }

    // Content
    public string? Search { get; init; }

    // AI Metadata
    public string? AiModel { get; init; }
    public int? MinAiTokensUsed { get; init; }
    public int? MaxAiTokensUsed { get; init; }
    public decimal? MinAiCost { get; init; }
    public decimal? MaxAiCost { get; init; }

    // Status
    public PostStatus? Status { get; init; }

    // Moderation
    public Guid? ReviewedBy { get; init; }
    public bool? IsRejected { get; init; }
    public DateTime? ReviewedFrom { get; init; }
    public DateTime? ReviewedTo { get; init; }

    // Publishing
    public DateTime? SubmittedFrom { get; init; }
    public DateTime? SubmittedTo { get; init; }
    public DateTime? PublishedFrom { get; init; }
    public DateTime? PublishedTo { get; init; }

    // External
    public bool? HasExternalPostUrl { get; init; }
}
