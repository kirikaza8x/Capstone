using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.Application.Features.Policies.Commands.DeletePolicy;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace Users.Api.Endpoints.Policies.Delete;

public class DeletePolicyEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/policies/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new DeletePolicyCommand(id), cancellationToken);
            return result.ToNoContent();
        })
        .WithTags("Policies")
        .WithName("DeletePolicy")
        .WithSummary("Delete policy")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(UserRoles.Admin);
    }
}
