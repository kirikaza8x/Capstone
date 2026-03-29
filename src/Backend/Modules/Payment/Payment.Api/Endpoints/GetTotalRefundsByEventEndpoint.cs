using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payments.Application.Features.Vnpay.DTOs;
using Shared.Api.Results;
using Shared.Domain.Abstractions;

namespace Payments.Api.Reports;

public class GetRevenueByEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/event", async (
            GetRevenueByEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            Result<EventRevenueDto> result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetRevenueByEvent")
        .WithSummary("Gross revenue for a single event")
        .WithDescription("Returns the total charged amount across all completed transactions for the given event. Does not subtract refunds.")
        .Produces<EventRevenueDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetRevenuePerEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/reports/revenue/events", async (
            GetRevenuePerEventQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetRevenuePerEvent")
        .WithSummary("Gross revenue grouped by event")
        .WithDescription("Returns the total charged amount per event across all events with at least one completed transaction. Does not subtract refunds.")
        .Produces<List<EventRevenueDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

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
        .WithName("GetNetRevenueByEvent")
        .WithSummary("Net revenue for a single event")
        .WithDescription("Returns gross revenue minus the sum of all refunded ticket amounts for the given event. Returns 0 if no refunds exist.")
        .Produces<EventRevenueDto>(StatusCodes.Status200OK)
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
        .WithSummary("Net revenue grouped by event")
        .WithDescription("Returns gross minus refunds per event across all events with at least one completed transaction.")
        .Produces<List<EventRevenueDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

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
        .WithSummary("Total refunded amount for a single event")
        .WithDescription("Returns the sum of all refunded BatchPaymentItem amounts for the given event. Returns 0 if no refunds have been issued.")
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
        .WithSummary("Refund rate for a single event")
        .WithDescription("Returns gross revenue, total refunds, and the refund percentage (refunds / gross × 100) for the given event.")
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
        .WithSummary("Transaction counts and payment type breakdown for a single event")
        .WithDescription("Returns total, completed, failed and refunded transaction counts, plus revenue split between BatchWalletPay and BatchDirectPay for the given event.")
        .Produces<EventTransactionSummaryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}

public class GetGlobalRevenueSummaryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/revenue/global", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetGlobalRevenueSummaryQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags("Reports")
        .WithName("GetGlobalRevenueSummary")
        .WithSummary("Platform-wide revenue totals")
        .WithDescription("Returns total gross revenue, total refunds, net revenue, and the number of distinct events with completed transactions across the entire platform.")
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
        .WithSummary("Top N events ranked by revenue")
        .WithDescription("Returns the top N events ordered by gross revenue by default. Set ByNet to true to rank by net revenue (gross minus refunds) instead.")
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
        .WithSummary("Revenue summary for a single organizer")
        .WithDescription("Resolves all event IDs belonging to the organizer via the Events module, then returns their combined gross revenue, total refunds, net revenue, and event count.")
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
        .WithSummary("Per-event revenue breakdown for a single organizer")
        .WithDescription("Resolves all event IDs belonging to the organizer via the Events module, then returns revenue per event. Pass byNet=true for net revenue (gross minus refunds), omit or pass false for gross.")
        .Produces<OrganizerRevenuePerEventDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}