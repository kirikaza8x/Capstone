using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.DTOs.Payment;
using Payments.Application.Features.Payments.Queries.GetTransactionById;
using Shared.Api.Results;

namespace Payments.Api.Features.Payments.GetTransactionById;

public class GetTransactionByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetTransactionByIdQuery(id), cancellationToken);


            return result.ToOk();
        })
        .WithName("GetTransactionById")
        .WithTags("Payments")
        .Produces<PaymentTransactionDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithSummary("Fetch a single payment transaction by ID, including child items");
    }
}
