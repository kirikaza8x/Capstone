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

        ws.Cell(1, 1).Value = "STT";
        ws.Cell(1, 2).Value = "Order ID";
        ws.Cell(1, 3).Value = "Ngày tạo";
        ws.Cell(1, 4).Value = "Trạng thái";

        ws.Cell(1, 5).Value = "Tên người mua";
        ws.Cell(1, 6).Value = "Email người mua";

        ws.Cell(1, 7).Value = "Giá gốc";
        ws.Cell(1, 8).Value = "Giảm giá";
        ws.Cell(1, 9).Value = "Thực thu";
        ws.Cell(1, 10).Value = "Mã giảm giá";

        ws.Cell(1, 11).Value = "Tên sự kiện";
        ws.Cell(1, 12).Value = "Địa điểm";
        ws.Cell(1, 13).Value = "Thời gian bắt đầu sự kiện";

        ws.Cell(1, 14).Value = "Ticket ID";
        ws.Cell(1, 15).Value = "Loại vé";
        ws.Cell(1, 16).Value = "Giá vé";
        ws.Cell(1, 17).Value = "Trạng thái vé";
        ws.Cell(1, 18).Value = "Session";
        ws.Cell(1, 19).Value = "Thời gian bắt đầu session";
        ws.Cell(1, 20).Value = "Ghế";

        int row = 2;
        foreach (var order in orders)
        {
            ws.Cell(row, 1).Value = order.Index;
            ws.Cell(row, 2).Value = order.OrderId.ToString();
            ws.Cell(row, 3).Value = order.CreatedAt == DateTime.MinValue ? string.Empty : order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 4).Value = order.Status;

            ws.Cell(row, 5).Value = order.BuyerName;
            ws.Cell(row, 6).Value = order.BuyerEmail;

            ws.Cell(row, 7).Value = order.OriginalPrice;
            ws.Cell(row, 8).Value = order.DiscountAmount;
            ws.Cell(row, 9).Value = order.FinalPrice;
            ws.Cell(row, 10).Value = order.CouponCode;

            ws.Cell(row, 11).Value = order.EventName;
            ws.Cell(row, 12).Value = order.Location;
            ws.Cell(row, 13).Value = order.EventStartAt == DateTime.MinValue ? string.Empty : order.EventStartAt.ToString("yyyy-MM-dd HH:mm:ss");

            ws.Cell(row, 14).Value = order.TicketId?.ToString();
            ws.Cell(row, 15).Value = order.TicketType;
            ws.Cell(row, 16).Value = order.TicketPrice;
            ws.Cell(row, 17).Value = order.TicketStatus;
            ws.Cell(row, 18).Value = order.SessionTitle;
            ws.Cell(row, 19).Value = order.SessionStartTime?.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 20).Value = order.SeatCode;

            row++;
        }

        ws.Columns().AdjustToContents();
    };
}
