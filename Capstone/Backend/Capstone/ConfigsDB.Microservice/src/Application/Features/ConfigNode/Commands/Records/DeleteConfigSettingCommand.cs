using Shared.Application.Abstractions.Messaging;

namespace ConfigsDB.Application.Features.ConfigSettings.Commands
{
    /// <summary>
    /// Command to soft delete a configuration setting.
    /// </summary>
    public record DeleteConfigSettingCommand(Guid Id) 
        : ICommand<bool>, ITransactionalCommand;

    /// <summary>
    /// Command to delete multiple configuration settings by their IDs.
    /// </summary>
    public record BulkDeleteConfigSettingsCommand(List<Guid> Ids) 
        : ICommand<int>, ITransactionalCommand;

}