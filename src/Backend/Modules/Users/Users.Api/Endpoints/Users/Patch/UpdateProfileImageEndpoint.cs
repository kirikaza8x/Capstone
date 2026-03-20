using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;

namespace Users.Api.Users.Patch;

public class UpdateProfileImageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/users/{userId}/profile-image", async (
            Guid userId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProfileImageCommand(new FormFileUpload(file), userId);

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.ToCreated($"/api/users/{userId}/profile-image", "Profile image updated successfully.");
        })
        .WithTags("Users")
        .WithName("UpdateProfileImage")
        .WithSummary("Update user profile image")
        .WithDescription("Uploads and updates the profile image for the specified user")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<UserProfileDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
