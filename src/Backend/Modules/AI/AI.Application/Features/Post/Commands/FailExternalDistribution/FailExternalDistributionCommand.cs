using Shared.Application.Abstractions.Messaging;
using Marketing.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Commands;

public record FailExternalDistributionCommand(
    Guid PostId,
    ExternalPlatform Platform,
    string ErrorMessage
) : ICommand;