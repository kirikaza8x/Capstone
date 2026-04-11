using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetMyPurchasedAiPackages;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.AiPackages.Get;

public class GetMyPurchasedAiPackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ai/packages/me/purchased", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetMyPurchasedAiPackagesQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags("AI - Packages")
        .WithName("GetMyPurchasedAiPackages")
        .WithSummary("Get AI packages purchased by current organizer")
        .Produces<ApiResult<IReadOnlyList<MyPurchasedAiPackageDto>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireRoles(Roles.Organizer);
    }
}
