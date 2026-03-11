using Carter;
using Events.Application.Categories.Commands.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Categories;

public sealed record UpdateCategoryRequest(string Code, string Name, string? Description);

public class UpdateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(Constants.Routes.CategoryById, async (
            [FromRoute] int categoryId,
            UpdateCategoryRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateCategoryCommand(categoryId, request.Code, request.Name, request.Description),
                cancellationToken);

            return result.ToOk("Category updated successfully.");
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("UpdateCategory")
        .WithSummary("Update category")
        .WithDescription("Update an existing category's name and description.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}