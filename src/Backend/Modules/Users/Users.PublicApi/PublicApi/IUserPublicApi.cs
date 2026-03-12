namespace Users.PublicApi.Services;

public sealed record UserInfo(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);

public interface IUserPublicApi
{
    Task<UserInfo?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}