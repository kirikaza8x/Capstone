using AI.Domain.Entities;

namespace AI.Application.Abstractions;
public interface IMarketingService 
{ 
    Task<MarketingContent> GenerateContentAsync(UserPrompt prompt, CancellationToken cancellationToken = default); 
    Task PublishContentAsync(Guid contentId, CancellationToken cancellationToken = default); 
    Task CloseContentAsync(Guid contentId, CancellationToken cancellationToken = default); 
}