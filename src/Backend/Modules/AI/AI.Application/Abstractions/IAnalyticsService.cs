using AI.Domain.Entities;
namespace AI.Application.Abstractions;
public interface IAnalyticsService { 
    Task AddAnalyticsAsync(Guid contentId, int views, int clicks, int conversions, CancellationToken cancellationToken = default); 
    Task<IReadOnlyList<MarketingAnalytics>> GetAnalyticsAsync(Guid contentId, CancellationToken cancellationToken = default); 
    Task<double> ComputeCTRAsync(Guid contentId, CancellationToken cancellationToken = default); 
    Task<double> ComputeConversionRateAsync(Guid contentId, CancellationToken cancellationToken = default); 
    Task<double> ForecastConversionsAsync(Guid contentId, int daysAhead, CancellationToken cancellationToken = default); 
}