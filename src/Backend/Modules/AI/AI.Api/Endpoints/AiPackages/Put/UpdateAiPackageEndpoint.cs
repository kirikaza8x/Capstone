using AI.Application.Features.AiPackages.Commands.UpdateAiPackage;
using AI.Application.Features.AiPackages.Dtos;
using AI.Domain.Enums;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace AI.Api.Endpoints.AiPackages.Put;

public sealed record UpdateAiPackageRequest(
    string Name,
    string? Description,
    AiPackageType Type,
    decimal Price,
    int TokenQuota,
    bool IsActive);

public class UpdateAiPackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/ai/packages/{id:guid}", async (
            Guid id,
            [FromBody] UpdateAiPackageRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateAiPackageCommand(
                    id,
                    request.Name,
                    request.Description,
                    request.Type,
                    request.Price,
                    request.TokenQuota,
                    request.IsActive),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags("AI - Packages")
        .WithName("UpdateAiPackage")
        .WithSummary("Update AI package")
        .Produces<ApiResult<AiPackageDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(UserRoles.Admin);
    }
}
