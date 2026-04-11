using Payment.Domain.Enums;
using Payments.Application.DTOs.Payment;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Pagination;
using Shared.Domain.Queries;

namespace Payments.Application.Features.Payments.Queries.GetAdminTransactions;

public sealed record GetAdminTransactionsQuery : PagedQuery, IQuery<PagedResult<PaymentTransactionDto>>
{
    public Guid? UserId { get; init; }
    public Guid? OrderId { get; init; }
    public Guid? EventId { get; init; }
    public PaymentType? Type { get; init; }
    public PaymentReferenceType? ReferenceType { get; init; }
    public PaymentInternalStatus? Status { get; init; }
    public decimal? AmountMin { get; init; }
    public decimal? AmountMax { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public string? GatewayTxnRef { get; init; }
}
