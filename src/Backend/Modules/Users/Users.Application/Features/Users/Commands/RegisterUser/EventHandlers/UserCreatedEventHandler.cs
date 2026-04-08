using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.EventBus;
using Shared.Application.Abstractions.Messaging;
using Users.Domain.Events;
using Users.Domain.Repositories;
using Users.Domain.UOW;
using Users.IntegrationEvents;

namespace Users.Application.Features.Users.EventHandlers;

public class PublishUserCreatedIntegrationHandler(
    IUserRepository userRepository,
    // IUserUnitOfWork userUnitOfWork,
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

        await eventBus.PublishAsync(integrationEvent, CancellationToken.None); 
        logger.LogInformation("Integration event published for User: {UserId}", user.Id);
    }
}
