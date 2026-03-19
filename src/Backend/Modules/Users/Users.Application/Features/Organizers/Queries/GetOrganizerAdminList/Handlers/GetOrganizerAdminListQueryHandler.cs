using System.Linq.Expressions;
using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Users.Application.Features.Organizers.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Organizers.Queries
{
    public sealed class GetOrganizerAdminListQueryHandler
        : IQueryHandler<GetOrganizerAdminListQuery, PagedResult<OrganizerAdminListItemDto>>
    {
        private readonly IOrganizerProfileRepository _repository;
        private readonly IMapper _mapper;

        public GetOrganizerAdminListQueryHandler(
            IOrganizerProfileRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<OrganizerAdminListItemDto>>> Handle(
            GetOrganizerAdminListQuery query,
            CancellationToken cancellationToken)
        {
            var list = await _repository.GetAllWithPagingAsync(
                query,
                predicate: p =>
                    (!query.Status.HasValue || p.Status == query.Status.Value) &&
                    (!query.BusinessType.HasValue || p.BusinessType == query.BusinessType.Value) &&
                    (string.IsNullOrWhiteSpace(query.Search) ||
                        (p.DisplayName != null && p.DisplayName.Contains(query.Search))),
                includes: new Expression<Func<OrganizerProfile, object>>[]
                {
                    p => p.User
                },
                cancellationToken: cancellationToken);

            var dtoItems = list.Items
                .Select(p => _mapper.Map<OrganizerAdminListItemDto>(p))
                .ToList();

            var pagedResult = new PagedResult<OrganizerAdminListItemDto>(
                dtoItems,
                list.TotalCount,
                list.PageNumber,
                list.PageSize);

            return Result.Success(pagedResult);
        }
    }
}
