// using Carter;
// using MediatR;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Routing;
// using Payments.Application.DTOs.Wallet;
// using Shared.Api.Results;
// using Shared.Domain.Abstractions;
// using Payments.Application.Features.Vnpay.DTOs;

// namespace Payments.Api.Reports;

// public class GetRevenuePerEventEndpoint : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         app.MapPost("api/reports/revenue/events", async (
//             GetRevenuePerEventQuery query,
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             var result = await sender.Send(query, cancellationToken);

//             return result.ToOk();
//         })
//         .WithTags("Reports")
//         .WithName("GetRevenuePerEvent")
//         .WithSummary("Get revenue per event")
//         .WithDescription("Returns total revenue grouped by event")
//         .Produces<List<EventRevenueDto>>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status500InternalServerError);
//     }
// }