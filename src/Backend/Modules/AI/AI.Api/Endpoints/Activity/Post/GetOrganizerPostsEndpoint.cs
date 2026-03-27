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

public class GetOrganizerPostsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/organizers/posts", async (
            [AsParameters] GetOrganizerPostsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetOrganizerPostsQuery(
                null,
                request.EventId,
                request.Search,
                request.Status,
                request.SubmittedFrom,
                request.SubmittedTo,
                request.PublishedFrom,
                request.PublishedTo,
                request.IsPublished,
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
        .WithName("GetOrganizerPosts")
        .WithTags("Posts")
        .WithDescription("Get posts belonging to the organizer with filtering and pagination")
        .Produces<PagedResult<PostDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetOrganizerPostsRequestDto
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

    // Filters
    public Guid? EventId { get; init; }
    public string? Search { get; init; }
    public PostStatus? Status { get; init; }
    public DateTime? SubmittedFrom { get; init; }
    public DateTime? SubmittedTo { get; init; }
    public DateTime? PublishedFrom { get; init; }
    public DateTime? PublishedTo { get; init; }
    public bool? IsPublished { get; init; }
    public bool? HasExternalPostUrl { get; init; }
}
