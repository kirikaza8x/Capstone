using Shared.Application.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Commands.RegisterUser
{
    // public record RegisterUserCommand(
    //     string Email,
    //     string UserName,
    //     string Password
    // ) : IRequest<UserResponse>;

    public record RegisterUserCommand(RegisterRequestDto RegisterRequest) : ICommand<UserResponseDto>, ITransactionalCommand;

}
