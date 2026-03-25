using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Ticketing.Application.Orders.Queries.GetAllOrders;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Get;

public class GetAllOrdersForEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.OrganizerOrdersForEvent, async (
            [AsParameters] GetAllOrdersQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("GetAllOrdersForEvent")
        .WithSummary("Get all orders for an event (organizer only)")
        .WithDescription("Retrieve paginated orders for a specific event, only accessible by the event organizer.")
        .Produces<ApiResult<PagedResult<OrderListItemResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireRoles(Roles.Organizer);
    }
}
