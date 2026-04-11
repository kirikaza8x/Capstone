using AutoMapper;
using MediatR;
using Payments.Application.DTOs.Payment;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using System.Linq.Expressions;
using Users.PublicApi.PublicApi;

namespace Payments.Application.Features.Payments.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryHandler
    : IQueryHandler<GetTransactionByIdQuery, PaymentTransactionDetailDto>
{
    private readonly IPaymentTransactionRepository _transactionRepository;
    private readonly IUserPublicApi _userPublicApi;
    private readonly IMapper _mapper;

    public GetTransactionByIdQueryHandler(
        IPaymentTransactionRepository transactionRepository,
        IUserPublicApi userPublicApi,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _userPublicApi = userPublicApi;
        _mapper = mapper;
    }

    public async Task<Result<PaymentTransactionDetailDto>> Handle(
    GetTransactionByIdQuery query,
    CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(
            query.Id,
            includes: new Expression<Func<PaymentTransaction, object>>[]
            {
            t => t.Items
            },
            cancellationToken: cancellationToken);

        if (transaction is null)
            return Result.Failure<PaymentTransactionDetailDto>(
            Error.NotFound("Transaction.NotFound", $"Transaction {query.Id} not found")
        );

        var userMap = await _userPublicApi.GetUserMapByIdsAsync(
            new[] { transaction.UserId },
            cancellationToken
        );

        var dto = _mapper.Map<PaymentTransactionDetailDto>(
        transaction,
        opt => opt.Items["userMap"] = userMap
        );

        return Result.Success(dto);
    }

}
