using Shared.Application.Messaging;
using Users.Application.Features.Users.Dtos;
using Users.Domain.Enums;

namespace Users.Application.Features.Users.Commands.Records
{
    public record UpdateProfileCommand(
        Guid UserId,
        string? FirstName,
        string? LastName,
        DateTime? Birthday,
        Gender? Gender,
        string? Phone,
        string? Address,
        string? Description,
        string? SocialLink,
        string? ProfileImageUrl
    ) : ICommand<UserProfileDto>;
}
