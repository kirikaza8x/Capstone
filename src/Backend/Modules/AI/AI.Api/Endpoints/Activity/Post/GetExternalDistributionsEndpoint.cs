using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using System.Linq.Dynamic.Core;
using Marketing.Application.Posts.Dtos;
using System.ComponentModel;
using Marketing.Domain.Enums;


namespace Marketing.Api.ExternalDistributions;

public class GetExternalDistributionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/external-distributions", async (
            [AsParameters] GetExternalDistributionsRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetExternalDistributionsQuery(
                request.PostMarketingId,
                request.Platform,
                request.ExternalUrl,
                request.ExternalPostId,
                request.Status,
                request.PlatformMetadata,
                request.HasError,
                request.SentFrom,
                request.SentTo)
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
        .WithName("GetExternalDistributions")
        .WithTags("ExternalDistributions")
        .WithDescription("Get external distributions with advanced filtering for admin users")
        .Produces<PagedResult<ExternalDistributionDto>>(StatusCodes.Status200OK);
    }
}

public sealed record GetExternalDistributionsRequestDto
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
    public Guid? PostMarketingId { get; init; }

    // Platform
    public ExternalPlatform? Platform { get; init; }
    public string? ExternalUrl { get; init; }
    public string? ExternalPostId { get; init; }

    // Status
    public DistributionStatus? Status { get; init; }

    // Metadata
    public string? PlatformMetadata { get; init; }

    // Error
    public bool? HasError { get; init; }

    // Sent Date Range
    public DateTime? SentFrom { get; init; }
    public DateTime? SentTo { get; init; }
}
