// using AutoMapper;
// using ClothingStore.Application.Features.User.Dtos;
// using ClothingStore.Domain.Repositories;
// using MediatR;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Domain.Common.ResponseModel;

// namespace ClothingStore.Application.Features.User.Queries
// {
//     public class GetUserByIdQueryHandler 
//     : IQueryHandler<GetUserByIdQuery, UserResponseDto>
// {
//     private readonly IUserRepository _userRepository;
//     private readonly IMapper _mapper;

//     public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
//     {
//         _userRepository = userRepository;
//         _mapper = mapper;
//     }

//     public async Task<Result<UserResponseDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
//     {
//         var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
//         if (user is null)
//             return Result.Failure<UserResponseDto>(new Error("UserNotFound", "User not found"));

//         return Result.Success(_mapper.Map<UserResponseDto>(user));
//     }
// }
// }
