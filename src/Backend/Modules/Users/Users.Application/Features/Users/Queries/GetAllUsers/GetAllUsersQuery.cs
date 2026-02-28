using Shared.Application.Messaging;
using Users.Application.Features.Users.Dtos;

namespace Users.Application.Features.Users.Queries
{
    public record GetAllUsersQuery() : IQuery<IEnumerable<UserResponseDto>>;
}
