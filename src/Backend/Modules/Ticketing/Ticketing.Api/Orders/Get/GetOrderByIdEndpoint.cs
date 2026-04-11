using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Orders.Queries.GetOrderById;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Orders.Get;

public class GetOrderByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.OrderById, async (
            [FromRoute] Guid orderId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetOrderByIdQuery(orderId),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Orders)
        .WithName("GetOrderById")
        .WithSummary("Get order detail")
        .WithDescription("Retrieve order detail with all tickets and event information.")
        .Produces<ApiResult<IReadOnlyList<OrderTicketResponse>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AttendeeAndOrganizer);
    }
}
