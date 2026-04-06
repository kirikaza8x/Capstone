using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

public record GetExternalDistributionsQuery(
    // Identity
    Guid? PostMarketingId = null,

    // Platform
    ExternalPlatform? Platform = null,
    string? ExternalUrl = null,
    string? ExternalPostId = null,

    // Status
    DistributionStatus? Status = null,

    // Metadata
    string? PlatformMetadata = null,

    // Error
    bool? HasError = null,

    // Sent
    DateTime? SentFrom = null,
    DateTime? SentTo = null

) : PagedQuery, IQuery<PagedResult<ExternalDistributionDto>>;
