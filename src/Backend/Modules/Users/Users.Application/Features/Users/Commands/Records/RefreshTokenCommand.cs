using Shared.Application.DTOs;
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Commands.Records
{
    public record RefreshTokenCommand(RefreshTokenRequestDto Request) : ICommand<LoginResponseDto>, ITransactionalCommand;

}
