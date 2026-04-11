
namespace Shared.Application.Abstractions.Report;
public interface IFileImportExportService<T>
{
    Task<IReadOnlyList<T>> ImportAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<byte[]> ExportAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
