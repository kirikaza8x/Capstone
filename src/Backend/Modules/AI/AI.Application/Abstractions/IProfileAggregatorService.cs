namespace AI.Application.Abstractions
{
    public interface IProfileAggregatorService
    {
        /// <summary>
        /// Aggregates a user's behavior logs into interest scores.
        /// 
        /// Algorithm:
        /// - Each user action (view, click, register, etc.) is mapped to a weight.
        /// - Scores are accumulated per category based on these weights.
        /// Purpose:
        /// - Builds a profile that reflects the user's preferences.
        /// </summary>
        Task AggregateUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Applies exponential decay to a user's interest scores.
        /// 
        /// Algorithm:
        /// - score = score * e^(-λ * Δt), where λ is the decay factor and Δt is elapsed time.
        /// Purpose:
        /// - Prevents old interactions from dominating.
        /// - Keeps recommendations fresh and time-sensitive.
        /// </summary>
        Task ApplyDecayAsync(Guid userId, double decayFactor, CancellationToken cancellationToken = default);
    }
}
