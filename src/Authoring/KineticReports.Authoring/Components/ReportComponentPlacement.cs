namespace KineticReports.Authoring.Components;

/// <summary>
/// Absolute placement metadata for a designer component on a page canvas (in DIPs).
/// </summary>
public sealed record ReportComponentPlacement
{
    /// <summary>Gets or inits the X coordinate in DIPs.</summary>
    public float X { get; init; }

    /// <summary>Gets or inits the Y coordinate in DIPs.</summary>
    public float Y { get; init; }

    /// <summary>Gets or inits the width in DIPs.</summary>
    public float Width { get; init; }

    /// <summary>Gets or inits the height in DIPs.</summary>
    public float Height { get; init; }
}
