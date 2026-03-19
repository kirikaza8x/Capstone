// AI.Api/ImageGeneration/ImageGenerationEndpoint.cs
using AI.Application.Features.ImageGeneration;
using AI.Application.Features.ImageGeneration.Commands;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;

namespace AI.Api.ImageGeneration;

public class ImageGenerationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/bot/image-generation", async (
            ImageGenerationRequestDto dto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new GenerateImageCommand(dto.Prompt);

            Result<IReadOnlyList<GenerateImageResponse>> result =
                await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Bot")
        .WithName("GenerateImage")
        .WithSummary("Generate images from a text prompt")
        .WithDescription("Generates images using OpenRouter text-to-image models based on the provided prompt")
        .Produces<IReadOnlyList<GenerateImageResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
