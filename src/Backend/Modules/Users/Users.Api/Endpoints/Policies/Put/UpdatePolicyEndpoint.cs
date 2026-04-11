using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.Application.Features.Policies.Commands.UpdatePolicy;
using Users.Application.Features.Policies.Dtos;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace Users.Api.Endpoints.Policies.Put;

public sealed record UpdatePolicyRequest(string Type, string? FileUrl, string Description);

public class UpdatePolicyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("api/policies/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePolicyRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdatePolicyCommand(id, request.Type, request.FileUrl, request.Description),
                cancellationToken);

            return result.ToOk();
        })
        .WithTags("Policies")
        .WithName("UpdatePolicy")
        .WithSummary("Update policy")
        .Produces<ApiResult<PolicyDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(UserRoles.Admin);
    }
}
