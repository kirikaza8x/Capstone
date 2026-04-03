using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Policies.Dtos;
using Users.Application.Features.Policies.Queries;
using Users.Application.Features.Policies.Queries.GetPolicies;

namespace Users.Api.Endpoints.Policies.Get;

public class GetPoliciesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/policies", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetPoliciesQuery(), cancellationToken);
            return result.ToOk();
        })
        .WithTags("Policies")
        .WithName("GetPolicies")
        .WithSummary("Get all policies")
        .Produces<ApiResult<IReadOnlyList<PolicyDto>>>(StatusCodes.Status200OK);
    }
}
