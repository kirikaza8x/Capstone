using Microsoft.Extensions.Logging;
using Shared.Application.EventBus;
using Shared.Application.Messaging;
using Users.Domain.Events;
using Users.IntegrationEvents;

namespace Users.Application.Features.Users.EventHandlers
{
    /// <summary>
    /// Handles password change events.
    /// Publishes integration event to notify other services (e.g., audit, security monitoring).
    /// </summary>
    public class PasswordChangedEventHandler : IDomainEventHandler<PasswordChangedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<PasswordChangedEventHandler> _logger;

        public PasswordChangedEventHandler(IEventBus eventBus, ILogger<PasswordChangedEventHandler> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task Handle(PasswordChangedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling password change for UserId: {UserId}", notification.UserId);

                var integrationEvent = new PasswordIntegrationChangedEvent(
                    notification.UserId,
                    DateTime.UtcNow
                );

                await _eventBus.PublishAsync(integrationEvent, cancellationToken);

                _logger.LogInformation("Password change integration event published for UserId: {UserId}", notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle password change for {UserId}", notification.UserId);
                throw;
            }
        }
    }
}
