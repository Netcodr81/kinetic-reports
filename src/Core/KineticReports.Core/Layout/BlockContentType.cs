namespace KineticReports.Core.Layout;

/// <summary>
/// Identifies the content type of a <see cref="ContentBlock"/>.
/// This enum replaces the previous 12-class hierarchy with a single unified model.
/// </summary>
public enum BlockContentType
{
    /// <summary>A physical or logical report page.</summary>
    Page,

    /// <summary>A logical report section (detail, group header, group footer, etc.).</summary>
    ReportSection,

    /// <summary>A physical page section (header, body, footer).</summary>
    PageSection,

    /// <summary>A generic container that groups child blocks.</summary>
    Container,

    /// <summary>A text paragraph or run block.</summary>
    Text,

    /// <summary>A tabular data block.</summary>
    Table,

    /// <summary>A table row.</summary>
    Row,

    /// <summary>A table cell.</summary>
    Cell,

    /// <summary>An image block.</summary>
    Image,

    /// <summary>A vector shape (rectangle, ellipse, or line).</summary>
    Shape,

    /// <summary>A chart block.</summary>
    Chart,

    /// <summary>A barcode or QR code block.</summary>
    Barcode,
}
