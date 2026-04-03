using Shared.Application.Abstractions.Messaging;
using Marketing.Domain.Enums;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Commands;

public record ConfirmExternalDistributionCommand(
    Guid PostId,
    ExternalPlatform Platform,
    string ExternalUrl,
    string? ExternalPostId = null,
    string? PlatformMetadata = null
) : ICommand;