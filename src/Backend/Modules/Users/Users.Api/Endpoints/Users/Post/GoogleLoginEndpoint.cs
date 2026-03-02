//using Carter;
//using MediatR;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Routing;
//using Shared.Api.Results;
//using Users.Application.Features.Users.Commands.Records;
//using Users.Application.Features.Users.Dtos;
//using Users.Application.Abstractions.Authentication;

//namespace Users.Api.Users;

//public class GoogleLoginEndpoint : ICarterModule
//{
//    public void AddRoutes(IEndpointRouteBuilder app)
//    {
//        app.MapPost("api/users/google-login", async (
//            [FromBody] GoogleLoginRequestDto request,
//            ISender sender,
//            IGooglePayloadValidator validator,
//            CancellationToken cancellationToken) =>
//        {
//            var payloadResult = await validator.ValidateAsync(request.IdToken);
//            if (payloadResult.IsFailure)
//                return payloadResult.ToProblem();

//            var payload = payloadResult.Value;

//            var command = new GoogleLoginCommand(
//                ProviderKey: payload.Subject,
//                Email: payload.Email,
//                UserName: payload.Email?.Split('@')[0],
//                FirstName: payload.GivenName,
//                LastName: payload.FamilyName
//            );

//            var result = await sender.Send(command, cancellationToken);

//            return result.ToOk();
//        })
//        .WithTags("Users")
//        .WithName("GoogleLogin")
//        .WithSummary("Login with Google")
//        .WithDescription("Authenticates a user via Google ID token and returns access/refresh tokens along with user info")
//        .Produces<LoginResponseDto>(StatusCodes.Status200OK)
//        .ProducesProblem(StatusCodes.Status400BadRequest)
//        .ProducesProblem(StatusCodes.Status401Unauthorized)
//        .ProducesProblem(StatusCodes.Status404NotFound);
//    }
//}