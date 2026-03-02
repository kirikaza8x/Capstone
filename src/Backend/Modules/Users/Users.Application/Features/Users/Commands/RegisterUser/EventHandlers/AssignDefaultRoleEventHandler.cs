using Microsoft.Extensions.Logging;
using Shared.Application.Messaging;
using Users.Domain.Entities;
using Users.Domain.Events;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.EventHandlers;

public class AssignDefaultRoleEventHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserUnitOfWork unitOfWork,
    ILogger<AssignDefaultRoleEventHandler> logger
) : IDomainEventHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null || user.Roles.Any()) return;

        var defaultRole = await roleRepository.GetByRoleNameAsync(PublicApi.Constants.Roles.Attendee, cancellationToken)
                          ?? await CreateAndPersistDefaultRole(cancellationToken);

        user.AssignRole(defaultRole);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Default role assigned to User: {UserId}", notification.UserId);
    }

    private async Task<Role> CreateAndPersistDefaultRole(CancellationToken ct)
    {
        var role = Role.Create(PublicApi.Constants.Roles.Attendee, "Default system role.");
        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(ct);
        return role;
    }
}