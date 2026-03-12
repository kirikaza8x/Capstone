using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Persistence.Contexts;
using Users.PublicApi.Services;

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
}