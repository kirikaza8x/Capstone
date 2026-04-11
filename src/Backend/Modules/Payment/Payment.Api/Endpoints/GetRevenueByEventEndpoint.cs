// using Carter;
// using MediatR;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Routing;
// using Payments.Application.Features.Vnpay.DTOs;
// using Shared.Api.Results;
// using Shared.Domain.Abstractions;

// namespace Payments.Api.Reports;

// public class GetRevenueByEventEndpoint : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         app.MapPost("api/reports/revenue/event", async (
//             GetRevenueByEventQuery query,
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             Result<EventRevenueDto> result =
//                 await sender.Send(query, cancellationToken);

//             return result.ToOk();
//         })
//         .WithTags("Reports")
//         .WithName("GetRevenueByEvent")
//         .WithSummary("Get revenue for a specific event")
//         .WithDescription("Returns total revenue of a given event")
//         .Produces<EventRevenueDto>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status404NotFound)
//         .ProducesProblem(StatusCodes.Status500InternalServerError);
//     }
// }