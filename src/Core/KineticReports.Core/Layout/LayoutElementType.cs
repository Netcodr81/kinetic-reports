namespace KineticReports.Core.Layout;

/// <summary>Identifies the concrete type of a <see cref="LayoutElement"/>.</summary>
public enum LayoutElementType
{
    /// <summary>A physical or logical report page.</summary>
    Page,

    /// <summary>A horizontal section of a page (header, body, footer).</summary>
    Section,

    /// <summary>A data-driven band (detail, group header, group footer, etc.).</summary>
    Band,

    /// <summary>A generic container that groups child elements.</summary>
    Container,

    /// <summary>A text paragraph or run element.</summary>
    Text,

    /// <summary>A tabular data element.</summary>
    Table,

    /// <summary>A table row.</summary>
    Row,

    /// <summary>A table cell.</summary>
    Cell,

    /// <summary>An image element.</summary>
    Image,

    /// <summary>A vector shape (rectangle, ellipse, or line).</summary>
    Shape,

    /// <summary>A chart element.</summary>
    Chart,

    /// <summary>A barcode or QR code element.</summary>
    Barcode,
}
