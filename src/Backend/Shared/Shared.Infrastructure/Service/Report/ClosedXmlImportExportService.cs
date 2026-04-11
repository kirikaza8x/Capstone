using ClosedXML.Excel;
using Shared.Application.Abstractions.Report;

namespace Shared.Infrastructure.Service.Report;
public class ClosedXmlImportExportService<T> : IFileImportExportService<T>
{
    private readonly Func<IXLRow, T> _rowMapper;
    private readonly Action<IXLWorksheet, IEnumerable<T>> _exporter;

    public ClosedXmlImportExportService(Func<IXLRow, T> rowMapper,
                                        Action<IXLWorksheet, IEnumerable<T>> exporter)
    {
        _rowMapper = rowMapper;
        _exporter = exporter;
    }

    public async Task<IReadOnlyList<T>> ImportAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);

        var range = worksheet.RangeUsed();
        if (range == null)
        {
            return await Task.FromResult(Array.Empty<T>());
        }

        var rows = range.RowsUsed().Cast<IXLRow>().Skip(1);

        var entities = rows.Select(_rowMapper).ToList();
        return await Task.FromResult<IReadOnlyList<T>>(entities);
    }

    public async Task<byte[]> ExportAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Export");

        _exporter(worksheet, entities);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }
}
