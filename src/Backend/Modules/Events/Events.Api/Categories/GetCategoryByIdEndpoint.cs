using Carter;
using Events.Application.Categories.Queries.GetCategories;
using Events.Application.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;

namespace Events.Api.Categories;

public class GetCategoryByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.CategoryById, async (
            [FromRoute] int categoryId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetCategoryByIdQuery(categoryId), cancellationToken);
            return result.ToOk();
        })
        .WithTags(Constants.Tags.Categories)
        .WithName("GetCategoryById")
        .WithSummary("Get category by ID")
        .WithDescription("Retrieve a single category by its ID.")
        .Produces<ApiResult<CategoryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}