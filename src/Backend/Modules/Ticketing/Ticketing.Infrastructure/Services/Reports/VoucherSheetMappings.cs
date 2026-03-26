using ClosedXML.Excel;
using Shared.Application.Abstractions.Report;
using Ticketing.Application.Orders.Queries.ExportVoucherSheet;

namespace Ticketing.Infrastructure.Services.Reports;

public class VoucherSheetMappings : ISheetMappings<VoucherExportDto>
{
    public Func<object, VoucherExportDto> GetRowMapper()
        => _ => throw new NotSupportedException("Import is not supported for VoucherExportDto.");

    public Action<object, IEnumerable<VoucherExportDto>> Exporter => (wsObj, vouchers) =>
    {
        var ws = (IXLWorksheet)wsObj;

        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 2).Value = "ID";
        ws.Cell(1, 3).Value = "Mã voucher";
        ws.Cell(1, 4).Value = "Loại";
        ws.Cell(1, 5).Value = "Giá trị";
        ws.Cell(1, 6).Value = "Đã dùng";
        ws.Cell(1, 7).Value = "Ngày bắt đầu";
        ws.Cell(1, 8).Value = "Ngày kết thúc";
        ws.Cell(1, 9).Value = "Trạng thái";
        ws.Cell(1, 10).Value = "Ngày tạo";
        ws.Cell(1, 11).Value = "Người tạo";

        int row = 2;
        foreach (var voucher in vouchers)
        {
            ws.Cell(row, 1).Value = voucher.Index;
            ws.Cell(row, 2).Value = voucher.Id.ToString();
            ws.Cell(row, 3).Value = voucher.CouponCode;
            ws.Cell(row, 4).Value = voucher.Type;
            ws.Cell(row, 5).Value = voucher.Value;
            ws.Cell(row, 6).Value = voucher.Used;
            ws.Cell(row, 7).Value = voucher.StartDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 8).Value = voucher.EndDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 9).Value = voucher.IsActive;
            ws.Cell(row, 10).Value = voucher.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 11).Value = voucher.CreatedBy;
            row++;
        }

        ws.Columns().AdjustToContents();
    };
}
