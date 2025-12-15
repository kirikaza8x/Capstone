

/// <summary>
/// INTEGRATION EVENT - Published to OTHER services
/// Raised when: User is successfully created in UserService
/// Consumers: EmailService, AnalyticsService, ProfileService
/// 
/// NOTE: This is DIFFERENT from Users.Domain.Events.UserCreatedEvent
/// This one contains aggregated data (like roles) after domain logic completes
/// </summary>

namespace Shared.Contracts.Events.Users;

public record UserIntegrationCreatedEvent : IntegrationEventBase
{
    public override string EventType => nameof(UserIntegrationCreatedEvent);

    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();  
    public DateTime CreatedAt { get; init; }
}