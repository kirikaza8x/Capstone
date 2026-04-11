
using Shared.Application.Abstractions.Messaging;
using Shared.Application.Abstractions.Report;
using Shared.Domain.Abstractions;
using Users.Domain.Entities;
using Users.Domain.Repositories;

public class ExportSheetUsersQueryHandler
    : IQueryHandler<ExportSheetUsersQuery, byte[]>
{
    private readonly IFileImportExportService<User> _excelService;
    private readonly IUserRepository _userRepository;

    public ExportSheetUsersQueryHandler(
        IFileImportExportService<User> excelService,
        IUserRepository userRepository)
    {
        _excelService = excelService;
        _userRepository = userRepository;
    }

    public async Task<Result<byte[]>> Handle(
        ExportSheetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var fileBytes = await _excelService.ExportAsync(users, cancellationToken);

        return Result.Success(fileBytes);
    }
}
