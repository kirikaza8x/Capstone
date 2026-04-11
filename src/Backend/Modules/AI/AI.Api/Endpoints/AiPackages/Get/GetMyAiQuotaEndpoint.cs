using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetMyAiQuota;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.AiPackages.Get;

public class GetMyAiQuotaEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ai/quotas/me", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetMyAiQuotaQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags("AI - Quota")
        .WithName("GetMyAiQuota")
        .WithSummary("Get current organizer AI quota")
        .Produces<ApiResult<MyAiQuotaDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireRoles(Roles.Organizer);
    }
}
