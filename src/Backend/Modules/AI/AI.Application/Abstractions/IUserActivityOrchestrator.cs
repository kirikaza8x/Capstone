
namespace AI.Application.Services
{
    public interface IUserActivityOrchestrator
    {
        Task HandleUserActivityAsync(Guid userId, string actionType, string targetId, string targetType, IReadOnlyDictionary<string, string>? metadata);
    };

}
