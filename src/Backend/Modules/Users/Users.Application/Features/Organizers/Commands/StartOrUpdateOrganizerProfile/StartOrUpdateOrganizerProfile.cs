using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Enums;

public record StartOrUpdateOrganizerProfileCommand(
    OrganizerType Type,
    OrganizerBusinessInfoDto BusinessInfo,
    OrganizerBankInfoDto BankInfo,
    IFileUpload? LogoFile = null
) : ICommand<Guid>;