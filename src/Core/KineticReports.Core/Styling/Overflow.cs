namespace KineticReports.Core.Styling;

/// <summary>Specifies how content that overflows a layout element's bounds is handled.</summary>
public enum Overflow
{
    /// <summary>Overflowing content is clipped to the element's bounds.</summary>
    Hidden,

    /// <summary>Overflowing content is rendered outside the element's bounds.</summary>
    Visible,

    /// <summary>The element expands its height to accommodate overflowing content.</summary>
    Grow,
}
