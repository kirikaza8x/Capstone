using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Report;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Users.PublicApi.PublicApi;

namespace Ticketing.Application.Orders.Queries.ExportOrdersSheet;

internal class ExportOrdersSheetQueryHandler(
    IFileImportExportService<OrderExportDto> excelService,
    IOrderRepository orderRepository,
    IUserPublicApi userPublicApi,
    IVoucherRepository voucherRepository,
    ICurrentUserService currentUserService,
    IEventTicketingPublicApi eventTicketingPublicApi)
    : IQueryHandler<ExportOrdersSheetQuery, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportOrdersSheetQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            [query.EventId],
            cancellationToken);

        var eventSummary = eventSummaryMap.TryGetValue(query.EventId, out var summary)
            ? summary
            : null;

        if (eventSummary is null)
            return Result.Failure<byte[]>(TicketingErrors.Order.NotFound(query.EventId));

        if (eventSummary.OrganizerId != userId)
            return Result.Failure<byte[]>(TicketingErrors.Event.NotOwner);

        var orders = await orderRepository.GetAllByEventIdAsync(query.EventId, cancellationToken);

        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var userMap = await userPublicApi.GetUserMapByIdsAsync(userIds, cancellationToken);

        var voucherIds = orders
            .SelectMany(o => o.OrderVouchers.Select(v => v.VoucherId))
            .Distinct()
            .ToList();

        var voucherMap = await voucherRepository.GetVoucherMapByIdsAsync(voucherIds, cancellationToken);

        var ticketDetailRequests = orders
            .SelectMany(o => o.Tickets.Select(t => (t.TicketTypeId, t.EventSessionId, t.SeatId)))
            .Distinct()
            .ToList();

        var ticketDetailsMap = await eventTicketingPublicApi.GetOrderTicketDetailsAsync(
            ticketDetailRequests,
            cancellationToken);

        var exportDtos = new List<OrderExportDto>();
        var index = 1;

        foreach (var order in orders)
        {
            var buyer = userMap.TryGetValue(order.UserId, out var buyerInfo) ? buyerInfo : null;

            var orderVoucher = order.OrderVouchers.FirstOrDefault();
            Voucher? voucher = null;
            if (orderVoucher is not null)
            {
                voucherMap.TryGetValue(orderVoucher.VoucherId, out voucher);
            }

            decimal? discountAmount = null;
            if (voucher is not null)
            {
                if (voucher.Type == VoucherType.Percentage)
                {
                    discountAmount = Math.Round(order.OriginalTotalPrice * voucher.Value / 100, 2);
                }
                else if (voucher.Type == VoucherType.Fixed)
                {
                    discountAmount = Math.Min(voucher.Value, order.OriginalTotalPrice);
                }
            }

            if (order.Tickets.Count == 0)
            {
                exportDtos.Add(new OrderExportDto
                {
                    Index = index++,
                    OrderId = order.Id,
                    CreatedAt = order.CreatedAt ?? DateTime.MinValue,
                    Status = order.Status.ToString(),
                    BuyerName = buyer?.FullName ?? string.Empty,
                    BuyerEmail = buyer?.Email ?? string.Empty,
                    OriginalPrice = order.OriginalTotalPrice,
                    DiscountAmount = discountAmount,
                    FinalPrice = order.TotalPrice,
                    CouponCode = voucher?.CouponCode,
                    EventName = eventSummary.EventTitle,
                    Location = eventSummary.Location ?? string.Empty,
                    EventStartAt = eventSummary.EventStartAt ?? DateTime.MinValue
                });

                continue;
            }

            foreach (var ticket in order.Tickets)
            {
                var key = (ticket.TicketTypeId, ticket.EventSessionId);
                var hasDetail = ticketDetailsMap.TryGetValue(key, out var detail);

                exportDtos.Add(new OrderExportDto
                {
                    Index = index++,
                    OrderId = order.Id,
                    CreatedAt = order.CreatedAt ?? DateTime.MinValue,
                    Status = order.Status.ToString(),
                    BuyerName = buyer?.FullName ?? string.Empty,
                    BuyerEmail = buyer?.Email ?? string.Empty,
                    OriginalPrice = order.OriginalTotalPrice,
                    DiscountAmount = discountAmount,
                    FinalPrice = order.TotalPrice,
                    CouponCode = voucher?.CouponCode,
                    EventName = eventSummary.EventTitle,
                    Location = eventSummary.Location ?? string.Empty,
                    EventStartAt = eventSummary.EventStartAt ?? DateTime.MinValue,
                    TicketId = ticket.Id,
                    TicketType = hasDetail ? detail!.TicketTypeName : null,
                    TicketPrice = hasDetail ? detail!.Price : ticket.Price,
                    TicketStatus = ticket.Status.ToString(),
                    SessionTitle = hasDetail ? detail!.SessionTitle : null,
                    SessionStartTime = hasDetail ? detail!.SessionStartTime : null,
                    SeatCode = hasDetail ? detail!.SeatCode : null
                });
            }
        }

        var fileBytes = await excelService.ExportAsync(exportDtos, cancellationToken);
        return Result.Success(fileBytes);
    }
}
