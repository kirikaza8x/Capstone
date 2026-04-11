using Shared.Application.Abstractions.Messaging;

namespace AI.Application.Features.Tracking.Commands
{
    public record TrackActivityCommand(
        Guid UserId,
        string ActionType, // "click", "view", "purchase"
        string TargetId,   // "product-123"
        string TargetType, // "product"
        IReadOnlyDictionary<string, string>? Metadata
    ) : ICommand<bool>;
}
