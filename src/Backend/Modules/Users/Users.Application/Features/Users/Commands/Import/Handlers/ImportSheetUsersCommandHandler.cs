using FluentValidation;
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Report;
using Shared.Domain.Abstractions;
using Users.Application.Features.Users.Commands.Import.Records;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.UOW;


namespace Users.Application.Features.Users.Commands.Import.Handlers;
public class ImportSheetUsersCommandHandler
    : ICommandHandler<ImportSheetUsersCommand, int>
{
    private readonly IFileImportExportService<User> _excelService;
    private readonly IUserRepository _userRepository;
    private readonly IUserUnitOfWork _unitOfWork;
    private readonly IValidator<ImportSheetUsersCommand> _validator;

    public ImportSheetUsersCommandHandler(
        IFileImportExportService<User> excelService,
        IUserRepository userRepository,
        IUserUnitOfWork unitOfWork,
        IValidator<ImportSheetUsersCommand> validator)
    {
        _excelService = excelService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<Result<int>> Handle(
        ImportSheetUsersCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            var error = validation.Errors.First();
            return Result.Failure<int>(
                Error.Validation("Import.Validation", error.ErrorMessage));
        }

        var users = await _excelService.ImportAsync(command.File.OpenReadStream(), cancellationToken);
        if (!users.Any())
            return Result.Failure<int>(Error.Validation("Import.EmptyFile", "No users found in the record file."));


        await _userRepository.BulkInsertAsync(users, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(users.Count);
    }
}
