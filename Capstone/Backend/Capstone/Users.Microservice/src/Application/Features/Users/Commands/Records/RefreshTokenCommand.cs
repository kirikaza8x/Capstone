using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;

namespace Users.Application.Features.User.Commands.Login
{
    public record RefreshTokenCommand(RefreshTokenRequestDto Request) : ICommand<LoginResponseDto>;

}
