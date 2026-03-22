using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
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
    ITicketingUnitOfWork unitOfWork) : ICommandHandler<CheckInCommand, CheckInResponse>
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

        //Parse QR
        if (!QrCodeHelper.TryParse(command.QrCode, out var orderTicketId))
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.InvalidQrCode);

        // Load OrderTicket
        var order = await orderRepository.GetByOrderTicketIdAsync(
            orderTicketId,
            cancellationToken);

        if (order is null)
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.TicketNotFound);

        var ticket = order.Tickets.FirstOrDefault(t => t.Id == orderTicketId);
        if (ticket is null)
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.TicketNotFound);

        //  Validate session
        if (ticket.EventSessionId != command.EventSessionId)
            return Result.Failure<CheckInResponse>(TicketingErrors.CheckIn.SessionMismatch);

        // Check-in
        var utcNow = dateTimeProvider.UtcNow;
        var checkInResult = order.CheckIn(orderTicketId, staffUserId, utcNow);
        if (checkInResult.IsFailure)
            return Result.Failure<CheckInResponse>(checkInResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Get check-in info for response
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
