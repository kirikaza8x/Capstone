using Shared.Domain.Data.Repositories;
using Users.Domain.Entities;

public interface IOrganizerProfileRepository : IRepository<OrganizerProfile, Guid>
{
    // Task<OrganizerProfile?> GetByUserIdAsync(
    //     Guid userId,
    //     CancellationToken cancellationToken);

    // Task<OrganizerProfile?> GetByProfileIdAsync(
    //     Guid profileId,
    //     CancellationToken cancellationToken);
}