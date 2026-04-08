using Carter;
using Marketing.Api.Filters;
using Marketing.Application.Posts.Commands;
using Marketing.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Shared.Api.RateLimiting;
using Shared.Api.Results;

namespace Marketing.Api.Features.Webhooks.N8n.DistributionSuccess;

public class N8nDistributionSuccessEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/webhooks/n8n/distribution-success", async (
            N8nDistributionSuccessRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmExternalDistributionCommand(
                PostId: request.post_id,
                Platform: request.platform,
                ExternalUrl: request.external_url,
                ExternalPostId: request.external_post_id,
                PlatformMetadata: request.platform_metadata
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .RequireRateLimiting(RateLimitPolicies.Webhook)
        .AddEndpointFilter<ValidateN8nApiKeyAttribute>()  // ← Validate API key
        .AllowAnonymous()  // ← Webhooks don't have user auth
        .WithTags("Webhooks", "n8n")
        .WithName("N8nDistributionSuccess")
        .WithSummary("Callback from n8n when post successfully distributed to platform");
    }
}

public sealed class N8nDistributionSuccessRequestDto
{
    public Guid post_id { get; init; }
    public ExternalPlatform platform { get; init; }  // "Facebook", "LinkedIn", etc.
    public string external_url { get; init; } = string.Empty;  // https://facebook.com/...
    public string? external_post_id { get; init; }  // Platform's internal ID
    public string? platform_metadata { get; init; }  // JSON string with extra data
}
