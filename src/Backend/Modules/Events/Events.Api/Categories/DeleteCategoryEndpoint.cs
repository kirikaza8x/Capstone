using Carter;
using Events.Application.Categories.Commands.DeleteCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Categories;

public class DeleteCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(Constants.Routes.CategoryById, async (
            [FromRoute] int categoryId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
            return result.ToOk("Category deleted successfully.");
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("DeleteCategory")
        .WithSummary("Delete category")
        .WithDescription("Permanently delete a category.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
