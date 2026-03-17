namespace AI.Application.Abstractions.ReadModels;

/// <summary>
/// Read model of an event as seen by the AI module.
/// 
/// The AI module cannot reference Events.Domain directly — that would create
/// a circular or cross-module coupling. Instead, the Events module exposes
/// this thin query contract and provides the implementation.
///
/// Implemented by: Events.Infrastructure.AI.EventReadModelForAI (or similar)
/// Registered in DI by the Events module's service registration.
/// </summary>
public interface IEventReadModelService
{
    Task<EventAiReadModel?> GetByIdAsync(Guid eventId, CancellationToken ct = default);
}

/// <summary>
/// Flat projection of an Event — only the fields the AI module needs.
/// No navigation properties, no domain logic.
/// </summary>
public record EventAiReadModel(
    Guid         EventId,
    string       Title,
    string?      Description,
    string?      BannerUrl,
    List<string> CategoryNames,
    List<string> HashtagNames,
    DateTime?    EventStartAt,
    decimal?     MinPrice
);