using Shared.Application.Abstractions.Messaging;
using Shared.Application.DTOs;

namespace Users.Application.Features.Users.Queries
{
    public record GetCurrentUserQuery() : IQuery<CurrentUserDto>;
}
