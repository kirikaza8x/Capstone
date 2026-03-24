using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

public record StartOrUpdateOrganizerProfileCommand(
    OrganizerType Type,
    OrganizerBusinessInfoDto BusinessInfo,
    OrganizerBankInfoDto BankInfo
) : ICommand<Guid>;