using Shared.Application.Abstractions.Storage;
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Commands.Records
{
    public record UpdateProfileImageCommand(
        IFileUpload File,
        Guid UserId
    ) : ICommand<Guid>;
}
