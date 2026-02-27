using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Users.Commands.Records;

namespace Users.Api.Users.Post;

public class RequestPasswordResetEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/forgot-password", async (
            RequestPasswordResetRequestDto request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RequestPasswordResetCommand(request.Email);

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk("If an account exists, an OTP has been sent.");
        })
        .WithTags("Authentication")
        .WithName("RequestPasswordReset")
        .WithSummary("Initiate forgot password flow")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public record RequestPasswordResetRequestDto(string Email);