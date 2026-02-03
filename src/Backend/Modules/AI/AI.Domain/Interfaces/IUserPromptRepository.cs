using AI.Domain.Entities;
using Shared.Domain.Data;

namespace AI.Domain.Repositories;

public interface IUserPromptRepository : IRepository<UserPrompt, Guid>
{
    Task<IReadOnlyList<UserPrompt>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserPrompt>> GetByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
