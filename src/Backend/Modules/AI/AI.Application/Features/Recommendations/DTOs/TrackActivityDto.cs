
namespace AI.Application.Features.Recommendations.DTOs;

public record TrackActivityRequestDto(
        Guid UserId,
        string ActionType,
        string TargetId,
        string TargetType,
        Dictionary<string, string>? Metadata
    );
