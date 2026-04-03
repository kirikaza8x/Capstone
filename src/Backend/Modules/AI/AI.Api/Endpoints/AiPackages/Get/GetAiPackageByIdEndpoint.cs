using AI.Application.Features.AiPackages.Dtos;
using AI.Application.Features.AiPackages.Queries.GetAiPackageById;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.AiPackages.Get;

public class GetAiPackageByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ai/packages/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetAiPackageByIdQuery(id), cancellationToken);
            return result.ToOk();
        })
        .WithTags("AI - Packages")
        .WithName("GetAiPackageById")
        .WithSummary("Get AI package by id")
        .Produces<ApiResult<AiPackageDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
