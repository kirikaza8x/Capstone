// Payments.Application/Features/Payments/Queries/GetAdminTransactions/GetAdminTransactionsQueryHandler.cs

using AutoMapper;
using Payments.Application.DTOs.Payment;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using System.Linq.Expressions;

namespace Payments.Application.Features.Payments.Queries.GetAdminTransactions;

public sealed class GetAdminTransactionsQueryHandler
    : IQueryHandler<GetAdminTransactionsQuery, PagedResult<PaymentTransactionDto>>
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    public GetAdminTransactionsQueryHandler(
        IPaymentTransactionRepository transactionRepository,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PaymentTransactionDto>>> Handle(
    GetAdminTransactionsQuery query,
    CancellationToken cancellationToken)
    {
        var pagedResult = await _transactionRepository.GetAllWithPagingAsync(
            query,
            predicate: t =>
                (query.UserId == null || t.UserId == query.UserId)
                && (query.OrderId == null || t.OrderId == query.OrderId)
                && (query.EventId == null || t.EventId == query.EventId)
                && (query.Type == null || t.Type == query.Type)
                && (query.Status == null || t.InternalStatus == query.Status)
                && (query.AmountMin == null || t.Amount >= query.AmountMin)
                && (query.AmountMax == null || t.Amount <= query.AmountMax)
                && (query.CreatedFrom == null || t.CreatedAt >= query.CreatedFrom)
                && (query.CreatedTo == null || t.CreatedAt <= query.CreatedTo)
                && (string.IsNullOrWhiteSpace(query.GatewayTxnRef) ||
                    (t.GatewayTxnRef != null && t.GatewayTxnRef.Contains(query.GatewayTxnRef))),
            includes: new Expression<Func<PaymentTransaction, object>>[]
            {
            t => t.Items
            },
            cancellationToken: cancellationToken);

        var dtoPagedResult = new PagedResult<PaymentTransactionDto>(
            _mapper.Map<List<PaymentTransactionDto>>(pagedResult.Items), 
            pagedResult.PageNumber,                                     
            pagedResult.PageSize,                                       
            pagedResult.TotalCount                                      
        );


        return Result.Success(dtoPagedResult);
    }

}