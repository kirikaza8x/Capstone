using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;

namespace AI.Api.ChatBot;

public class ChatEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bot/chat", async (
        ChatCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        {
            Result<string> result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Bot")
        .WithName("Chat")
        .WithSummary("Send a prompt to Gemini")
        .WithDescription("Generates a response from Gemini based on the user prompt")
        .Produces<string>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
