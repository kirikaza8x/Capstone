using AI.Application.Abstractions;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AI.Api.Features.Admin;

/// <summary>
/// Admin endpoints for manual Qdrant re-indexing.
///
/// POST /api/admin/events/reindex          → re-index all events
/// POST /api/admin/events/reindex/{eventId} → re-index single event
/// </summary>
public class ReIndexEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Re-index all events
        app.MapPost("api/admin/events/reindex", async (
            IEventReIndexService reIndexService,
            CancellationToken ct) =>
        {
            var count = await reIndexService.ReIndexAllAsync(ct);
            return Results.Ok(new
            {
                success = true,
                indexed = count,
                message = $"Successfully re-indexed {count} events into Qdrant."
            });
        })
        .WithTags("Admin")
        .WithName("ReIndexAllEvents")
        .WithSummary("Re-index all events into Qdrant")
        .WithDescription(
            "Fetches all active events from the Events module and upserts their " +
            "embeddings into Qdrant. Safe to run multiple times — existing vectors " +
            "are overwritten. Use this to recover from missed integration events.")
        .Produces<object>(StatusCodes.Status200OK);

        // Re-index single event
        app.MapPost("api/admin/events/reindex/{eventId:guid}", async (
            Guid eventId,
            IEventReIndexService reIndexService,
            CancellationToken ct) =>
        {
            await reIndexService.ReIndexOneAsync(eventId, ct);
            return Results.Ok(new
            {
                success = true,
                eventId,
                message = $"Event {eventId} re-indexed into Qdrant."
            });
        })
        .WithTags("Admin")
        .WithName("ReIndexOneEvent")
        .WithSummary("Re-index a single event into Qdrant")
        .Produces<object>(StatusCodes.Status200OK);
    }
}
