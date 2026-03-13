using Shared.Domain.Abstractions;
using AI.Application.Services;
using AI.Application.Features.Tracking.Commands;
using Shared.Application.Abstractions.Messaging;
using AI.Domain.Repositories;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;

namespace AI.Application.Features.Tracking.Handlers
{
    public class TrackActivityCommandHandler : ICommandHandler<TrackActivityCommand, bool>
    {
        // private readonly IUserActivityOrchestrator _orchestrator;
        private readonly IUserBehaviorLogRepository _logRepository;
        private readonly IAiUnitOfWork _uow;


        public TrackActivityCommandHandler(
            // IUserActivityOrchestrator orchestrator
            IUserBehaviorLogRepository logRepository,
            IAiUnitOfWork unitOfWork
            )
        {
            _logRepository = logRepository;
            _uow = unitOfWork;
        }

        public async Task<Result<bool>> Handle(TrackActivityCommand command, CancellationToken cancellationToken)
        {
            // await _orchestrator.HandleUserActivityAsync(
            //     request.UserId,
            //     request.ActionType,
            //     request.TargetId,
            //     request.TargetType,
            //     request.Metadata,
            //     cancellationToken
            // );
            var log = UserBehaviorLog.Create(
            command.UserId,
            command.ActionType,
            command.TargetId,
            command.TargetType,
            command.Metadata
        );

            _logRepository.Add(log);
            await _uow.SaveChangesAsync();
            // 2. Return Success
            // (In the future, Failure if the Orchestrator throws a domain exception)
            return Result.Success(true);
        }
    }
}