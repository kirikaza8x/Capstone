using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Features.Commands.RefundByEvent;

namespace Payments.Api.Features;

public class RefundByEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapPost("api/payments/refund/event/{eventId:guid}", async (
            Guid eventId,
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RefundByEventCommand(
                UserId: userId,
                EventId: eventId
            );

            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        })
        .RequireAuthorization()
        .WithTags("Payments")
        .WithName("RefundByEvent")
        .WithSummary("Refund a completed event payment to wallet balance")
        .Produces<RefundByEventResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

