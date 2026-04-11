using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Ticketing.Application.Orders.Queries.GetMyOrders;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Get;

public class GetMyOrdersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.MyOrders, async (
            [AsParameters] GetMyOrdersQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("GetMyOrders")
        .WithSummary("Get my orders")
        .WithDescription("Retrieve paginated order history of the current user.")
        .Produces<ApiResult<PagedResult<MyOrderResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireRoles(Roles.AttendeeAndOrganizer);
    }
}
