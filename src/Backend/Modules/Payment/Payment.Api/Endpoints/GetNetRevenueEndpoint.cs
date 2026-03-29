using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Payments.Application.Features.Vnpay.DTOs;

namespace Payments.Api.Reports;

public class GetNetRevenueEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/net", async (
            GetNetRevenueByEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetNetRevenue")
        .WithSummary("Get net revenue")
        .WithDescription("Returns revenue after platform fees are deducted")
        .Produces<EventRevenueDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}