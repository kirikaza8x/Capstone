using Shared.Application.DTOs;
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Queries
{
    public record GetCurrentUserQuery() : IQuery<CurrentUserDto>;
}
