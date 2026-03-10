using Carter;
using Events.Application.Categories.Commands.ToggleCategoryStatus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Categories;

public sealed record ToggleCategoryStatusRequest(bool Activate);

public class ToggleCategoryStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(Constants.Routes.CategoryById + "/status", async (
            [FromRoute] int categoryId,
            ToggleCategoryStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ToggleCategoryStatusCommand(categoryId, request.Activate),
                cancellationToken);

            return result.ToOk(request.Activate ? "Category activated." : "Category deactivated.");
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("ToggleCategoryStatus")
        .WithSummary("Toggle category status")
        .WithDescription("Activate or deactivate a category.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}