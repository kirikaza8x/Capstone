using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Records;
using Users.Application.Features.Users.Dtos;

namespace Users.Api.Users;

public class UpdateProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/users/profile", async (
            [FromBody] UpdateProfileRequestDto requestDto,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateProfileCommand command = new UpdateProfileCommand(
                requestDto.UserId,
                requestDto.FirstName,
                requestDto.LastName,
                requestDto.Birthday,
                requestDto.Gender,
                requestDto.Phone,
                requestDto.Address,
                requestDto.Description,
                requestDto.SocialLink,
                requestDto.ProfileImageUrl
            );
            Result<UserProfileDto> result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithTags("Users")
        .WithName("UpdateUserProfile")
        .WithSummary("Update user profile")
        .WithDescription("Updates the profile information of an existing user")
        .Produces<UserProfileDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
