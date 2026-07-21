namespace KineticReports.Core.Layout;

/// <summary>Identifies the concrete type of a <see cref="LayoutBlock"/>.</summary>
public enum LayoutBlockType
{
    /// <summary>A physical or logical report page.</summary>
    Page,

    /// <summary>A horizontal section of a page (header, body, footer).</summary>
    Section,

    /// <summary>A report content region (detail, group header, group footer, etc.).</summary>
    ContentRegion,

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
