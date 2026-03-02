using System.Linq.Expressions;
using AutoMapper;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Shared.Domain.Pagination;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Entities;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries
{
    public sealed class GetUsersQueryHandler
        : IQueryHandler<GetUsersQuery, PagedResult<UserProfileDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<UserProfileDto>>> Handle(
            GetUsersQuery query,
            CancellationToken cancellationToken)
        {
            var pagedResult = await _userRepository.GetPagedAsync(
                query,
                selector: u => _mapper.Map<UserProfileDto>(u),
                predicate: u =>
                    (string.IsNullOrWhiteSpace(query.Email) ||
                     (u.Email != null && u.Email.Contains(query.Email))) &&
                    (string.IsNullOrWhiteSpace(query.UserName) ||
                     (u.UserName != null && u.UserName.Contains(query.UserName))) &&
                    (string.IsNullOrWhiteSpace(query.FirstName) ||
                     (u.FirstName != null && u.FirstName.Contains(query.FirstName))) &&
                    (string.IsNullOrWhiteSpace(query.LastName) ||
                     (u.LastName != null && u.LastName.Contains(query.LastName))) &&
                    ((!query.BirthdayFrom.HasValue && !query.BirthdayTo.HasValue) ||
                        u.Birthday == null ||  // ← Let null birthdays pass through
                    (
                        (!query.BirthdayFrom.HasValue || u.Birthday >= query.BirthdayFrom.Value) &&
                        (!query.BirthdayTo.HasValue || u.Birthday <= query.BirthdayTo.Value)
                    )) &&
                    (!query.Gender.HasValue || u.Gender == query.Gender.Value) &&
                    (string.IsNullOrWhiteSpace(query.PhoneNumber) ||
                     (u.PhoneNumber != null && u.PhoneNumber.Contains(query.PhoneNumber))) &&
                    (!query.Status.HasValue || u.Status == query.Status.Value) &&
                    (string.IsNullOrWhiteSpace(query.SearchTerm) ||
                     (u.Email != null && u.Email.Contains(query.SearchTerm)) ||
                     (u.UserName != null && u.UserName.Contains(query.SearchTerm)) ||
                     (u.FirstName != null && u.FirstName.Contains(query.SearchTerm)) ||
                     (u.LastName != null && u.LastName.Contains(query.SearchTerm))),
                includes: new Expression<Func<User, object>>[]
                    {
                    u => u.Roles, 
                    // u => u.RefreshTokens, 
                    // u => u.ExternalIdentities 
                    },
                cancellationToken: cancellationToken);

            return Result.Success(pagedResult);
        }
    }
}
