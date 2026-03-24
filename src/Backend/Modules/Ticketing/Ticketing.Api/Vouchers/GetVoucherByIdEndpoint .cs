using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Vouchers.Queries.Dto;
using Ticketing.Application.Vouchers.Queries.GetVoucherById;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Vouchers;

public class GetVoucherByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(Constants.Routes.VoucherById, async (
            [FromRoute] Guid voucherId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetVoucherByIdQuery(voucherId),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToOk();
        })
        .WithTags(Constants.Tags.Vouchers)
        .WithName("GetVoucherById")
        .WithSummary("Get voucher detail")
        .WithDescription("Retrieve voucher detail by ID.")
        .Produces<ApiResult<VoucherDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
