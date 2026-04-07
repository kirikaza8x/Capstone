using Shared.Domain.Abstractions;

namespace AI.Application.Abstractions;

public interface IAiTokenQuotaService
{
    Task<Result> ConsumeAsync(
        Guid organizerId,
        int tokens,
        Guid? referenceId = null,
        CancellationToken ct = default);
}
