using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Abstractions.Storage;

namespace Events.Api.Events;

public sealed class UploadImageRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Folder { get; set; }
}

public sealed record UploadImageResponse(
    string Url,
    string FileName,
    long Size,
    string ContentType);

public class UploadImageEndpoint : ICarterModule
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/images/upload", async (
            [FromForm] UploadImageRequest request,
            IStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            var file = request.File;
            var folder = request.Folder ?? "images";

            // Validate file
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "File is required" });
            }

            if (file.Length > MaxFileSize)
            {
                return Results.BadRequest(new { error = "File size exceeds 10MB limit" });
            }

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP" });
            }

            // Upload
            await using var stream = file.OpenReadStream();
            var url = await storageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                folder,
                cancellationToken);

            return Results.Ok(new UploadImageResponse(url, file.FileName, file.Length, file.ContentType));
        })
        .WithTags("Images")
        .WithName("UploadImage")
        .WithSummary("Upload an image to storage")
        .WithDescription("Upload image and get URL. Use 'folder' form field to organize: events, avatars, etc.")
        .DisableAntiforgery()
        .Produces<UploadImageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}