using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Users.Commands.Records;
namespace Users.Api.Users;

public class ResetPasswordEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/reset-password", async (
            ResetPasswordRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ResetPasswordCommand(
                request.Email,
                request.OtpCode,
                request.NewPassword);

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk("Password has been reset successfully.");
        })
        .WithTags("Authentication")
        .WithName("ResetPassword")
        .WithSummary("Reset password using OTP")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

// Request DTO for the final step
public record ResetPasswordRequest(string Email, string OtpCode, string NewPassword);
