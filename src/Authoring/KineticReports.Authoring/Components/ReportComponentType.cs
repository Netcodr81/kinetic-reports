namespace KineticReports.Authoring.Components;

/// <summary>
/// Represents a component type that can be placed on a report designer canvas.
/// </summary>
public enum ReportComponentType
{
    // ---------------------------------------------------------------------
    // Structure
    // ---------------------------------------------------------------------

    /// <summary>Represents the root page surface in the designer.</summary>
    Page,

    /// <summary>Defines a visual section within a report page.</summary>
    Section,

    /// <summary>Generic container used to group child components.</summary>
    Panel,

    /// <summary>Groups children components under a repeated data scope.</summary>
    Group,

    /// <summary>Repeats child content for each row in a bound data source.</summary>
    List,

    /// <summary>Displays static or expression-based text.</summary>
    Text,

    /// <summary>Displays tabular data with header and data rows.</summary>
    Table,

    // ---------------------------------------------------------------------
    // Content
    // ---------------------------------------------------------------------

    /// <summary>Displays an image sourced by URL or binary data binding.</summary>
    Image,

    /// <summary>Displays a chart using a configured chart type.</summary>
    Chart,

    /// <summary>Displays a line shape.</summary>
    Line,

    /// <summary>Displays a rectangle shape.</summary>
    Rectangle,

    /// <summary>Displays an ellipse shape.</summary>
    Ellipse,

    /// <summary>Displays a machine-readable barcode.</summary>
    Barcode,

    // ---------------------------------------------------------------------
    // Supporting
    // ---------------------------------------------------------------------

    /// <summary>Creates flexible empty space in layout.</summary>
    Spacer,

    /// <summary>Draws a visual separator between adjacent regions.</summary>
    Divider,

    /// <summary>Forces pagination to begin on a new page.</summary>
    PageBreak,

    /// <summary>Renders the current page number token.</summary>
    PageNumber,

    /// <summary>Renders the total page count token.</summary>
    TotalPages,

    /// <summary>Renders the current date token.</summary>
    CurrentDate,

    /// <summary>Renders the current time token.</summary>
    CurrentTime,

    /// <summary>Renders document metadata such as title or author.</summary>
    DocumentInfo,

    /// <summary>Defines content that belongs to the report/page header area.</summary>
    Header,

    /// <summary>Defines content that belongs to the report/page footer area.</summary>
    Footer,

    /// <summary>Defines a background region behind report content.</summary>
    Background,
}
