using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

public record GetAdminPostsQuery(
    // Identity
    Guid? EventId = null,
    Guid? OrganizerId = null,
    // Content
    string? Search = null, // Search in Title + Summary
    // AI Metadata
    string? AiModel = null,
    int? MinAiTokensUsed = null,
    int? MaxAiTokensUsed = null,
    decimal? MinAiCost = null,
    decimal? MaxAiCost = null,

    // Status
    PostStatus? Status = null,

    // Moderation
    Guid? ReviewedBy = null,
    bool? IsRejected = null,                 
    DateTime? ReviewedFrom = null,
    DateTime? ReviewedTo = null,

    // Publishing
    DateTime? SubmittedFrom = null,
    DateTime? SubmittedTo = null,
    DateTime? PublishedFrom = null,
    DateTime? PublishedTo = null,

    // External
    bool? HasExternalPostUrl = null

) : PagedQuery, IQuery<PagedResult<PostDto>>;