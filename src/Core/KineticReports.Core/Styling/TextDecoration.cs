namespace KineticReports.Core.Styling;

/// <summary>Specifies one or more decorations applied to text.</summary>
[Flags]
public enum TextDecoration
{
    /// <summary>No decoration is applied.</summary>
    None = 0,

    /// <summary>A line is drawn beneath the text.</summary>
    Underline = 1 << 0,

    /// <summary>A line is drawn through the middle of the text.</summary>
    Strikethrough = 1 << 1,

    /// <summary>A line is drawn above the text.</summary>
    Overline = 1 << 2,
}
