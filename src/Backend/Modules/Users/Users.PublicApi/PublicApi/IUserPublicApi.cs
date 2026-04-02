using Users.PublicApi.Records;

namespace Users.PublicApi.PublicApi;

public sealed record UserInfo(Guid Id, string? Email, string FullName, IReadOnlyList<string> Roles);


public interface IUserPublicApi
{
    Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, UserInfo>> GetUserMapByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
    Task<UserMetricsDto> GetUserMetricsAsync(CancellationToken cancellationToken = default);
}
