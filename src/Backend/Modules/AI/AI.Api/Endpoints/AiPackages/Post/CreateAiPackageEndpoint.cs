using AI.Application.Features.AiPackages.Commands.CreateAiPackage;
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
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.AiPackages.Post;

public sealed record CreateAiPackageRequest(
    string Name,
    string? Description,
    AiPackageType Type,
    decimal Price,
    int TokenQuota);

public class CreateAiPackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/ai/packages", async (
            [FromBody] CreateAiPackageRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateAiPackageCommand(
                    request.Name,
                    request.Description,
                    request.Type,
                    request.Price,
                    request.TokenQuota),
                cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblem();
            }

            return result.ToCreated($"/api/ai/packages/{result.Value.Id}");
        })
        .WithTags("AI - Packages")
        .WithName("CreateAiPackage")
        .WithSummary("Create AI package")
        .Produces<ApiResult<AiPackageDto>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.Admin);
    }
}
