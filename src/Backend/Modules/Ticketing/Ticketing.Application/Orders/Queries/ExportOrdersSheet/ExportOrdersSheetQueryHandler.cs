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
    IFileImportExportService<OrderExportDto> _excelService,
    IOrderRepository _orderRepository,
    IUserPublicApi _userPublicApi,
    IVoucherRepository _voucherRepository,
    ICurrentUserService currentUserService,
    IEventTicketingPublicApi eventTicketingPublicApi
    )
    : IQueryHandler<ExportOrdersSheetQuery, byte[]>
{

    public async Task<Result<byte[]>> Handle(
           ExportOrdersSheetQuery query,
           CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            new[] { query.EventId }, cancellationToken);
        var eventSummary = eventSummaryMap.TryGetValue(query.EventId, out var summary) ? summary : null;
        if (eventSummary == null)
            return Result.Failure<byte[]>(TicketingErrors.Order.NotFound(query.EventId));

        // Check owner event
        if (eventSummary.OrganizerId != userId)
            return Result.Failure<byte[]>(TicketingErrors.Event.NotOwner);

        // Get orders
        var orders = await _orderRepository.GetAllByEventIdAsync(query.EventId, cancellationToken);

        // 2. Get user info
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var userMap = await _userPublicApi.GetUserMapByIdsAsync(userIds, cancellationToken);

        // 3. get voucher info
        var voucherIds = orders
            .SelectMany(o => o.OrderVouchers.Select(v => v.VoucherId))
            .Distinct()
            .ToList();
        var voucherMap = await _voucherRepository.GetVoucherMapByIdsAsync(voucherIds, cancellationToken);

        // 4. Mapping data to export DTO
        var exportDtos = orders.Select((order, idx) =>
        {
            var user = userMap.TryGetValue(order.UserId, out var u) ? u : null;
            var orderVoucher = order.OrderVouchers.FirstOrDefault();
            Voucher? voucher = null;
            if (orderVoucher != null)
                voucherMap.TryGetValue(orderVoucher.VoucherId, out voucher);

            // calculate discount amount
            decimal? discountAmount = null;
            if (voucher != null)
            {
                if (voucher.Type == VoucherType.Percentage)
                    discountAmount = Math.Round(order.OriginalTotalPrice * voucher.Value / 100, 2);
                else if (voucher.Type == VoucherType.Fixed)
                    discountAmount = Math.Min(voucher.Value, order.OriginalTotalPrice);
            }
            Guid createdById;
            var createdByName = (order.CreatedBy != null
                && Guid.TryParse(order.CreatedBy, out createdById)
                && userMap.TryGetValue(createdById, out var creatorUser))
                ? creatorUser.FullName
                : "";

            return new OrderExportDto
            {
                Index = idx + 1,
                OrderId = order.Id,
                BuyerName = user?.FullName ?? "",
                BuyerEmail = user?.Email ?? "",
                TotalPrice = order.OriginalTotalPrice,
                CouponCode = voucher?.CouponCode,
                VoucherType = voucher?.Type.ToString(),
                DiscountAmount = discountAmount,
                FinalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt ?? DateTime.MinValue,
                CreatedBy = createdByName
            };
        }).ToList();

        // 5. Export Excel
        var fileBytes = await _excelService.ExportAsync(exportDtos, cancellationToken);

        return Result.Success(fileBytes);
    }
}
