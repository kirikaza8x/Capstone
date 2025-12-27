using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Commands.Login
{
    public record LoginUserCommand(LoginRequestDto LoginRequest) : ICommand<LoginResponseDto>, ITransactionalCommand;
}
