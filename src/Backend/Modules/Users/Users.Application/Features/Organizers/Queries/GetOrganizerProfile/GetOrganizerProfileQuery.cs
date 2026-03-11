using Shared.Application.Abstractions.Messaging;

namespace Users.Application.Features.Organizers.Queries.GetOrganizerProfile;

public sealed record OrganizerProfileResponse(
    Guid Id,
    Guid UserId,
    string? Logo,
    string? DisplayName,
    string? Description,
    string? Address,
    string? SocialLink,
    string BusinessType,
    string? TaxCode,
    string? IdentityNumber,
    string? CompanyName,
    string? AccountName,
    string? AccountNumber,
    string? BankCode,
    string? Branch,
    string Status,
    string Type,
    DateTimeOffset? VerifiedAt);

public sealed record GetOrganizerProfileQuery : IQuery<OrganizerProfileResponse>;