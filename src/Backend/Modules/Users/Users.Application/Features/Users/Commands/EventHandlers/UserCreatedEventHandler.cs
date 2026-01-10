// using Microsoft.Extensions.Logging;
// using Shared.Application.EventBus;
// using Shared.Application.Messaging;
// using Users.Domain.Events;
// using Users.Domain.Repositories;

// namespace Users.Application.Features.Users.EventHandlers;

// /// <summary>
// /// Handles internal domain logic after user creation
// /// Then publishes integration event to notify other services
// /// </summary>
// public class UserCreatedEventHandler : IDomainEventHandler<UserCreatedEvent>
// {
//     private readonly IUserRepository _userRepository;
//     private readonly IRoleRepository _roleRepository;
//     private readonly IEventBus _eventBus;
//     private readonly ILogger<UserCreatedEventHandler> _logger;

//     public UserCreatedEventHandler(
//         IUserRepository userRepository,
//         IRoleRepository roleRepository,
//         IEventBus eventBus,
//         ILogger<UserCreatedEventHandler> logger)
//     {
//         _userRepository = userRepository;
//         _roleRepository = roleRepository;
//         _eventBus = eventBus;
//         _logger = logger;
//     }

//     public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
//     {
//         try
//         {
//             _logger.LogInformation("Processing user creation logic for UserId: {UserId}", notification.UserId);

//             var user = await _userRepository.GetByIdAsync(notification.UserId, cancellationToken);
//             if (user == null)
//             {
//                 _logger.LogWarning("User not found for UserId: {UserId}", notification.UserId);
//                 return;
//             }

//             var defaultRole = await _roleRepository.GetByRoleNameAsync("user", cancellationToken);
//             if (defaultRole != null)
//             {
//                 user.AssignRole(defaultRole);
//                 // Save inside the same transaction
//                 await _unitOfWork.SaveChangesAsync(cancellationToken);
//             }

           
//             var integrationEvent = new UserIntegrationCreatedEvent
//             {
//                 CorrelationId = Guid.NewGuid(),
//                 SourceService = "UserService",
//                 UserId = notification.UserId,
//                 Email = notification.Email,
//                 UserName = notification.UserName,
//                 Roles = user.Roles.Select(r => r.Name).ToList(),
//                 CreatedAt = DateTime.UtcNow
//             };

           

//             _logger.LogInformation("User {UserId} logic complete and event published.", notification.UserId);
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Failed to process user creation for {UserId}", notification.UserId);
//             throw; // Re-throw so the system knows the operation failed
//         }
//     }
// }