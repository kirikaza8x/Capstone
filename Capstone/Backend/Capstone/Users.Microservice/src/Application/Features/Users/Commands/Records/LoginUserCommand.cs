using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;
using Users.Application.Features.User.Dtos;

namespace Users.Application.Features.User.Commands.Login
{
    public record LoginUserCommand(LoginRequestDto LoginRequest) : ICommand<LoginResponseDto>;
}
