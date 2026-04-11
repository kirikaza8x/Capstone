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
            string? name,
            int? take,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCategoriesQuery(name, take ?? 20), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("GetCategories")
        .WithSummary("Get all categories")
        .WithDescription("Retrieve categories. Supports search-as-you-type via `name` query param. Use `take` to limit results.")
        .Produces<ApiResult<IReadOnlyList<CategoryResponse>>>(StatusCodes.Status200OK);
    }
}
