using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Payments.Application.Features.Vnpay.DTOs;

namespace Payments.Api.Reports;

public class GetTotalRefundsByEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/refunds", async (
            GetTotalRefundsByEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetTotalRefundsByEvent")
        .WithSummary("Get total refunds for an event")
        .WithDescription("Returns the total refunded amount for a given event")
        .Produces<decimal>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetRefundRateByEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/refund-rate", async (
            GetRefundRateByEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetRefundRateByEvent")
        .WithSummary("Get refund rate for an event")
        .WithDescription("Returns refund percentage of gross revenue for a given event")
        .Produces<EventRefundRateDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetTransactionSummaryByEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/transaction-summary", async (
            GetTransactionSummaryByEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetTransactionSummaryByEvent")
        .WithSummary("Get transaction summary for an event")
        .WithDescription("Returns transaction counts and wallet vs direct pay breakdown for a given event")
        .Produces<EventTransactionSummaryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetGlobalRevenueSummaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/global", async (
            GetGlobalRevenueSummaryQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetGlobalRevenueSummary")
        .WithSummary("Get platform-wide revenue summary")
        .WithDescription("Returns gross, net, refunds and event count across all events")
        .Produces<GlobalRevenueSummaryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetTopEventsByRevenueEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/top-events", async (
            GetTopEventsByRevenueQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetTopEventsByRevenue")
        .WithSummary("Get top events by revenue")
        .WithDescription("Returns the top N events ranked by gross or net revenue")
        .Produces<List<EventRevenueDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetNetRevenuePerEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/net/events", async (
            GetNetRevenuePerEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetNetRevenuePerEvent")
        .WithSummary("Get net revenue per event")
        .WithDescription("Returns net revenue grouped by event across all events")
        .Produces<List<EventRevenueDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetOrganizerRevenueSummaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/revenue/organizer/{organizerId:guid}/summary", async (
            Guid organizerId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetOrganizerRevenueSummaryQuery(organizerId), cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetOrganizerRevenueSummary")
        .WithSummary("Get revenue summary for an organizer")
        .WithDescription("Returns gross, net, refunds and event count for all events belonging to the organizer")
        .Produces<OrganizerRevenueSummaryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetOrganizerRevenuePerEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/revenue/organizer/{organizerId:guid}/events", async (
            Guid organizerId,
            bool byNet,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetOrganizerRevenuePerEventQuery(organizerId, byNet), cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetOrganizerRevenuePerEvent")
        .WithSummary("Get per-event revenue breakdown for an organizer")
        .WithDescription("Returns gross or net revenue per event for all events belonging to the organizer")
        .Produces<OrganizerRevenuePerEventDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}