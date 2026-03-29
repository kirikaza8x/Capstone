using Shared.Application.Abstractions.Messaging;

public record GetTotalRefundsByEventQuery(Guid EventId) : IQuery<decimal>;
