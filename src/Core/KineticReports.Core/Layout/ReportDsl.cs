namespace KineticReports.Core.Layout;

using KineticReports.Core.Styling;

/// <summary>
/// Minimal code-first DSL helpers for common report construction patterns.
/// </summary>
public static class ReportDsl
{
    /// <summary>
    /// Creates a single-text report region block for the given block role.
    /// </summary>
    /// <param name="id">Stable region id.</param>
    /// <param name="blockType">Report region role.</param>
    /// <param name="blockStyle">Resolved style for the containing region.</param>
    /// <param name="textStyle">Resolved style for the text child.</param>
    /// <param name="text">Text content.</param>
    /// <returns>A report block containing one text element.</returns>
    public static ReportBlock TextRegion(
        string id,
        BlockType blockType,
        AppliedStyle blockStyle,
        AppliedStyle textStyle,
        string text)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(blockStyle);
        ArgumentNullException.ThrowIfNull(textStyle);
        ArgumentNullException.ThrowIfNull(text);

        var child = ContentBlockFactory.CreateText($"{id}-text", textStyle, text);
        return ReportBlockFactory.Create(blockType, id, blockStyle, [child]);
    }

    /// <summary>
    /// Creates a table cell containing a single text element.
    /// </summary>
    /// <param name="id">Stable cell id.</param>
    /// <param name="columnIndex">Zero-based table column index.</param>
    /// <param name="cellStyle">Resolved style for the cell container.</param>
    /// <param name="textStyle">Resolved style for the text child.</param>
    /// <param name="text">Cell text content.</param>
    /// <returns>A table cell with a single text child.</returns>
    public static CellBlock TextCell(
        string id,
        int columnIndex,
        AppliedStyle cellStyle,
        AppliedStyle textStyle,
        string text)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(cellStyle);
        ArgumentNullException.ThrowIfNull(textStyle);
        ArgumentNullException.ThrowIfNull(text);

        return new CellBlock
        {
            Id = id,
            ColumnIndex = columnIndex,
            Style = cellStyle,
            Children =
            [
                ContentBlockFactory.CreateText($"{id}-text", textStyle, text)
            ]
        };
    }
}