using Shared.Application.Abstractions.Messaging;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands
{
    /// <summary>
    /// Command to update an existing configuration setting's value and description.
    /// </summary>
    public record UpdateConfigSettingCommand(Guid Id, UpdateConfigSettingRequestDto Request) 
        : ICommand<ConfigSettingResponseDto>, ITransactionalCommand;

    /// <summary>
    /// Command to update configuration setting metadata (category/environment).
    /// </summary>
    public record UpdateConfigSettingMetadataCommand(Guid Id, ChangeConfigSettingMetadataRequestDto Request)
        : ICommand<ConfigSettingResponseDto>, ITransactionalCommand;
    
    /// <summary>
    /// Command to mark a configuration value as encrypted.
    /// </summary>
    public record MarkConfigAsEncryptedCommand(Guid Id) 
        : ICommand<bool>, ITransactionalCommand;

    /// <summary>
    /// Command to deactivate a configuration setting.
    /// </summary>
    public record DeactivateConfigSettingCommand(Guid Id) 
        : ICommand<bool>, ITransactionalCommand;

    /// <summary>
    /// Command to activate a configuration setting.
    /// </summary>
    public record ActivateConfigSettingCommand(Guid Id) 
        : ICommand<bool>, ITransactionalCommand;
}