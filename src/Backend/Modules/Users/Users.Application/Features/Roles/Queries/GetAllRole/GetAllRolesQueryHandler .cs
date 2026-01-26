// using System.Linq.Expressions;
// using AutoMapper;
// using Users.Application.Features.Roles.Dtos;
// using Users.Domain.Entities;
// using Users.Domain.Repositories;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Application.Common.ResponseModel.Pagination;
// using Shared.Application.Common.ResponseModel;
// using Shared.Application.Helpers;
// using Users.Application.Features.Roles.Queries;

// namespace ClothingStore.Application.Features.Roles.Queries
// {
//     public class GetRoleListQueryHandler
//         : IQueryHandler<GetRoleListQuery, PaginatedResult<RoleResponseDto>>
//     {
//         private readonly IRoleRepository _roleRepository;
//         private readonly IMapper _mapper;

//         public GetRoleListQueryHandler(IRoleRepository roleRepository, IMapper mapper)
//         {
//             _roleRepository = roleRepository;
//             _mapper = mapper;
//         }

//         public async Task<Result<PaginatedResult<RoleResponseDto>>> Handle(GetRoleListQuery request, CancellationToken cancellationToken)
//         {
//             var filter = request.Filter;
//             var pageIndex = filter.PageIndex;
//             var pageSize = filter.PageSize;

//             // Build order by expression dynamically
//             var orderByColumn = ExpressionBuilder.BuildOrderByExpression<Role>(
//                 string.IsNullOrWhiteSpace(filter.OrderBy) ? nameof(Role.Name) : filter.OrderBy
//             );
//             var isAscending = filter.IsAscending;

//             // Start with a base predicate
//             Expression<Func<Role, bool>> predicate = e => true;

//             // Exact match filters
//             if (!string.IsNullOrWhiteSpace(filter.Name))
//             {
//                 var name = filter.Name.ToLower();
//                 predicate = predicate.And(e => e.Name.ToLower() == name);
//             }

//             if (!string.IsNullOrWhiteSpace(filter.Description))
//             {
//                 var description = filter.Description.ToLower();
//                 predicate = predicate.And(e => e.Description != null && e.Description.ToLower() == description);
//             }

//             // Search term filter (case-insensitive partial match)
//             if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
//             {
//                 var term = filter.SearchTerm.ToLower();
//                 predicate = predicate.And(e =>
//                     e.Name.ToLower().Contains(term) ||
//                     (e.Description != null && e.Description.ToLower().Contains(term))
//                 );
//             }

//             // Execute paged query
//             var (items, totalCount) = await _roleRepository.GetPagedAsync(
//                 pageIndex,
//                 pageSize,
//                 predicate,
//                 orderByColumn,
//                 isAscending,
//                 cancellationToken
//             );

//             var paginatedResult = new PaginatedResult<RoleResponseDto>(
//                 pageIndex: pageIndex,
//                 pageSize: pageSize,
//                 count: totalCount,
//                 data: _mapper.Map<IEnumerable<RoleResponseDto>>(items)
//             );

//             return Result.Success(paginatedResult);
//         }
//     }
// }
