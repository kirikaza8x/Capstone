using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.User.Dtos;

namespace Users.Application.Features.User.Commands.RegisterUser
{
    // public record RegisterUserCommand(
    //     string Email,
    //     string UserName,
    //     string Password
    // ) : IRequest<UserResponse>;

    public record RegisterUserCommand(RegisterRequestDto RegisterRequest) : ICommand<UserResponseDto>;

}
