using AI.Application.Features.AiPackages.Commands.ToggleAiPackageStatus;
using AI.Application.Features.AiPackages.Dtos;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace AI.Api.Endpoints.AiPackages.Patch;

public class ToggleAiPackageStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/ai/packages/{id:guid}/toggle-status", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ToggleAiPackageStatusCommand(id), cancellationToken);
            return result.ToOk();
        })
        .WithTags("AI - Packages")
        .WithName("ToggleAiPackageStatus")
        .WithSummary("Toggle AI package active status")
        .Produces<ApiResult<AiPackageDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(UserRoles.Admin);
    }
}
