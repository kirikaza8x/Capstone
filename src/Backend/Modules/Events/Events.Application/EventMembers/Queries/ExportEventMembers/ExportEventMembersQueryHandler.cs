using Events.Domain.Enums;
using Events.Domain.Errors;
using Events.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Report;
using Shared.Domain.Abstractions;
using Users.PublicApi.PublicApi;

namespace Events.Application.EventMembers.Queries.ExportEventMembers;

public class ExportEventMembersQueryHandler(
    IFileImportExportService<EventMemberExportDto> excelService,
    IEventRepository eventRepository,
    IUserPublicApi userPublicApi,
    ICurrentUserService currentUserService
) : IQueryHandler<ExportEventMembersQuery, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        ExportEventMembersQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        // 1. Load event with members
        var @event = await eventRepository.GetByIdWithMembersAsync(query.EventId, cancellationToken);
        if (@event == null)
            return Result.Failure<byte[]>(EventErrors.Event.NotFound(query.EventId));
        if (@event.OrganizerId != userId)
            return Result.Failure<byte[]>(EventErrors.Event.NotOwner);

        var members = @event.Members;

        // 2. Collect userIds for member and assigned_by
        var userIds = members.Select(m => m.UserId)
            .Concat(members.Select(m => m.AssignedBy))
            .Distinct()
            .ToList();
        var userMap = await userPublicApi.GetUserMapByIdsAsync(userIds, cancellationToken);

        // 3. Map to export DTOs
        var exportDtos = members.Select((m, idx) =>
        {
            var memberUser = userMap.TryGetValue(m.UserId, out var u) ? u : null;
            var assignedByUser = userMap.TryGetValue(m.AssignedBy, out var a) ? a : null;

            // Permissions mapping
            var permissionMap = new Dictionary<string, string>
            {
                ["CheckIn"] = "Soát vé",
                ["ViewReports"] = "Xem báo cáo"
            };
            var permissions = string.Join(", ", m.Permissions.Select(p => permissionMap.TryGetValue(p, out var v) ? v : p));

            // Status mapping
            var status = m.Status == EventMemberStatus.Active ? "Đang hoạt động" : "Đã khoá";
            var isActive = m.Status == EventMemberStatus.Active ? "Có" : "Không";

            return new EventMemberExportDto
            {
                Index = idx + 1,
                Id = m.Id,
                MemberName = memberUser?.FullName ?? "",
                MemberEmail = memberUser?.Email ?? "",
                Permissions = permissions,
                Status = status,
                AssignedBy = assignedByUser?.FullName ?? "",
                CreatedAt = m.CreatedAt,
                CreatedBy = assignedByUser?.FullName ?? "",
                IsActive = isActive
            };
        }).ToList();

        var fileBytes = await excelService.ExportAsync(exportDtos, cancellationToken);
        return Result.Success(fileBytes);
    }
}
