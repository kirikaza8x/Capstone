using ClosedXML.Excel;
using Shared.Application.Abstractions.Report;
using Ticketing.Application.Orders.Queries.ExportOrdersSheet;

namespace Ticketing.Infrastructure.Services.Reports;

public class OrderSheetMappings : ISheetMappings<OrderExportDto>
{
    public Func<object, OrderExportDto> GetRowMapper()
    {
        return _ => throw new NotSupportedException("Import is not supported for OrderExportDto.");
    }

    public Action<object, IEnumerable<OrderExportDto>> Exporter => (wsObj, orders) =>
    {
        var ws = (IXLWorksheet)wsObj;

        // Header
        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 2).Value = "Order ID";
        ws.Cell(1, 3).Value = "Tên người mua";
        ws.Cell(1, 4).Value = "Email người mua";
        ws.Cell(1, 5).Value = "Tổng tiền";
        ws.Cell(1, 6).Value = "Mã voucher";
        ws.Cell(1, 7).Value = "Loại voucher";
        ws.Cell(1, 8).Value = "Giảm giá";
        ws.Cell(1, 9).Value = "Thực thu";
        ws.Cell(1, 10).Value = "Trạng thái";
        ws.Cell(1, 11).Value = "Ngày tạo";
        ws.Cell(1, 12).Value = "Người tạo";

        int row = 2;
        foreach (var order in orders)
        {
            ws.Cell(row, 1).Value = order.Index;
            ws.Cell(row, 2).Value = order.OrderId.ToString();
            ws.Cell(row, 3).Value = order.BuyerName;
            ws.Cell(row, 4).Value = order.BuyerEmail;
            ws.Cell(row, 5).Value = order.TotalPrice;
            ws.Cell(row, 6).Value = order.CouponCode;
            ws.Cell(row, 7).Value = order.VoucherType;
            ws.Cell(row, 8).Value = order.DiscountAmount;
            ws.Cell(row, 9).Value = order.FinalPrice;
            ws.Cell(row, 10).Value = order.Status;
            ws.Cell(row, 11).Value = order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 12).Value = order.CreatedBy;
            row++;
        }
    };
}
