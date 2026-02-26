using Shared.Application.EventBus;

namespace Users.IntegrationEvents;

/// <summary>
/// Integration event published when an OTP is generated for password reset.
/// </summary>
public sealed class OtpIntegrationCreatedEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string OtpCode { get; init; } = default!;
    public DateTime CreatedAt { get; init; }

    public OtpIntegrationCreatedEvent(Guid userId, string otpCode, DateTime createdAt)
        : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        UserId = userId;
        OtpCode = otpCode;
        CreatedAt = createdAt;
    }
}


