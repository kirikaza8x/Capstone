using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Shared.Domain.Pagination;
using Ticketing.Application.Vouchers.Queries.Dto;
using Ticketing.Application.Vouchers.Queries.GetVouchers;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Vouchers;


public class GetVouchersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.Vouchers, async (
            [AsParameters] GetVouchersQuery query,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(query, cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Vouchers)
        .WithName("GetVouchers")
        .WithSummary("Get vouchers")
        .WithDescription("Retrieve paginated vouchers.")
        .Produces<ApiResult<PagedResult<VoucherDto>>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
