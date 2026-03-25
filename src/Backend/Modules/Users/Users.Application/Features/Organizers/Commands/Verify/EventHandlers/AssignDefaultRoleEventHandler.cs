using Microsoft.Extensions.Logging;
using Shared.Application.Abstractions.Messaging;
using Users.Domain.Entities;
using Users.Domain.Events;
using Users.Domain.Repositories;
using Users.Domain.UOW;

namespace Users.Application.Features.Users.EventHandlers;

public class AssignOrganizerRoleEventHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUserUnitOfWork unitOfWork,
    ILogger<AssignOrganizerRoleEventHandler> logger
) : IDomainEventHandler<OrganizerProfileVerifiedEvent>
{
    public async Task Handle(OrganizerProfileVerifiedEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(notification.UserId, cancellationToken);
        if (user == null || user.Roles.Any(r => r.Name == PublicApi.Constants.Roles.Organizer)) return;

        var defaultRole = await roleRepository.GetByRoleNameAsync(PublicApi.Constants.Roles.Organizer, cancellationToken)
                          ?? await CreateAndPersistDefaultRole(cancellationToken);

        user.AssignRole(defaultRole);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Organizer role assigned to User: {UserId}", notification.UserId);
    }

    private async Task<Role> CreateAndPersistDefaultRole(CancellationToken ct)
    {
        var role = Role.Create(PublicApi.Constants.Roles.Organizer, "Organizer role.");
        roleRepository.Add(role);
        await unitOfWork.SaveChangesAsync(ct);
        return role;
    }
}
