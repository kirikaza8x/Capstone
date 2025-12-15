using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Events;
using Shared.Contracts.Events.Users;
using Shared.Domain.UnitOfWork;
using Users.Domain.Events;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.EventHandlers;

/// <summary>
/// Handles internal domain logic after user creation
/// Then publishes integration event to notify other services
/// </summary>
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICompositeUnitOfWork _unitOfWork;
    private readonly IServiceBusPublisher _serviceBusPublisher;  
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICompositeUnitOfWork unitOfWork,
        IServiceBusPublisher serviceBusPublisher,  
        ILogger<UserCreatedEventHandler> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _serviceBusPublisher = serviceBusPublisher; 
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "UserCreatedEventHandler triggered for UserId: {UserId}", 
                notification.UserId);

            
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            var user = await _userRepository.GetByIdAsync(notification.UserId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for UserId: {UserId}", notification.UserId);
                return;
            }

            // Assign default role
            var defaultRole = await _roleRepository.GetByRoleNameAsync("user", cancellationToken);
            if (defaultRole == null)
            {
                _logger.LogWarning("Default user role not found");
                return;
            }

            _logger.LogInformation("Assigning user role to user {UserId}", notification.UserId);
            user.AssignRole(defaultRole);

            // Save changes (this commits the transaction)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully assigned user role to user {UserId}", 
                notification.UserId);

           
            var integrationEvent = new UserIntegrationCreatedEvent
            {
                CorrelationId = Guid.NewGuid(),
                SourceService = "UserService",
                UserId = notification.UserId,
                Email = notification.Email,
                UserName = notification.UserName,
                Roles = user.Roles.Select(r => r.Name).ToList(),  
                CreatedAt = notification.OccurredOn
            };

            // ⭐ Fire-and-forget pattern (don't block domain logic)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _serviceBusPublisher.PublishAsync(integrationEvent, cancellationToken);
                    
                    _logger.LogInformation(
                        "Published UserCreatedEvent integration event for UserId: {UserId}", 
                        notification.UserId);
                }
                catch (Exception ex)
                {
                    // Domain logic already succeeded, event publishing is "nice to have"
                    _logger.LogError(
                        ex, 
                        "Failed to publish integration event for UserId: {UserId}", 
                        notification.UserId);
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error in UserCreatedEventHandler for UserId: {UserId}", 
                notification.UserId);
            throw;  // ⚠️ Throw for domain logic errors
        }
    }
}