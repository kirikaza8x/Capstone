using Carter;
using Events.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Application.Abstractions.Storage;

namespace Events.Api.Events.Post;

public sealed record UploadEventBannerResponse(
    string Url,
    string FileName,
    long Size,
    string ContentType);

public class UploadEventBannerEndpoint : ICarterModule
{
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Events + "/banners/upload", async (
            IFormFile file,
            IStorageService storageService,
            CancellationToken cancellationToken) =>
        {
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

            // Upload to events/banners folder
            await using var stream = file.OpenReadStream();
            var url = await storageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                StorageFolders.EventBanners,
                cancellationToken);

            return Results.Ok(new UploadEventBannerResponse(url, file.FileName, file.Length, file.ContentType));
        })
        .WithTags(Constants.Tags.Events)
        .WithName("UploadEventBanner")
        .WithSummary("Upload event banner image")
        .WithDescription("Upload banner image and get URL.")
        .DisableAntiforgery()
        .Produces<UploadEventBannerResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}