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
        : IQueryHandler<GetUsersQuery, PagedResult<UserResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedResult<UserResponseDto>>> Handle(
            GetUsersQuery query,
            CancellationToken cancellationToken)
        {
            var pagedResult = await _userRepository.GetPagedAsync(
                query,
                selector: u => _mapper.Map<UserResponseDto>(u),
                predicate: u =>
                    (string.IsNullOrEmpty(query.Email) || u.Email!.Contains(query.Email)) &&
                    (string.IsNullOrEmpty(query.UserName) || u.UserName.Contains(query.UserName)) &&
                    (string.IsNullOrEmpty(query.FirstName) || u.FirstName!.Contains(query.FirstName)) &&
                    (string.IsNullOrEmpty(query.LastName) || u.LastName!.Contains(query.LastName)) &&
                    (!query.BirthdayFrom.HasValue || u.Birthday >= query.BirthdayFrom.Value) &&
                    (!query.BirthdayTo.HasValue || u.Birthday <= query.BirthdayTo.Value) &&
                    (!query.Gender.HasValue || u.Gender == query.Gender.Value) &&
                    (string.IsNullOrEmpty(query.PhoneNumber) || u.PhoneNumber!.Contains(query.PhoneNumber)) &&
                    (!query.Status.HasValue || u.Status == query.Status.Value),
                cancellationToken: cancellationToken);

            return Result.Success(pagedResult);
        }
    }
}
