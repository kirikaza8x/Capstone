using AutoMapper;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Common.ResponseModel;
using Users.Application.Features.Users.Dtos;
using Users.Application.Features.Users.Queries;
using Users.Domain.Repositories;

namespace ClothingStore.Application.Features.Users.Queries
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
            return Result.Failure<UserResponseDto>(new Error("UserNotFound", "User not found"));

        return Result.Success(_mapper.Map<UserResponseDto>(user));
    }
}
}
