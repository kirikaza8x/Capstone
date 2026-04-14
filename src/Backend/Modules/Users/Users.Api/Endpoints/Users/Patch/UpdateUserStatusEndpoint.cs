using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.Application.Features.Users.Commands.Records;
using Users.Domain.Enums;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace Users.Api.Endpoints.Users.Patch;

public class UpdateUserStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/users/status", async (
            Guid userId,          
            UserStatus userStatus,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateStatusCommand(userId, userStatus),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags("Users")
        .WithName("UpdateUserStatus")
        .WithSummary("Update user status")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        // .RequireRoles(UserRoles.Admin)
        ;
    }
}
