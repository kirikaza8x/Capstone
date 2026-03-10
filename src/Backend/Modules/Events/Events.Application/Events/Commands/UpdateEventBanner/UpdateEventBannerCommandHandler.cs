using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.Events.Commands.UpdateEventBanner;

internal sealed class UpdateEventBannerCommandHandler(
    IEventRepository eventRepository,
    IEventUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : ICommandHandler<UpdateEventBannerCommand>
{
    public async Task<Result> Handle(UpdateEventBannerCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        @event.UpdateBannerUrl(command.BannerUrl);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}