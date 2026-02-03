
namespace AI.Application.Abstractions;

public interface IProfileAggregatorService
{
    Task AggregateUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ApplyDecayAsync(Guid userId, double decayFactor, CancellationToken cancellationToken = default);
}