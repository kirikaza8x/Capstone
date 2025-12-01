using Shared.Application.Abstractions.Messaging;
using Users.Application.Features.User.Dtos;

namespace Users.Application.Features.User.Queries
{
    public record GetAllUsersQuery() : IQuery<IEnumerable<UserResponseDto>>;
}
