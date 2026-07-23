namespace KineticReports.Core.Layout;

/// <summary>
/// Strongly typed block taxonomy for code-first report authoring.
/// </summary>
/// <remarks>
/// This enum is authoring-facing. Runtime layout still uses concrete
/// <see cref="LayoutBlock"/> types.
/// </remarks>
public enum CodeBlockType
{
    // ---------------------------------------------------------------------
    // Structure
    // ---------------------------------------------------------------------

    /// <summary>Represents the page root.</summary>
    Page,

    /// <summary>Represents a section region.</summary>
    Section,

    /// <summary>Represents a generic panel/container region.</summary>
    Panel,

    /// <summary>Represents a grouped repeating region.</summary>
    Group,

    /// <summary>Represents list-style repetition; canonicalized to <see cref="Group"/>.</summary>
    List,

    /// <summary>Represents tabular layout.</summary>
    Table,

    // ---------------------------------------------------------------------
    // Content
    // ---------------------------------------------------------------------

    /// <summary>Represents text content.</summary>
    Text,

    /// <summary>Represents image content.</summary>
    Image,

    /// <summary>Represents chart content.</summary>
    Chart,

    /// <summary>Represents a line shape; canonicalized to <see cref="Rectangle"/> with shape metadata.</summary>
    Line,

    /// <summary>Represents rectangle shape content.</summary>
    Rectangle,

    /// <summary>Represents ellipse shape content; canonicalized to <see cref="Rectangle"/> with shape metadata.</summary>
    Ellipse,

    /// <summary>Represents barcode (or QR) content.</summary>
    Barcode,

    // ---------------------------------------------------------------------
    // Supporting
    // ---------------------------------------------------------------------

    /// <summary>Represents spacing support; canonicalized to <see cref="Panel"/>.</summary>
    Spacer,

    /// <summary>Represents divider support; canonicalized to <see cref="Line"/>.</summary>
    Divider,

    /// <summary>Represents page-break behavior; canonicalized to <see cref="Panel"/> + pagination flags.</summary>
    PageBreak,

    /// <summary>Represents current page-number token; canonicalized to <see cref="Text"/>.</summary>
    PageNumber,

    /// <summary>Represents total-pages token; canonicalized to <see cref="Text"/>.</summary>
    TotalPages,

    /// <summary>Represents current-date token; canonicalized to <see cref="Text"/>.</summary>
    CurrentDate,

    /// <summary>Represents current-time token; canonicalized to <see cref="Text"/>.</summary>
    CurrentTime,

    /// <summary>Represents document-info token content; canonicalized to <see cref="Text"/>.</summary>
    DocumentInfo,

    /// <summary>Represents header support; canonicalized to <see cref="Section"/>.</summary>
    Header,

    /// <summary>Represents footer support; canonicalized to <see cref="Section"/>.</summary>
    Footer,

    /// <summary>Represents background support; canonicalized to <see cref="Panel"/>.</summary>
    Background,
}

/// <summary>
/// Broad block families used by code-first authoring tooling.
/// </summary>
public enum CodeBlockFamily
{
    /// <summary>Structural composition blocks.</summary>
    Structure,

    /// <summary>Renderable content blocks.</summary>
    Content,

    /// <summary>Behavioral/supporting blocks.</summary>
    Supporting,
}

/// <summary>
/// Extension helpers for code-first block typing.
/// </summary>
public static class CodeBlockTypeExtensions
{
    /// <summary>
    /// Gets the broad family for this code block type.
    /// </summary>
    public static CodeBlockFamily GetFamily(this CodeBlockType blockType)
    {
        return blockType switch
        {
            CodeBlockType.Page
            or CodeBlockType.Section
            or CodeBlockType.Panel
            or CodeBlockType.Group
            or CodeBlockType.List
            or CodeBlockType.Table => CodeBlockFamily.Structure,

            CodeBlockType.Text
            or CodeBlockType.Image
            or CodeBlockType.Chart
            or CodeBlockType.Line
            or CodeBlockType.Rectangle
            or CodeBlockType.Ellipse
            or CodeBlockType.Barcode => CodeBlockFamily.Content,

            _ => CodeBlockFamily.Supporting,
        };
    }

    /// <summary>
    /// Collapses aliases to canonical code block types.
    /// </summary>
    public static CodeBlockType ToCanonicalType(this CodeBlockType blockType)
    {
        return blockType switch
        {
            CodeBlockType.List => CodeBlockType.Group,
            CodeBlockType.Divider => CodeBlockType.Line,
            CodeBlockType.Ellipse => CodeBlockType.Rectangle,
            CodeBlockType.Spacer => CodeBlockType.Panel,
            CodeBlockType.PageBreak => CodeBlockType.Panel,
            CodeBlockType.PageNumber => CodeBlockType.Text,
            CodeBlockType.TotalPages => CodeBlockType.Text,
            CodeBlockType.CurrentDate => CodeBlockType.Text,
            CodeBlockType.CurrentTime => CodeBlockType.Text,
            CodeBlockType.DocumentInfo => CodeBlockType.Text,
            CodeBlockType.Header => CodeBlockType.Section,
            CodeBlockType.Footer => CodeBlockType.Section,
            CodeBlockType.Background => CodeBlockType.Panel,
            _ => blockType,
        };
    }

    /// <summary>
    /// Gets the runtime layout block type represented by this code block type.
    /// </summary>
    public static LayoutBlockType ToLayoutBlockType(this CodeBlockType blockType)
    {
        return blockType.ToCanonicalType() switch
        {
            CodeBlockType.Page => LayoutBlockType.Page,
            CodeBlockType.Section => LayoutBlockType.Section,
            CodeBlockType.Panel => LayoutBlockType.Container,
            CodeBlockType.Group => LayoutBlockType.ContentRegion,
            CodeBlockType.Table => LayoutBlockType.Table,
            CodeBlockType.Text => LayoutBlockType.Text,
            CodeBlockType.Image => LayoutBlockType.Image,
            CodeBlockType.Chart => LayoutBlockType.Chart,
            CodeBlockType.Rectangle => LayoutBlockType.Shape,
            CodeBlockType.Line => LayoutBlockType.Shape,
            CodeBlockType.Barcode => LayoutBlockType.Barcode,
            _ => LayoutBlockType.Container,
        };
    }
}