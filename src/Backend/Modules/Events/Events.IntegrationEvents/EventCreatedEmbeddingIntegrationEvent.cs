// using Shared.Application.Abstractions.EventBus;

// namespace Events.IntegrationEvents
// {
//     /// <summary>
//     /// Published when a new event is created and needs an embedding generated.
//     /// AI module consumes this to generate + store the vector in Qdrant.
//     /// </summary>
//     public sealed record EventCreatedEmbeddingIntegrationEvent : IntegrationEvent
//     {
//         public Guid EventId { get; init; }
//         public Guid OrganizerId { get; init; }
//         public string Title { get; init; } = string.Empty;
//         public string Description { get; init; } = string.Empty;
//         public IReadOnlyList<string> Categories { get; init; } = [];
//         public IReadOnlyList<string> Hashtags { get; init; } = [];
//         public bool IsActive { get; init; }
//         public DateTime? CreatedAt { get; init; }

//         public EventCreatedEmbeddingIntegrationEvent(
//             Guid eventId,
//             Guid organizerId,
//             string title,
//             string description,
//             IReadOnlyList<string> categories,
//             IReadOnlyList<string> hashtags,
//             bool isActive,
//             DateTime? createdAt)
//             : base(Guid.NewGuid(), DateTime.UtcNow)
//         {
//             EventId = eventId;
//             OrganizerId = organizerId;
//             Title = title;
//             Description = description;
//             Categories = categories;
//             Hashtags = hashtags;
//             IsActive = isActive;
//             CreatedAt = createdAt;
//         }
//     }

    
    
// }
