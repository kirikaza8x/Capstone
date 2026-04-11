using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.Application.Abstractions.Notifications;
using Ticketing.Application.Helpers;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Application.Orders.Commands.CheckIn;

internal sealed class CheckInCommandHandler(
    IOrderRepository orderRepository,
    IEventTicketingPublicApi eventTicketingPublicApi,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    ITicketingUnitOfWork unitOfWork,
    ICheckInStatsBroadcaster checkInStatsBroadcaster) : ICommandHandler<CheckInCommand, CheckInResponse>
{
    public async Task<Result<CheckInResponse>> Handle(
        CheckInCommand command,
        CancellationToken cancellationToken)
    {
        var staffUserId = currentUserService.UserId;
        if (staffUserId == Guid.Empty)
            return Result.Failure<CheckInResponse>(Error.Unauthorized(
                "CheckIn.Unauthorized",
                "Current user is not authenticated."));

        if (!QrCodeHelper.TryParse(command.QrCode, out var orderTicketId, out var qrEventSessionId))
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.InvalidQrCode);

        if (qrEventSessionId != command.EventSessionId)
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.SessionMismatch);

        var order = await orderRepository.GetByOrderTicketIdAsync(orderTicketId, cancellationToken);
        if (order is null)
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.TicketNotFound);

        if (order.EventId != command.EventId)
            return Result.Failure<CheckInResponse>(Error.Forbidden(
                "CheckIn.WrongEvent",
                "This ticket does not belong to the current event."));

        var ticket = order.Tickets.FirstOrDefault(t => t.Id == orderTicketId);
        if (ticket is null)
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.TicketNotFound);

        var utcNow = dateTimeProvider.UtcNow;
        var checkInResult = order.CheckIn(orderTicketId, staffUserId, utcNow);
        if (checkInResult.IsFailure)
            return Result.Failure<CheckInResponse>(checkInResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await checkInStatsBroadcaster.BroadcastAsync(
            command.EventId,
            command.EventSessionId,
            cancellationToken);

        var checkInInfo = await eventTicketingPublicApi.GetTicketCheckInInfoAsync(
            ticket.TicketTypeId,
            ticket.EventSessionId,
            ticket.SeatId,
            cancellationToken);

        return Result.Success(new CheckInResponse(
            ticket.Id,
            checkInInfo?.TicketTypeName ?? ticket.TicketTypeId.ToString(),
            checkInInfo?.SessionTitle ?? string.Empty,
            checkInInfo?.SessionStartTime ?? utcNow,
            checkInInfo?.SeatCode,
            utcNow));
    }
}
