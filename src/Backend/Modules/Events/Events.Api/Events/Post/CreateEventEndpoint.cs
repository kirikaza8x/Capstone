using Carter;
using Events.Application.Events.Commands.CreateEvent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.PublicApi.Constants;

namespace Events.Api.Events.Post;

public sealed record CreateEventRequest(
    Guid OrganizerId,
    string Title,
    string? BannerUrl,
    List<int> HashtagIds,
    int EventCategoryId,
    string Location,
    string? MapUrl,
    string Description);

public class CreateEventEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Events, async (
            [FromBody] CreateEventRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreateEventCommand(
                    request.OrganizerId,
                    request.Title,
                    request.BannerUrl,
                    request.HashtagIds,
                    request.EventCategoryId,
                    request.Location,
                    request.MapUrl,
                    request.Description),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToCreated(
                $"{Constants.Routes.Events}/{result.Value}",
                "Event created successfully.");
        })
        .WithTags(Constants.Tags.TicketTypes)
        .WithName("CreateEvent")
        .WithSummary("Create a new event")
        .WithDescription("Creates a new event with basic information. The event will be created in Draft status.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.Organizer);
    }
}