using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Storage;

namespace Users.Application.Features.Users.Commands.Import.Records;

public record ImportSheetUsersCommand(
    IFileUpload File
) : ICommand<int>;
