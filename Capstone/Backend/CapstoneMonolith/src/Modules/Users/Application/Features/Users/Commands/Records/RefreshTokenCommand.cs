using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;

namespace Users.Application.Features.Users.Commands.Login
{
    public record RefreshTokenCommand(RefreshTokenRequestDto Request) : ICommand<LoginResponseDto>, ITransactionalCommand;

}
