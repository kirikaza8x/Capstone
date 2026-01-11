using Shared.Application.DTOs;
using Shared.Application.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Commands.Records
{
    public record LoginUserCommand(LoginRequestDto LoginRequest) : ICommand<LoginResponseDto>, ITransactionalCommand;
}
