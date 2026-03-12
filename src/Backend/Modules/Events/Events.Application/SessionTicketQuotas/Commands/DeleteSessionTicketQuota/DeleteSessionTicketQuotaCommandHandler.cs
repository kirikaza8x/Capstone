using Events.Domain.Errors;
using Events.Domain.Repositories;
using Events.Domain.Uow;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;

namespace Events.Application.SessionTicketQuotas.Commands.DeleteSessionTicketQuota;

internal sealed class DeleteSessionTicketQuotaCommandHandler(
    IEventRepository eventRepository,
    ISessionTicketQuotaRepository quotaRepository,
    ICurrentUserService currentUserService,
    IEventUnitOfWork unitOfWork) : ICommandHandler<DeleteSessionTicketQuotaCommand>
{
    public async Task<Result> Handle(DeleteSessionTicketQuotaCommand command, CancellationToken cancellationToken)
    {
        var @event = await eventRepository.GetByIdAsync(command.EventId, cancellationToken);

        if (@event is null)
            return Result.Failure(EventErrors.Event.NotFound(command.EventId));

        if (@event.OrganizerId != currentUserService.UserId)
            return Result.Failure(EventErrors.Event.NotOwner);

        var quota = await quotaRepository.GetBySessionAndTicketTypeAsync(
            command.SessionId, command.TicketTypeId, cancellationToken);

        if (quota is null)
            return Result.Failure(EventErrors.SessionTicketQuotaErrors.NotFound(command.SessionId, command.TicketTypeId));

        quotaRepository.Remove(quota);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}