using Shared.Application.Abstractions.Messaging;
using Users.Domain.Enums;

namespace Users.Application.Features.Organizers.Commands;

public record UpdateOrganizerProfileCommand(
    string? Logo,
    string? DisplayName,
    string? Description,
    string? Address,
    string? SocialLink,
    BusinessType? BusinessType,
    string? TaxCode,
    string? IdentityNumber,
    string? CompanyName
) : ICommand;

public record UpdateOrganizerBankCommand(
    string? AccountName,
    string? AccountNumber,
    string? BankCode,
    string? Branch
) : ICommand;