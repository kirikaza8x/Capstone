using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Marketing.Application.Posts.Dtos;
using Carter;

namespace Marketing.Api.ExternalDistributions;

public class GetExternalDistributionByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/external-distributions/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetExternalDistributionByIdQuery(id);
            var result = await sender.Send(query, cancellationToken);
            return result.ToOk();
        })
        .WithName("GetExternalDistributionById")
        .WithTags("ExternalDistributions")
        .WithDescription("Get a single external distribution by ID")
        .Produces<ExternalDistributionDetailDto>(StatusCodes.Status200OK);
    }
}
