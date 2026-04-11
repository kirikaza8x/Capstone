using System.Linq.Expressions;
using AutoMapper;
using Payments.Application.Features.WithdrawalRequests.Dtos;
using Payments.Application.Features.WithdrawalRequests.Queries;
using Payments.Domain.Entities;
using Payments.Domain.Repositories;
using Shared.Application.Abstractions.Authentication;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;

namespace Payments.Application.Features.WithdrawalRequests.Handlers;

public class GetMyWithdrawalRequestsQueryHandler
    : IQueryHandler<GetMyWithdrawalRequestsQuery, PagedResult<WithdrawalRequestListItemDto>>
{
    private readonly IWithdrawalRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyWithdrawalRequestsQueryHandler(
        IWithdrawalRequestRepository repository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _repository         = repository;
        _currentUserService = currentUserService;
        _mapper             = mapper;
    }

    public async Task<Result<PagedResult<WithdrawalRequestListItemDto>>> Handle(
        GetMyWithdrawalRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var predicate = BuildPredicate(query, userId);

        var result = await _repository.GetAllWithPagingAsync(
            query,
            predicate,
            cancellationToken: cancellationToken);

        var mapped = new PagedResult<WithdrawalRequestListItemDto>(
            (result.Items ?? [])
                .Select(_mapper.Map<WithdrawalRequestListItemDto>)
                .ToList(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount);

        return Result.Success(mapped);
    }

    private static Expression<Func<WithdrawalRequest, bool>> BuildPredicate(
        GetMyWithdrawalRequestsQuery query,
        Guid userId)
    {
        return x =>
            x.UserId == userId &&
            (!query.Status.HasValue || x.Status == query.Status);
    }
}

public class GetMyWithdrawalRequestDetailQueryHandler
    : IQueryHandler<GetMyWithdrawalRequestDetailQuery, WithdrawalRequestDetailDto>
{
    private readonly IWithdrawalRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyWithdrawalRequestDetailQueryHandler(
        IWithdrawalRequestRepository repository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _repository         = repository;
        _currentUserService = currentUserService;
        _mapper             = mapper;
    }

    public async Task<Result<WithdrawalRequestDetailDto>> Handle(
        GetMyWithdrawalRequestDetailQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var request = await _repository.GetByIdAsync(query.RequestId, cancellationToken);

        if (request == null)
            return Result.Failure<WithdrawalRequestDetailDto>(
                Error.NotFound("WithdrawalRequest.NotFound", "Withdrawal request not found."));

        if (request.UserId != userId)
            return Result.Failure<WithdrawalRequestDetailDto>(
                Error.Forbidden(
                    "WithdrawalRequest.Forbidden",
                    "You are not allowed to view this request."));

        return Result.Success(_mapper.Map<WithdrawalRequestDetailDto>(request));
    }
}
