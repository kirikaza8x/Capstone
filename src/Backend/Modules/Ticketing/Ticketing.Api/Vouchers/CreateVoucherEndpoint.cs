using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Extensions;
using Shared.Api.Results;
using Ticketing.Application.Vouchers.Commands.CreateVoucher;
using Ticketing.Domain.Enums;
using Users.PublicApi.Constants;

namespace Ticketing.Api.Vouchers;

public sealed record CreateVoucherRequest(
    string CouponCode,
    VoucherType Type,
    decimal Value,
    int MaxUse,
    DateTime StartDate,
    DateTime EndDate,
    Guid? EventId);

public class CreateVoucherEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(Constants.Routes.Vouchers, async (
            [FromBody] CreateVoucherRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new CreateVoucherCommand(
                request.CouponCode,
                request.Type,
                request.Value,
                request.MaxUse,
                request.StartDate,
                request.EndDate,
                request.EventId),
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblem();

            return result.ToCreated(
                $"{Constants.Routes.Vouchers}/{result.Value}",
                "Voucher created successfully.");
        })
        .WithTags(Constants.Tags.Vouchers)
        .WithName("CreateVoucher")
        .WithSummary("Create voucher")
        .WithDescription("Admin creates global voucher (EventId = null). Organizer must specify EventId.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .RequireRoles(Roles.AdminAndOrganizer);
    }
}
