using ClosedXML.Excel;
using Shared.Application.Abstractions.Report;
using Events.Application.EventMembers.Queries.ExportEventMembers;

namespace Events.Infrastructure.Services.Reports;

public class EventMemberSheetMappings : ISheetMappings<EventMemberExportDto>
{
    public Func<object, EventMemberExportDto> GetRowMapper()
        => _ => throw new NotSupportedException("Import is not supported for EventMemberExportDto.");

    public Action<object, IEnumerable<EventMemberExportDto>> Exporter => (wsObj, members) =>
    {
        var ws = (IXLWorksheet)wsObj;

        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 2).Value = "ID";
        ws.Cell(1, 3).Value = "Tên thành viên";
        ws.Cell(1, 4).Value = "Email thành viên";
        ws.Cell(1, 5).Value = "Quyền hạn";
        ws.Cell(1, 6).Value = "Trạng thái";
        ws.Cell(1, 7).Value = "Người phân công";
        ws.Cell(1, 8).Value = "Ngày tham gia";
        ws.Cell(1, 9).Value = "Người tạo";
        ws.Cell(1, 10).Value = "Còn hoạt động";

        int row = 2;
        foreach (var m in members)
        {
            ws.Cell(row, 1).Value = m.Index;
            ws.Cell(row, 2).Value = m.Id.ToString();
            ws.Cell(row, 3).Value = m.MemberName;
            ws.Cell(row, 4).Value = m.MemberEmail;
            ws.Cell(row, 5).Value = m.Permissions;
            ws.Cell(row, 6).Value = m.Status;
            ws.Cell(row, 7).Value = m.AssignedBy;
            ws.Cell(row, 8).Value = m.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            ws.Cell(row, 9).Value = m.CreatedBy;
            ws.Cell(row, 10).Value = m.IsActive;
            row++;
        }

        ws.Columns().AdjustToContents();
    };
}
