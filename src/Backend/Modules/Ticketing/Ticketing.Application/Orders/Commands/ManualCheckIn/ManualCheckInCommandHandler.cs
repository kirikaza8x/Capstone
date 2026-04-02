using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Time;
using Shared.Domain.Abstractions;
using Ticketing.Application.Abstractions.Notifications;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Repositories;
using Ticketing.Domain.Uow;

namespace Ticketing.Application.Orders.Commands.ManualCheckIn;

internal sealed class ManualCheckInCommandHandler(
    IOrderRepository orderRepository,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    ITicketingUnitOfWork unitOfWork,
    ICheckInStatsBroadcaster checkInStatsBroadcaster) : ICommandHandler<ManualCheckInCommand, ManualCheckInResponse>
{
    public async Task<Result<ManualCheckInResponse>> Handle(ManualCheckInCommand command, CancellationToken cancellationToken)
    {
        var staffUserId = currentUserService.UserId;
        var utcNow = dateTimeProvider.UtcNow;
        int successCount = 0;

        var ticketIdSet = command.OrderTicketIds.ToHashSet();

        var orders = await orderRepository.GetByTicketIdsAsync(command.OrderTicketIds, cancellationToken);

        if (!orders.Any())
        {
            return Result.Failure<ManualCheckInResponse>(Error.NotFound(
                "CheckIn.TicketsNotFound",
                "No valid tickets found for processing."));
        }

        foreach (var order in orders)
        {
            if (order.EventId != command.EventId) continue;

            var ticketsToProcess = order.Tickets
                .Where(t => ticketIdSet.Contains(t.Id) &&
                            t.EventSessionId == command.EventSessionId &&
                            t.Status == OrderTicketStatus.Valid)
                .ToList();

            foreach (var ticket in ticketsToProcess)
            {
                var result = order.CheckIn(ticket.Id, staffUserId, utcNow);

                if (result.IsSuccess)
                    successCount++;
            }
        }

        if (successCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await checkInStatsBroadcaster.BroadcastAsync(
                command.EventId,
                command.EventSessionId,
                cancellationToken);

            return Result.Success(new ManualCheckInResponse(successCount));
        }

        return Result.Failure<ManualCheckInResponse>(Error.Conflict(
            "CheckIn.NoTicketsProcessed",
            "All selected tickets are already checked-in or invalid."));
    }
}
