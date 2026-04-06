using AutoMapper;
using MediatR;
using Payments.Application.DTOs.Payment;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using System.Linq.Expressions;
using Users.PublicApi.PublicApi;

namespace Payments.Application.Features.Payments.Queries.GetAdminTransactions;

public sealed class GetAdminTransactionsQueryHandler
    : IQueryHandler<GetAdminTransactionsQuery, PagedResult<PaymentTransactionDto>>
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly IUserPublicApi _userPublicApi;
    private readonly IMapper _mapper;

    public GetAdminTransactionsQueryHandler(
        IPaymentTransactionRepository transactionRepository,
        IUserPublicApi userPublicApi,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _userPublicApi = userPublicApi;
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
            // includes: new Expression<Func<PaymentTransaction, object>>[]
            // {
            //     t => t.Items
            // },
            cancellationToken: cancellationToken);

        var ids = pagedResult.Items.Select(d => d.UserId)
            .Distinct()
            .ToList();

        var userMap = await _userPublicApi.GetUserMapByIdsAsync(ids, cancellationToken);
        var dtoItems = _mapper.Map<List<PaymentTransactionDto>>(
        pagedResult.Items,
        opt => opt.Items["userMap"] = userMap
        );
        // var dtoItems = _mapper.Map<List<PaymentTransactionDto>>(pagedResult.Items);



        var dtoPagedResult = new PagedResult<PaymentTransactionDto>(
            dtoItems,
            pagedResult.PageNumber,
            pagedResult.PageSize,
            pagedResult.TotalCount
        );

        return Result.Success(dtoPagedResult);
    }
}
