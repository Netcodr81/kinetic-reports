namespace KineticReports.Authoring.Components;

/// <summary>
/// Represents a component type that can be placed on a report designer canvas.
/// </summary>
public enum ReportComponentType
{
    /// <summary>Displays static or expression-based text.</summary>
    Text,

    /// <summary>Displays tabular data with header and data rows.</summary>
    Table,

    /// <summary>Displays an image sourced by URL or binary data binding.</summary>
    Image,

    /// <summary>Displays a machine-readable barcode.</summary>
    Barcode,

    /// <summary>Groups children components under a repeated data scope.</summary>
    Group,

    /// <summary>Defines a visual section within a report page.</summary>
    Section,
}
