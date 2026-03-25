using Shared.Application.Abstractions.Messaging;

namespace Marketing.Application.Posts.Commands;

public record RecordExternalDistributionCommand(
    Guid PostId,
    string ExternalUrl
) : ICommand;