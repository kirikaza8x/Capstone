using FluentValidation;
using Users.Application.Features.Users.Commands.Import.Records;

public class ImportSheetUsersCommandValidator : AbstractValidator<ImportSheetUsersCommand>
{
    private static readonly string[] AllowedContentTypes =
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-excel" };

    private const long MaxFileSize = 20 * 1024 * 1024; // 20 MB

    public ImportSheetUsersCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Excel file is required.")
            .Must(f => f.Length > 0).WithMessage("File is empty.")
            .Must(f => f.Length <= MaxFileSize).WithMessage("File too large. Max size is 20 MB.")
            .Must(f => AllowedContentTypes.Contains(f.ContentType.ToLowerInvariant()))
            .WithMessage("Invalid file type. Allowed: .xlsx or .xls.");
    }
}
