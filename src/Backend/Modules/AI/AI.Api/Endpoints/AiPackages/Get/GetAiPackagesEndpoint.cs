using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetAiPackages;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.AiPackages.Get;

public class GetAiPackagesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ai/packages", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAiPackagesQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags("AI - Packages")
        .WithName("GetAiPackages")
        .WithSummary("Get AI packages")
        .Produces<ApiResult<IReadOnlyList<AiPackageDto>>>(StatusCodes.Status200OK)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
