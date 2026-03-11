using Users.Domain.Enums;

namespace Users.Application.Features.Organizers.Dtos;

public sealed class OrganizerProfileDto
{
    public Guid UserId { get; init; }

    public string? Logo { get; init; }

    public string DisplayName { get; init; } = default!;

    public string? Description { get; init; }

    public string? Address { get; init; }

    public string? SocialLink { get; init; }

    public BusinessType? BusinessType { get; init; }

    public string? TaxCode { get; init; }

    public string? IdentityNumber { get; init; }

    public string? CompanyName { get; init; }

    public OrganizerStatus Status { get; init; }

    public string? RejectionReason { get; init; }

    public OrganizerBankDto? Bank { get; init; }
}

public sealed class OrganizerBankDto
{
    public string AccountName { get; init; } = default!;

    public string AccountNumber { get; init; } = default!;

    public string BankCode { get; init; } = default!;

    public string Branch { get; init; } = default!;
}

public sealed class OrganizerPublicProfileDto
{
    public Guid UserId { get; init; }

    public string? Logo { get; init; }

    public string DisplayName { get; init; } = default!;

    public string? Description { get; init; }

    public string? Address { get; init; }

    public string? SocialLink { get; init; }

    public BusinessType? BusinessType { get; init; }
}

public sealed class OrganizerAdminListItemDto
{
    public Guid UserId { get; init; }

    public Guid ProfileId { get; init; }

    public string DisplayName { get; init; } = default!;

    public OrganizerStatus Status { get; init; }

    public BusinessType? BusinessType { get; init; }

    public int VersionNumber { get; init; }

    public DateTime? CreatedAt { get; init; }
}