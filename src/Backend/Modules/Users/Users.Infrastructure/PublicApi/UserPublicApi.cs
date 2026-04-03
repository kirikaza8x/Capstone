using Microsoft.EntityFrameworkCore;
using Shared.Application.Abstractions.Time;
using Users.Infrastructure.Persistence.Contexts;
using Users.PublicApi.PublicApi;
using Users.PublicApi.Records;

namespace Users.Infrastructure.PublicApi;

internal sealed class UserPublicApi(
    UserModuleDbContext dbContext,
    IDateTimeProvider dateTimeProvider) 
    : IUserPublicApi
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

    public async Task<UserMetricsDto> GetUserMetricsAsync(CancellationToken cancellationToken = default)
    {
        var now = dateTimeProvider.UtcNow;
        // get user growth rate in the last 30 days compared to the previous 30 days
        var startOfCurrentPeriod = now.AddDays(-30);
        var startOfLastPeriod = now.AddDays(-60);

        var attendees = await dbContext.Users
            .AsNoTracking()
            .CountAsync(u => u.Roles.Any(r => r.Name == Users.PublicApi.Constants.Roles.Attendee), cancellationToken);

        var organizers = await dbContext.Users
            .AsNoTracking()
            .CountAsync(u => u.Roles.Any(r => r.Name == Users.PublicApi.Constants.Roles.Organizer), cancellationToken);

        // Calculate the number of users registered in the last 30 days
        var current = await dbContext.Users
            .CountAsync(u => u.CreatedAt >= startOfCurrentPeriod, cancellationToken);

        // Calculate the number of users registered in the previous 30 days
        var last = await dbContext.Users
            .CountAsync(u => u.CreatedAt >= startOfLastPeriod && u.CreatedAt < startOfCurrentPeriod, cancellationToken);

        double growthRate = 0;

        if (last == 0)
        {
            growthRate = current > 0 ? 100.0 : 0.0;
        }
        else
        {
            growthRate = Math.Round((double)(current - last) / last * 100, 1);
        }

        return new UserMetricsDto(
                    TotalAttendees: attendees,
                    TotalOrganizers: organizers,
                    UserGrowthRate: growthRate);
    }
}
