using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Commands;
using Marketing.Api.Filters;
using Marketing.Domain.Enums;

namespace Marketing.Api.Features.Webhooks.N8n.DistributionFailed;

public class N8nDistributionFailedEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/webhooks/n8n/distribution-failed", async (
            N8nDistributionFailedRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new FailExternalDistributionCommand(
                PostId: request.post_id,
                Platform: request.platform,
                ErrorMessage: request.error_message
            );

            var result = await sender.Send(command, cancellationToken);
            return result.ToOk();
        })
        .AddEndpointFilter<ValidateN8nApiKeyAttribute>()
        .AllowAnonymous()
        .WithTags("Webhooks", "n8n")
        .WithName("N8nDistributionFailed")
        .WithSummary("Callback from n8n when distribution to platform failed");
    }
}



public sealed class N8nDistributionFailedRequestDto
{
    public Guid post_id { get; init; }
    public ExternalPlatform platform { get; init; } 
    public string error_message { get; init; } = string.Empty;
    public bool can_retry { get; init; } = true;  // n8n can indicate if retry is possible
}