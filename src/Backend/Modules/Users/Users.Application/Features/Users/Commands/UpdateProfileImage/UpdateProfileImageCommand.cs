using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;

namespace Users.Application.Features.Users.Commands.Records
{
    public record UpdateProfileImageCommand(
        IFileUpload File,
        Guid UserId
    ) : ICommand<Guid>;
}
