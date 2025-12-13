using Shared.Application.Abstractions.Messaging;
using ConfigsDB.Application.Features.ConfigSettings.Dtos;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands
{
    /// <summary>
    /// Command to create a new configuration setting.
    /// </summary>
    public record CreateConfigSettingCommand(ConfigSettingRequestDto Request) 
        : ICommand<ConfigSettingResponseDto>, ITransactionalCommand;

    /// <summary>
    /// Command to create multiple configuration settings at once.
    /// </summary>
    public record BulkCreateConfigSettingsCommand(BulkConfigSettingRequestDto Request) 
        : ICommand<List<ConfigSettingResponseDto>>, ITransactionalCommand;
    
    /// <summary>
    /// Command to mark a configuration value as plain text.
    /// </summary>
    public record MarkConfigAsPlainTextCommand(Guid Id) 
        : ICommand<bool>, ITransactionalCommand;
}