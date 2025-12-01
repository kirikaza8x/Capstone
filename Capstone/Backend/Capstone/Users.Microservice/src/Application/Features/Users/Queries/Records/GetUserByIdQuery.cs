using MediatR;
using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.User.Dtos;

namespace Users.Application.Features.User.Queries
{
public record GetUserByIdQuery(Guid Id) : IQuery<UserResponseDto>;
}
