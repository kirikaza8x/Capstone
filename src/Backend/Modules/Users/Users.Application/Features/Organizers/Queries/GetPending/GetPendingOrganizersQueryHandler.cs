using System.Linq.Expressions;
using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Entities;
using Users.Domain.Enums;

public class GetPendingOrganizersQueryHandler
    : IQueryHandler<GetPendingOrganizersQuery, PagedResult<OrganizerAdminListItemDto>>
{
    private readonly IOrganizerProfileRepository _repository;
    private readonly IMapper _mapper;

    public GetPendingOrganizersQueryHandler(
        IOrganizerProfileRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<OrganizerAdminListItemDto>>> Handle(
        GetPendingOrganizersQuery query,
        CancellationToken cancellationToken)
    {
        var predicate = BuildPredicate(query);

        var result = await _repository.GetAllWithPagingAsync(
            query,
            predicate,
            cancellationToken: cancellationToken);

        var mapped = new PagedResult<OrganizerAdminListItemDto>(
            (result.Items ?? new List<OrganizerProfile>())
                .Select(_mapper.Map<OrganizerAdminListItemDto>)
                .ToList(),
            result.TotalCount,
            result.PageNumber,
            result.PageSize);

        return Result.Success(mapped);
    }

    private static Expression<Func<OrganizerProfile, bool>> BuildPredicate(
    GetPendingOrganizersQuery query)
    {
        return x =>
            x.Status == OrganizerStatus.Pending &&
            (string.IsNullOrEmpty(query.Keyword) ||
             (!string.IsNullOrEmpty(x.DisplayName) && x.DisplayName.Contains(query.Keyword))) &&
            (!query.BusinessType.HasValue || x.BusinessType == query.BusinessType);
    }

}