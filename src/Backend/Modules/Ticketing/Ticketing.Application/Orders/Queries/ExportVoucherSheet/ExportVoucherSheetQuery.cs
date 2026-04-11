using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.ExportVoucherSheet;
public record ExportVoucherSheetQuery(Guid EventId) : IQuery<byte[]>;
