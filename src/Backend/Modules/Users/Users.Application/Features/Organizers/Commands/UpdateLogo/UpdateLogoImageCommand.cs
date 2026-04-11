using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;

namespace Users.Application.Features.Organizers.Commands.UpdateLogo
{
    public record UpdateLogoImageCommand(
                Guid UserId,
        IFileUpload File
    ) : ICommand<Guid>;
}
