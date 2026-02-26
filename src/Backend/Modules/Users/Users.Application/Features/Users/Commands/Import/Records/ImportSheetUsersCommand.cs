using Shared.Application.Abstractions.Storage;
using Shared.Application.Messaging;

namespace Users.Application.Features.Users.Commands.Import.Records;

public record ImportSheetUsersCommand(
    IFileUpload File
) : ICommand<int>;
