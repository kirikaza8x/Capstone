using Carter;
using Events.Application.Categories.Commands.CreateCategory;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Categories;

public sealed record CreateCategoryRequest(string Code, string Name, string? Description);

public class CreateCategoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Categories, async (
            CreateCategoryRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateCategoryCommand(request.Code, request.Name, request.Description),
                cancellationToken);

            return result.ToCreated("GetCategoryById", id => new { categoryId = id });
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("CreateCategory")
        .WithSummary("Create category")
        .WithDescription("Create a new event category.")
        .Produces<int>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AdminAndStaff);
    }
}
