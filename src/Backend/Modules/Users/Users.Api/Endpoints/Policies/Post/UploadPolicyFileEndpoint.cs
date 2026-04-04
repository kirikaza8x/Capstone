using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Application.Abstractions.Storage;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace Users.Api.Endpoints.Policies.Post;

public sealed record UploadPolicyFileResponse(string Url);

public class UploadPolicyFileEndpoint : ICarterModule
{
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    ];

    private const long MaxFileSize = 20 * 1024 * 1024; // 20MB
    private const string PolicyFolder = "users/policies";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/policies/upload", async (
            IFormFile file,
            IStorageService storageService,
            CancellationToken cancellationToken) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "File is required." });

            if (file.Length > MaxFileSize)
                return Results.BadRequest(new { error = "File size exceeds 20MB limit." });

            var contentType = file.ContentType.ToLowerInvariant();
            if (!AllowedContentTypes.Contains(contentType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid file type. Allowed: PDF, DOC, DOCX, TXT."
                });
            }

            await using var stream = file.OpenReadStream();
            var url = await storageService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                PolicyFolder,
                cancellationToken);

            return Results.Ok(new UploadPolicyFileResponse(url));
        })
        .WithTags("Policies")
        .WithName("UploadPolicyFile")
        .WithSummary("Upload policy file")
        .WithDescription("Upload a policy attachment file and receive a public URL.")
        .DisableAntiforgery()
        .Produces<UploadPolicyFileResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireRoles(UserRoles.Admin);
    }
}
