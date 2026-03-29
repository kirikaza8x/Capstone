using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Users.PublicApi.PublicApi;

namespace Ticketing.Application.Orders.Queries.GetTicketsForCheckIn;

internal class GetTicketsForCheckInQueryHandler(
    IOrderRepository orderRepository,
    IUserPublicApi userPublicApi,
    IEventTicketingPublicApi eventTicketingPublicApi)
    : IQueryHandler<GetTicketsForCheckInQuery, IReadOnlyCollection<TicketForCheckInResponse>>
{
    public async Task<Result<IReadOnlyCollection<TicketForCheckInResponse>>> Handle(GetTicketsForCheckInQuery query, CancellationToken cancellationToken)
    {
        var userInfo = await userPublicApi.GetByEmailAsync(query.Email, cancellationToken);

        if (userInfo is null)
        {
            return Result.Failure<IReadOnlyCollection<TicketForCheckInResponse>>(TicketingErrors.User.EmailNotFound(query.Email));
        }

        var customerUserId = userInfo.Id;
        var orders = await orderRepository.GetByUserIdAndEventIdAsync(
                customerUserId,
                query.EventId,
                cancellationToken);

        if (orders is null || !orders.Any())
        {
            return Result.Success<IReadOnlyCollection<TicketForCheckInResponse>>([]);
        }

        // get tickets for the session
        var sessionTickets = orders
            .SelectMany(o => o.Tickets)
            .Where(t => t.EventSessionId == query.EventSessionId &&
                        t.Status != Domain.Enums.OrderTicketStatus.Cancelled)
            .ToList();

        if (!sessionTickets.Any())
        {
            return Result.Success<IReadOnlyCollection<TicketForCheckInResponse>>([]);
        }

        // get ticket details from EventTicketing service
        var ticketItems = sessionTickets
            .Select(t => (t.TicketTypeId, t.EventSessionId, t.SeatId))
            .ToList();

        var ticketDetailMap = await eventTicketingPublicApi
            .GetOrderTicketDetailsAsync(ticketItems, cancellationToken);

        // wrap data into response
        var responses = sessionTickets.Select(t =>
        {
            var detail = ticketDetailMap.TryGetValue((t.TicketTypeId, t.EventSessionId), out var d) ? d : null;
            bool isCheckedIn = t.Status == Domain.Enums.OrderTicketStatus.Valid ? true : false ;
            return new TicketForCheckInResponse(
                t.Id,
                detail?.TicketTypeName ?? string.Empty,
                detail?.SeatCode,
                isCheckedIn,
                t.CheckedInAt);
        }).ToList();

        return Result.Success<IReadOnlyCollection<TicketForCheckInResponse>>(responses);
    }
}
