using AI.Domain.Entities;
using Shared.Domain.Data.Repositories;

namespace AI.Domain.Repositories;

/// <summary>
/// Repository for UserInterestScore with optimized batch operations.
/// Critical path: decay + add score must be atomic to avoid race conditions.
/// </summary>
public interface IUserInterestScoreRepository : IRepository<UserInterestScore, Guid>
{
    // ─────────────────────────────────────────────────────────────
    // Basic CRUD
    // ─────────────────────────────────────────────────────────────

    Task<UserInterestScore?> GetByUserAndCategoryAsync(
     Guid userId,
     string category,
     CancellationToken ct = default);

    Task<List<UserInterestScore>> GetAllForUserAsync(
        Guid userId,
        CancellationToken ct = default);

    void Delete(UserInterestScore entity);

    // ─────────────────────────────────────────────────────────────
    // ⭐ Optimization: Batch Operations
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Fetches multiple categories for a user in a single DB call.
    /// Reduces N+1 queries when processing logs with multiple categories.
    /// </summary>
    Task<List<UserInterestScore>> GetByUserAndCategoriesAsync(
        Guid userId,
        List<string> categories,
        CancellationToken ct = default);

    /// <summary>
    /// ⭐ Thread-safe UPSERT with decay + add in one operation.
    /// Prevents race conditions when multiple logs arrive simultaneously.
    /// 
    /// Logic:
    /// 1. Try to fetch existing score for user+category
    /// 2. If exists: ApplyDecay(halfLifeDays) → AddScore(weight) → Update
    /// 3. If not exists: Create new with initialScore = weight
    /// 
    /// Returns the final entity (existing or new).
    /// </summary>
    Task<UserInterestScore> UpsertWithDecayAsync(
        Guid userId,
        string category,
        double weight,
        double halfLifeDays,
        CancellationToken ct = default);

    /// <summary>
    /// Bulk upsert for multiple categories at once.
    /// More efficient than calling UpsertWithDecayAsync in a loop.
    /// </summary>
    Task<List<UserInterestScore>> BulkUpsertWithDecayAsync(
        Guid userId,
        Dictionary<string, double> categoryWeights,  // category → weight
        double halfLifeDays,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Recommendation Queries
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets top N categories for a user ordered by score (descending).
    /// Used for personalized recommendations.
    /// </summary>
    Task<List<UserInterestScore>> GetTopCategoriesAsync(
        Guid userId,
        int topN = 10,
        double minScore = 1.0,
        CancellationToken ct = default);

    /// <summary>
    /// Gets categories where user score exceeds threshold.
    /// Used for filtering recommendations by interest strength.
    /// </summary>
    Task<List<string>> GetCategoriesAboveThresholdAsync(
        Guid userId,
        double minScore,
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Analytics Queries
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all scores that haven't been updated in X days (for cleanup).
    /// </summary>
    Task<List<UserInterestScore>> GetStaleScoresAsync(
        int daysThreshold = 90,
        double maxScore = 1.0,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the total number of tracked interests for a user.
    /// </summary>
    Task<int> GetInterestCountAsync(
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets aggregate stats across all users (for monitoring).
    /// </summary>
    Task<Dictionary<string, object>> GetAggregateStatsAsync(
        CancellationToken ct = default);

    // ─────────────────────────────────────────────────────────────
    // Cleanup
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Marks scores as inactive (soft delete) for archival.
    /// </summary>
    Task<int> ArchiveStaleScoresAsync(
        int daysThreshold = 180,
        double maxScore = 0.5,
        CancellationToken ct = default);
}
