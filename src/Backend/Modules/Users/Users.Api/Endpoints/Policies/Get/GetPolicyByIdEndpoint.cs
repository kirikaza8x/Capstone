using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Users.Application.Features.Policies.Dtos;
using Users.Application.Features.Policies.Queries.GetPolicyById;
using UserRoles = Users.PublicApi.Constants.Roles;

namespace Users.Api.Endpoints.Policies.Get;

public class GetPolicyByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/policies/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetPolicyByIdQuery(id), cancellationToken);
            return result.ToOk();
        })
        .WithTags("Policies")
        .WithName("GetPolicyById")
        .WithSummary("Get policy by id")
        .Produces<ApiResult<PolicyDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(UserRoles.Attendee, UserRoles.Organizer, UserRoles.Admin);
    }
}
