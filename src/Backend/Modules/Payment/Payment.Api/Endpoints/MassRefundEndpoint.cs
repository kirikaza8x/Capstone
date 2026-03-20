using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Application.Features.VnPay.Dtos;
using Payments.Application.Features.Commands.MassRefundByEvent;

namespace Payments.Api.Features;

public class PaymentActionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {



        // Mass refund — admin only
        app.MapPost("api/payments/refund/event/{eventId:guid}/mass", async (
            Guid eventId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new MassRefundByEventCommand(eventId), cancellationToken);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        })
        // .RequireAuthorization("Admin")        // restrict to admin policy
        .WithTags("Payments")
        .WithName("MassRefundByEvent")
        .WithSummary("Refund all attendees for a cancelled event")
        .Produces<MassRefundResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

