using Carter;
using Events.Application.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Categories;

public class GetCategoriesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Categories, async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCategoriesQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("GetCategories")
        .WithSummary("Get all categories")
        .WithDescription("Retrieve all event categories.")
        .Produces<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK);
    }
}