using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Domain.UnitOfWork;
using Users.Domain.Events;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.EventHandlers
{
    public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly ICompositeUnitOfWork _unitOfWork;
        private readonly ILogger<UserCreatedEventHandler> _logger;

        public UserCreatedEventHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ICompositeUnitOfWork unitOfWork,
            ILogger<UserCreatedEventHandler> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("UserCreatedEventHandler triggered for UserId: {UserId}", notification.UserId);

                var user = await _userRepository.GetByIdAsync(notification.UserId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("User not found for UserId: {UserId}", notification.UserId);
                    return;
                }

                var defaultRole = await _roleRepository.GetByRoleNameAsync("user", cancellationToken);
                if (defaultRole == null)
                {
                    _logger.LogWarning("Default admin role not found");
                    return;
                }

                _logger.LogInformation("Assigning admin role to user {UserId}", notification.UserId);
                user.AssignRole(defaultRole);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully assigned admin role to user {UserId}", notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UserCreatedEventHandler for UserId: {UserId}", notification.UserId);
                throw;
            }
        }
    }
}
