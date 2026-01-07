using Shared.Application.DTOs;
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Commands.Login
{
    public record RefreshTokenCommand(RefreshTokenRequestDto Request) : ICommand<LoginResponseDto>, ITransactionalCommand;

}
