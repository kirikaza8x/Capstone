using AI.Application.Features.AiPackages.Commands.DeleteAiPackage;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace AI.Api.Endpoints.AiPackages.Delete;

public class DeleteAiPackageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/ai/packages/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteAiPackageCommand(id), cancellationToken);
            return result.ToNoContent();
        })
        .WithTags("AI - Packages")
        .WithName("DeleteAiPackage")
        .WithSummary("Delete AI package")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(UserRoles.Admin);
    }
}
