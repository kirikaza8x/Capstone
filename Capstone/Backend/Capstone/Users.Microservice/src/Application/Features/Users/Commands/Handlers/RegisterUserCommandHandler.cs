// using AutoMapper;
// using ClothingStore.Application.Features.User.Dtos;
// using FluentValidation;
// using Shared.Application.Abstractions.Authentication;
// using Shared.Application.Abstractions.Messaging;
// using Shared.Application.Common.ResponseModel;
// using Users.Domain.Repositories;

// namespace ClothingStore.Application.Features.User.Commands.RegisterUser
// {
//     public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
//     {
//         public RegisterUserCommandValidator()
//         {
//             // RuleFor(x => x.RegisterRequest.Email)
//             //     // .NotEmpty().WithMessage("Email is required.")

//             RuleFor(x => x.RegisterRequest.UserName)
//                 .NotEmpty().WithMessage("Username is required.")
//                 .MaximumLength(50).WithMessage("Username must be at most 50 characters long.");

//             RuleFor(x => x.RegisterRequest.Password)
//                 .NotEmpty().WithMessage("Password is required.")
//                 .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
//         }
//     }

//     public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserResponseDto>
//     {
//         private readonly IUserRepository _userRepository;
//         private readonly IPasswordHasher _passwordHasher;
//         private readonly IMapper _mapper;

//         public RegisterUserCommandHandler(
//             IUserRepository userRepository,
//             IPasswordHasher passwordHasher,
//             IMapper mapper)
//         {
//             _userRepository = userRepository;
//             _passwordHasher = passwordHasher;
//             _mapper = mapper;
//         }

//         public async Task<Result<UserResponseDto>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
//         {
//             var request = command.RegisterRequest;

//             var existingUser = await _userRepository.GetUserByMailOrUserName(request.UserName, cancellationToken);
//             if (existingUser != null)
//             {
//                 return Result.Failure<UserResponseDto>(
//                     new Error("UserExist", "User already exists with the same email or username."));
//             }
//             var user = _mapper.Map<User>(request);
//             user.ChangePassword(_passwordHasher.HashPassword(request.Password));

//             await _userRepository.AddAsync(user, cancellationToken);

//             var dto = _mapper.Map<UserResponseDto>(user);
//             return Result.Success(dto);
//         }
//     }
// }
