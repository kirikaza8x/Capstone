using Microsoft.Extensions.Logging;
using Shared.Application.EventBus;
using Shared.Application.Messaging;
using Users.Domain.Events;
using Users.IntegrationEvents;

namespace Users.Application.Features.Users.EventHandlers
{
    /// <summary>
    /// Handles OTP creation events.
    /// Publishes integration event to notify external services (e.g., email/SMS sender).
    /// </summary>
    public class OtpCreatedEventHandler : IDomainEventHandler<OtpCreatedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<OtpCreatedEventHandler> _logger;

        public OtpCreatedEventHandler(IEventBus eventBus, ILogger<OtpCreatedEventHandler> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task Handle(OtpCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling OTP creation for UserId: {UserId}", notification.UserId);

                var integrationEvent = new OtpIntegrationCreatedEvent(
                    notification.UserId,
                    notification.OtpCode,
                    DateTime.UtcNow
                );

                await _eventBus.PublishAsync(integrationEvent, cancellationToken);

                _logger.LogInformation("OTP integration event published for UserId: {UserId}", notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle OTP creation for {UserId}", notification.UserId);
                throw;
            }
        }
    }
}
