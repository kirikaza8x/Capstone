
using Shared.Application.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Commands.Records;
public record GoogleLoginCommand(string IdToken) : ICommand<LoginResponseDto>;
