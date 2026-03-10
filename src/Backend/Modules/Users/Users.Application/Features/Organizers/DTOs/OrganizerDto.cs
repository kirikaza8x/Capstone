using Users.Domain.Enums;

namespace Users.Application.Features.Organizers.Dtos
{
    public record CreateOrganizerProfileRequestDto(OrganizerType Type);

    public record UpdateOrganizerProfileRequestDto(
        string? Logo,
        string DisplayName,
        string? Description,
        string? Address,
        string? SocialLink,
        BusinessType? BusinessType,
        string? TaxCode,
        string? IdentityNumber,
        string? CompanyName
    );

    public record UpdateOrganizerBankRequestDto(
        string AccountName,
        string AccountNumber,
        string BankCode,
        string? Branch
    );
}
