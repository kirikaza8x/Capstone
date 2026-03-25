using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence.Contexts;
using Users.PublicApi.PublicApi;

namespace Users.Infrastructure.PublicApi;

internal sealed class UserPublicApi(UserModuleDbContext dbContext) : IUserPublicApi
{
    public async Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Roles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null) return null;

        return new UserInfo(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Roles.Select(r => r.Name).ToList());
    }

    public async Task<UserInfo?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(u => u.Roles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) return null;

        return new UserInfo(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Roles.Select(r => r.Name).ToList());
    }

    public async Task<Dictionary<Guid, UserInfo>> GetUserMapByIdsAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .Include(u => u.Roles)
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return users.ToDictionary(
            u => u.Id,
            u => new UserInfo(
                u.Id,
                u.Email,
                $"{u.FirstName} {u.LastName}".Trim(),
                u.Roles.Select(r => r.Name).ToList()
            )
        );
    }
}
