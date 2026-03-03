namespace AI.Application.Abstractions
{
    public interface IGlobalTrendService
    {
        // Task UpdateGlobalTrendsAsync();

        Task UpdateGlobalTrendsAsync(CancellationToken ct = default);
    }

}