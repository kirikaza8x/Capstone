using AI.Application.Features.Tracking.Commands;
using AI.Domain.Entities;
using AI.Domain.Interfaces.UOW;
using AI.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace AI.Application.Features.Tracking.Handlers
{
    public class TrackActivityCommandHandler : ICommandHandler<TrackActivityCommand, bool>
    {
        private readonly IUserBehaviorLogRepository _logRepository;
        private readonly IAiUnitOfWork _uow;


        public TrackActivityCommandHandler(
            IUserBehaviorLogRepository logRepository,
            IAiUnitOfWork unitOfWork
            )
        {
            _logRepository = logRepository;
            _uow = unitOfWork;
        }

        public async Task<Result<bool>> Handle(TrackActivityCommand command, CancellationToken cancellationToken)
        {
            var log = UserBehaviorLog.Create(
            command.UserId,
            command.ActionType,
            command.TargetId,
            command.TargetType,
            command.Metadata
        );

            _logRepository.Add(log);
            await _uow.SaveChangesAsync();

            return Result.Success(true);
        }
    }
}
