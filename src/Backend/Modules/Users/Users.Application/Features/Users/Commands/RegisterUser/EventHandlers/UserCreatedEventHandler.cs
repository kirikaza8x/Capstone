using Microsoft.Extensions.Logging;
using Shared.Application.EventBus;
using Shared.Application.Messaging;
using Users.Domain.Events;
using Users.Domain.Repositories;
using Users.IntegrationEvents;

namespace Users.Application.Features.Users.EventHandlers;

public class PublishUserCreatedIntegrationHandler(
    IUserRepository userRepository,
    IEventBus eventBus,
    ILogger<PublishUserCreatedIntegrationHandler> logger
) : IDomainEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null) return;

        var integrationEvent = new UserIntegrationCreatedEvent(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName,
            user.Roles.Select(r => r.Name).ToList(),
            DateTime.UtcNow
        );

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
        logger.LogInformation("Integration event published for User: {UserId}", user.Id);
    }
}