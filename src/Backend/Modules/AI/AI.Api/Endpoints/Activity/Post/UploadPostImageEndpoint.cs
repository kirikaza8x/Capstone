using AI.Application.Features.Post.Commands.UpdatePostImage;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Storage;
using Shared.Domain.Abstractions;
using Users.PublicApi.Constants;

namespace AI.Api.Endpoints.Activity.Post;

public sealed record UploadPostImageResponse(string ImageUrl, string Folder);

public sealed class UploadPostImageRequest
{
    public IFormFile File { get; init; } = null!;
    public string? FolderName { get; init; }
}

public sealed class UploadPostImageEndpoint : ICarterModule
{
    private const long MaxFileSizeInBytes = 10 * 1024 * 1024;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/organizer/posts/{postId:guid}/image", async (
            Guid postId,
            [FromForm] UploadPostImageRequest request,
            ICurrentUserService currentUserService,
            IStorageService storageService,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.UserId;
            if (userId == Guid.Empty)
            {
                return Result.Failure<UploadPostImageResponse>(Error.Unauthorized(
                    "PostImage.Unauthorized",
                    "Current user is not authenticated.")).ToProblem();
            }

            if (request.File is null || request.File.Length == 0)
            {
                return Result.Failure<UploadPostImageResponse>(Error.Validation(
                    "PostImage.EmptyFile",
                    "Image file is required.")).ToProblem();
            }

            if (request.File.Length > MaxFileSizeInBytes)
            {
                return Result.Failure<UploadPostImageResponse>(Error.Validation(
                    "PostImage.FileTooLarge",
                    "Image size must be less than or equal to 10 MB.")).ToProblem();
            }

            if (string.IsNullOrWhiteSpace(request.File.ContentType) ||
                !request.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure<UploadPostImageResponse>(Error.Validation(
                    "PostImage.InvalidContentType",
                    "Only image files are allowed.")).ToProblem();
            }

            var normalizedFolderName = NormalizeFolderName(request.FolderName);
            var folder = string.IsNullOrWhiteSpace(normalizedFolderName)
                ? $"ai/posts/{userId:N}"
                : $"ai/posts/{userId:N}/{normalizedFolderName}";

            await using var stream = request.File.OpenReadStream();
            var imageUrl = await storageService.UploadAsync(
                stream,
                request.File.FileName,
                request.File.ContentType,
                folder,
                cancellationToken);

            var updateResult = await sender.Send(
                new UpdatePostImageCommand(postId, imageUrl),
                cancellationToken);

            if (updateResult.IsFailure)
            {
                try
                {
                    await storageService.DeleteAsync(imageUrl, cancellationToken);
                }
                catch
                {
                }

                return updateResult.ToProblem();
            }

            return Result.Success(new UploadPostImageResponse(imageUrl, folder)).ToOk();
        })
        .DisableAntiforgery()
        .Accepts<UploadPostImageRequest>("multipart/form-data")
        .WithTags("Posts")
        .WithName("UploadPostImage")
        .WithSummary("Upload and attach image to a post")
        .Produces<ApiResult<UploadPostImageResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireRoles(Roles.Organizer);
    }

    private static string NormalizeFolderName(string? folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            return string.Empty;

        var trimmed = folderName.Trim().Trim('/');

        var parts = trimmed
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => new string(part
                .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
                .ToArray()))
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join('/', parts);
    }
}
