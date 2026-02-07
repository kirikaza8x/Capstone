using Shared.Application.Messaging;
using Shared.Domain.Abstractions; 
using AI.Application.Services;    
using AI.Application.Features.Tracking.Commands;

namespace AI.Application.Features.Tracking.Handlers
{
    public class TrackActivityCommandHandler : ICommandHandler<TrackActivityCommand, bool>
    {
        private readonly UserActivityOrchestrator _orchestrator;

        public TrackActivityCommandHandler(UserActivityOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public async Task<Result<bool>> Handle(TrackActivityCommand request, CancellationToken cancellationToken)
        {
            await _orchestrator.HandleUserActivityAsync(
                request.UserId,
                request.ActionType,
                request.TargetId,
                request.TargetType,
                request.Metadata
            );

            // 2. Return Success
            // (In the future, Failure if the Orchestrator throws a domain exception)
            return Result.Success(true);
        }
    }
}