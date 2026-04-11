// Modules/Ticketing/Ticketing.Application/Orders/Queries/ExportVoucherSheet/ExportVoucherSheetQueryHandler.cs
using Events.PublicApi.PublicApi;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Report;
using Shared.Domain.Abstractions;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Errors;
using Ticketing.Domain.Repositories;
using Users.PublicApi.PublicApi;

namespace Ticketing.Application.Orders.Queries.ExportVoucherSheet;

public class ExportVoucherSheetQueryHandler(
    IFileImportExportService<VoucherExportDto> excelService,
    IVoucherRepository voucherRepository,
    IUserPublicApi userPublicApi,
    ICurrentUserService currentUserService,
    IEventTicketingPublicApi eventTicketingPublicApi
) : IQueryHandler<ExportVoucherSheetQuery, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportVoucherSheetQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        // Check event ownership
        var eventSummaryMap = await eventTicketingPublicApi.GetEventSummaryByEventIdsAsync(
            new[] { query.EventId }, cancellationToken);
        var eventSummary = eventSummaryMap.TryGetValue(query.EventId, out var summary) ? summary : null;
        if (eventSummary == null)
            return Result.Failure<byte[]>(TicketingErrors.Order.NotFound(query.EventId));
        if (eventSummary.OrganizerId != userId)
            return Result.Failure<byte[]>(TicketingErrors.Event.NotOwner);

        // Get vouchers for this event and this organizer only
        var vouchers = await voucherRepository.GetByEventAndCreatorAsync(query.EventId, userId, cancellationToken);

        // Get user info for CreatedBy
        var userIds = vouchers
            .Select(v => v.CreatedBy)
            .Where(x => Guid.TryParse(x, out _))
            .Select(x => Guid.Parse(x!))
            .Distinct()
            .ToList();
        var userMap = await userPublicApi.GetUserMapByIdsAsync(userIds, cancellationToken);

        var exportDtos = vouchers.Select((v, idx) =>
        {
            var typeText = v.Type == VoucherType.Percentage ? "Phần trăm" : "Số tiền cố định";
            var valueText = v.Type == VoucherType.Percentage
                ? $"{v.Value:0.#}%"
                : $"{v.Value:0,0}đ";
            var usedText = $"{v.TotalUse}/{v.MaxUse}";
            var isActiveText = (v.StartDate <= DateTime.UtcNow && v.EndDate >= DateTime.UtcNow) ? "Đang hoạt động" : "Đã tắt";
            string createdByName = "";
            if (v.CreatedBy != null && Guid.TryParse(v.CreatedBy, out var createdById) && userMap.TryGetValue(createdById, out var creatorUser))
                createdByName = creatorUser.FullName;

            return new VoucherExportDto
            {
                Index = idx + 1,
                Id = v.Id,
                CouponCode = v.CouponCode,
                Type = typeText,
                Value = valueText,
                Used = usedText,
                StartDate = v.StartDate,
                EndDate = v.EndDate,
                IsActive = isActiveText,
                CreatedAt = v.CreatedAt ?? DateTime.MinValue,
                CreatedBy = createdByName
            };
        }).ToList();

        var fileBytes = await excelService.ExportAsync(exportDtos, cancellationToken);
        return Result.Success(fileBytes);
    }
}
