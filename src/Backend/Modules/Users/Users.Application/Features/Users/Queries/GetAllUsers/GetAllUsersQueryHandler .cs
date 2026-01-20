using AutoMapper;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Users.Queries;
using Users.Domain.Repositories;

namespace ClothingStore.Application.Features.Users.Queries
{
    public class GetAllUsersQueryHandler 
        : IQueryHandler<GetAllUsersQuery, IEnumerable<UserResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<UserResponseDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            var dtos = users.Select(user => _mapper.Map<UserResponseDto>(user));
            return Result.Success(dtos);
        }
    }
}
