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

public sealed record UpdateUserStatusRequest(Guid UserId, UserStatus UserStatus);

public class UpdateUserStatusEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("api/users/status", async (
            [FromBody] UpdateUserStatusRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateStatusCommand(request.UserId, request.UserStatus),
                cancellationToken);
            return result.ToOk(); 
        })
        .WithTags("Users")
        .WithName("UpdateUserStatus")
        .WithSummary("Update user status")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(UserRoles.Admin);
    }
}
