using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.Application.Features.Policies.Commands.CreatePolicy;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace Users.Api.Endpoints.Policies.Post;

public sealed record CreatePolicyRequest(string Type, string? FileUrl, string Description);

public class CreatePolicyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("api/policies", async (
            [FromBody] CreatePolicyRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new CreatePolicyCommand(request.Type, request.FileUrl, request.Description),
                cancellationToken);

            if (result.IsFailure)
            {
                return result.ToProblem();
            }

            return result.ToCreated($"/api/policies/{result.Value.Id}");
        })
        .WithTags("Policies")
        .WithName("CreatePolicy")
        .WithSummary("Create policy")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .RequireRoles(UserRoles.Admin);
    }
}
