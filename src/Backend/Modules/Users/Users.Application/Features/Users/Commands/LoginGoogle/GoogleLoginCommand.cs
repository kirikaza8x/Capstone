using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Commands.LoginGoogle;

public record GoogleLoginCommand(
    string IdToken,
    string? DeviceName = null
) : ICommand<LoginResponseDto>;
