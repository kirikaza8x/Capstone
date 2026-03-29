using System.Linq.Expressions;
using AutoMapper;
using Payments.Application.Features.WithdrawalRequests.Dtos;
using Payments.Application.Features.WithdrawalRequests.Queries;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class GetAllWithdrawalRequestsQueryHandler
    : IQueryHandler<GetAllWithdrawalRequestsQuery, PagedResult<WithdrawalRequestAdminListItemDto>>
{
    private readonly IWithdrawalRequestRepository _repository;
    private readonly IMapper _mapper;

    public GetAllWithdrawalRequestsQueryHandler(
        IWithdrawalRequestRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<WithdrawalRequestAdminListItemDto>>> Handle(
        GetAllWithdrawalRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var predicate = BuildPredicate(query);

        var result = await _repository.GetAllWithPagingAsync(
            query,
            predicate,
            cancellationToken: cancellationToken);

        var mapped = new PagedResult<WithdrawalRequestAdminListItemDto>(
            (result.Items ?? [])
                .Select(_mapper.Map<WithdrawalRequestAdminListItemDto>)
                .ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);

        return Result.Success(mapped);
    }

    private static Expression<Func<WithdrawalRequest, bool>> BuildPredicate(
        GetAllWithdrawalRequestsQuery query)
    {
        return x =>
            (!query.UserId.HasValue || x.UserId == query.UserId) &&
            (!query.Status.HasValue || x.Status == query.Status) &&
            (!query.CreatedFrom.HasValue || x.CreatedAt >= query.CreatedFrom) &&
            (!query.CreatedTo.HasValue || x.CreatedAt <= query.CreatedTo);
    }
}

public class GetWithdrawalRequestDetailQueryHandler
    : IQueryHandler<GetWithdrawalRequestDetailQuery, WithdrawalRequestAdminDetailDto>
{
    private readonly IWithdrawalRequestRepository _repository;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestDetailQueryHandler(
        IWithdrawalRequestRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<WithdrawalRequestAdminDetailDto>> Handle(
        GetWithdrawalRequestDetailQuery query,
        CancellationToken cancellationToken)
    {
        var request = await _repository.GetByIdAsync(query.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure<WithdrawalRequestAdminDetailDto>(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        return Result.Success(_mapper.Map<WithdrawalRequestAdminDetailDto>(request));
    }
}