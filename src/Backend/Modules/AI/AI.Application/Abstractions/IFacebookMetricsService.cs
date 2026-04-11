// Marketing.Application/Services/IFacebookMetricsService.cs

using Marketing.Application.Posts.Dtos;
using Marketing.Domain.Enums;

namespace Marketing.Application.Services;

public interface IFacebookMetricsService
{
    /// <summary>
    /// Fetches metrics for a specific Facebook post
    /// </summary>
    Task<FacebookMetricsDto?> GetMetricsAsync(
        string externalPostId, 
        string externalUrl, 
        CancellationToken ct = default);

    /// <summary>
    /// Fetches aggregate page-level metrics for a specified period
    /// </summary>
    Task<FacebookPageMetricsDto?> GetPageTotalsAsync(
        FacebookPeriod period = FacebookPeriod.days_28, 
        CancellationToken ct = default);

    // /// <summary>
    // /// Updates an existing Facebook post with new content
    // /// </summary>
    // Task<FacebookOperationResult?> UpdatePostAsync(
    //     string externalPostId,
    //     string title,
    //     string body,
    //     string? summary,
    //     string? imageUrl,
    //     CancellationToken ct = default);

    // /// <summary>
    // /// Deletes a Facebook post by external_post_id
    // /// </summary>
    // Task<FacebookOperationResult?> DeletePostAsync(
    //     string externalPostId,
    //     CancellationToken ct = default);

    // /// <summary>
    // /// Retrieves page access token for API operations
    // /// </summary>
    Task<string?> GetPageAccessTokenAsync(HttpClient client, CancellationToken ct);
}