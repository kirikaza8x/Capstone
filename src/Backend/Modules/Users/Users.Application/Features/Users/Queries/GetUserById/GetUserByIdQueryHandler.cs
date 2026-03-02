using AutoMapper;
using Shared.Application.Messaging;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Repositories;

namespace Users.Application.Features.Users.Queries
{
    public class GetUserByIdQueryHandler
        : IQueryHandler<GetUserByIdQuery, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<Result<UserResponseDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

            if (user is null)
            {
                return Result.Failure<UserResponseDto>(
                    Error.NotFound("User.NotFound", "User not found.")
                );
            }

            var dto = _mapper.Map<UserResponseDto>(user);
            return Result.Success(dto);
        }
    }
}
