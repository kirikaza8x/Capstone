namespace Shared.Application.Abstractions.Report
{
    /// <summary>
    /// Defines a contract for mapping entities to and from sheet rows/worksheets.
    /// Keeps Shared project clean of ClosedXML dependency.
    /// </summary>
    public interface ISheetMappings<T>
    {
        /// <summary>
        /// Returns a delegate that maps a row into an entity.
        /// The row type is abstracted as object to avoid ClosedXML reference.
        /// </summary>
        Func<object, T> GetRowMapper();

        /// <summary>
        /// Returns a delegate that exports a collection of entities into a worksheet.
        /// The worksheet type is abstracted as object to avoid ClosedXML reference.
        /// </summary>
        Action<object, IEnumerable<T>> Exporter { get; }
    }
}
