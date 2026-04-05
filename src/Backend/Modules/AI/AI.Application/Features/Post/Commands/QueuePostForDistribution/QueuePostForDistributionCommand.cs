using Marketing.Domain.Enums;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Marketing.Application.Posts.Commands;

/// <summary>
/// Command to manually trigger distribution of a published post to an external platform.
/// Example: User clicks "Post to Facebook" in the UI.
/// </summary>
public record QueuePostForDistributionCommand(
    Guid PostId,
    ExternalPlatform Platform,
    bool IsRetry = true
) : ICommand;