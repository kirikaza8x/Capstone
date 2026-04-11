using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<GetPendingOrganizersQueryHandler> _logger;

    public GetPendingOrganizersQueryHandler(
        IOrganizerProfileRepository repository,
        IMapper mapper,
        ILogger<GetPendingOrganizersQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<OrganizerAdminListItemDto>>> Handle(
        GetPendingOrganizersQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GetPendingOrganizersQuery with keyword '{Keyword}', businessType '{BusinessType}', page {PageNumber}, size {PageSize}",
            query.Keyword, query.BusinessType, query.PageNumber, query.PageSize);

        var predicate = BuildPredicate(query);

        var result = await _repository.GetAllWithPagingAsync(
            query,
            predicate,
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Repository returned {Count} items out of total {TotalCount}",
            result.Items?.Count ?? 0, result.TotalCount);

        var mappedItems = (result.Items ?? new List<OrganizerProfile>())
            .Select(_mapper.Map<OrganizerAdminListItemDto>)
            .ToList();

        var mapped = PagedResult<OrganizerAdminListItemDto>.Create(
            mappedItems,
            result.PageNumber,   
            result.PageSize,     
            result.TotalCount);  

        _logger.LogInformation(
            "Returning {MappedCount} mapped items, page {PageNumber}/{TotalPages}, totalCount {TotalCount}",
            mapped.Items.Count, mapped.PageNumber, mapped.TotalPages, mapped.TotalCount);

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