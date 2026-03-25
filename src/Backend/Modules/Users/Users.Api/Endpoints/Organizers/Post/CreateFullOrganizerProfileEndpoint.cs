using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Api.Results;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

namespace Users.Api.Organizers;

public class StartOrUpdateOrganizerProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/organizer/profile/start-or-update", async (
    StartOrUpdateOrganizerProfileRequestDto request,
    ISender sender,
    CancellationToken cancellationToken) =>
        {
            var businessInfo = new OrganizerBusinessInfoDto(
                request.DisplayName,
                request.Description,
                request.Address,
                request.SocialLink,
                request.BusinessType,
                request.TaxCode,
                request.IdentityNumber,
                request.CompanyName
            );

            var bankInfo = new OrganizerBankInfoDto(
                request.AccountName,
                request.AccountNumber,
                request.BankCode,
                request.Branch
            );

            var command = new StartOrUpdateOrganizerProfileCommand(
                request.Type,
                businessInfo,
                bankInfo
            );

            var result = await sender.Send(command, cancellationToken);

            return result.ToOk();
        })
        .WithName("StartOrUpdateOrganizerProfile")
        .WithTags("Organizers")
        .WithSummary("Start or update organizer profile draft")
        .WithDescription("Creates a new organizer profile draft if none exists. Updates an existing draft if present. Returns conflict if a pending profile exists.")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

            }
        }

public sealed class StartOrUpdateOrganizerProfileRequestDto
{
    public OrganizerType Type { get; init; }

    // Business info fields
    public string? Logo { get; init; }
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? SocialLink { get; init; }
    public BusinessType? BusinessType { get; init; }
    public string? TaxCode { get; init; }
    public string? IdentityNumber { get; init; }
    public string? CompanyName { get; init; }

    // Bank info fields
    public string? AccountName { get; init; }
    public string? AccountNumber { get; init; }
    public string? BankCode { get; init; }
    public string? Branch { get; init; }
}
