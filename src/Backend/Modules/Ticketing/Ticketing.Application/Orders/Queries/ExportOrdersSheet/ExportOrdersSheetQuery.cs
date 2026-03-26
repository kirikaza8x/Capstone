using Shared.Application.Abstractions.Messaging;

namespace Ticketing.Application.Orders.Queries.ExportOrdersSheet;

public record ExportOrdersSheetQuery(Guid EventId) : IQuery<byte[]>;
