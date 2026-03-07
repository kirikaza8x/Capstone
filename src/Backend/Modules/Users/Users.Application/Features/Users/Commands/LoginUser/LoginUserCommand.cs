using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Commands.Records
{
    public record LoginUserCommand(
    string EmailOrUserName,
    string Password,
    string? DeviceName,
    string? IpAddress,
    string? UserAgent
) : ICommand<LoginResponseDto>;

}
