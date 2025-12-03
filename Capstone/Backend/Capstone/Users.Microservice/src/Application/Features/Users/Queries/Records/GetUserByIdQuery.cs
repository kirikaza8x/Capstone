using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Queries
{
public record GetUserByIdQuery(Guid Id) : IQuery<UserResponseDto>;
}
