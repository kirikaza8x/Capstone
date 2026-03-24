using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

public record CreateFullOrganizerProfileCommand(
    OrganizerType Type,
    OrganizerBusinessInfoDto BusinessInfo,
    OrganizerBankInfoDto BankInfo
) : ICommand<Guid>;