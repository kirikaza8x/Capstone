using Carter;
using Events.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Abstractions.Storage;

namespace Events.Api.Events.Post;

public sealed record UploadImageResponse(string Url);

public class UploadImageEndpoint : ICarterModule
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private static readonly string[] AllowedFolders =
    [
        StorageFolders.EventBanners,
        StorageFolders.EventImages,
        StorageFolders.EventActors,
        StorageFolders.EventSeatmaps
    ];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Events + "/upload", async (
            IFormFile file,
            string? folder,
            IStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "File is required." });

            if (file.Length > MaxFileSize)
                return Results.BadRequest(new { error = "File size exceeds 10MB limit." });

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return Results.BadRequest(new { error = "Invalid file type. Allowed: JPEG, PNG, GIF, WebP." });

            var targetFolder = AllowedFolders.Contains(folder)
                ? folder
                : StorageFolders.Events;

            await using var stream = file.OpenReadStream();
            var url = await storageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                targetFolder,
                cancellationToken);

            return Results.Ok(new UploadImageResponse(url));
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UploadImage")
        .WithSummary("Upload image")
        .WithDescription("Upload an image file and receive a URL. Optionally specify a folder: events/banners, events/images, events/actors, events/seatmaps.")
        .DisableAntiforgery()
        .Produces<UploadImageResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
