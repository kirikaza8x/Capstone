// using MediatR;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Routing;
// using Shared.Api.Results;
// using Shared.Domain.Abstractions;
// using Shared.Application.DTOs;
// using Users.Application.Features.Users.Queries;
// using Carter;

// namespace Users.Api.Users;

// public class UpdateUserEndpoint : ICarterModule
// {
//     public void AddRoutes(IEndpointRouteBuilder app)
//     {
//         app.MapPut("api/users/update", async (
//             HttpContext httpContext,
//             ISender sender,
//             CancellationToken cancellationToken) =>
//         {
//             var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
//             var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
//             var deviceId = httpContext.Request.Headers["X-Device-ID"].ToString();

//             Result<DeviceInfo> result = await sender.Send(
//                 new GetCurrentDeviceInfoQuery(userAgent, ipAddress, deviceId),
//                 cancellationToken);

//             return result.ToOk();
//         })
//         .WithTags("Users")
//         .WithName("GetCurrentDeviceInfo")
//         .WithSummary("Get current device info")
//         .WithDescription("Returns information about the device making the current request")
//         .Produces<DeviceInfo>(StatusCodes.Status200OK)
//         .ProducesProblem(StatusCodes.Status404NotFound);
//     }
// }
