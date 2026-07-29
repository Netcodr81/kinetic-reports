namespace KineticReports.Core.Layout;

/// <summary>
/// Quick reference guide for choosing the correct block type for your layout.
/// Use this guide when designing report blocks to understand which type to use for each scenario.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Block Type Decision Tree:</strong>
/// </para>
/// <list type="table">
/// <listheader>
///   <term>Scenario</term>
///   <description>Block Type to Use</description>
/// </listheader>
/// <item>
///   <term>Display static or data-bound text</term>
///   <description>
///   Use <see cref="TextBlock"/>.
///   Example: "Report Title", "Order Date: {OrderDate}"
///   Supports expression binding via <c>{FieldName}</c> syntax.
///   </description>
/// </item>
/// <item>
///   <term>Display an image or photo</term>
///   <description>
///   Use <see cref="ImageBlock"/>.
///   Example: Company logo, customer photo.
///   Can bind image source to data field using expressions.
///   </description>
/// </item>
/// <item>
///   <term>Display a chart or graph</term>
///   <description>
///   Use <see cref="ChartBlock"/>.
///   Example: Sales trend chart, revenue pie chart.
///   Charts typically bind to aggregated data sources.
///   </description>
/// </item>
/// <item>
///   <term>Display barcodes or QR codes</term>
///   <description>
///   Use <see cref="BarcodeBlock"/>.
///   Example: Product barcode, shipment tracking QR code.
///   Bind the code value to data fields.
///   </description>
/// </item>
/// <item>
///   <term>Create a table grid with rows and columns</term>
///   <description>
///   Use <see cref="TableBlock"/> containing <see cref="RowBlock"/> → <see cref="CellBlock"/>.
///   Structure:
///   <list type="bullet">
///     <item><see cref="TableBlock"/>: Container for the entire table</item>
///     <item><see cref="RowBlock"/>: Each row in the table</item>
///     <item><see cref="CellBlock"/>: Each cell in a row (contains text, images, etc.)</item>
///   </list>
///   Example: Sales data table, customer list, invoice line items.
///   </description>
/// </item>
/// <item>
///   <term>Repeat child blocks for each data row (non-tabular layout)</term>
///   <description>
///   Use <see cref="ContainerBlock"/> with row-level iteration.
///   Example: List of invoices, repeating customer cards.
///   Unlike tables, containers allow flexible layout of child blocks.
///   </description>
/// </item>
/// <item>
///   <term>Add decorative shapes (rectangles, ellipses, lines)</term>
///   <description>
///   Use <see cref="ShapeBlock"/> with <see cref="Rendering.PathGeometry"/>.
///   Example: Decorative borders, section separators, highlighting boxes.
///   Shapes are visual elements with no data binding.
///   </description>
/// </item>
/// <item>
///   <term>Report-level sections (header, detail, footer)</term>
///   <description>
///   Use <see cref="ReportBlock"/> subtypes:
///   <list type="bullet">
///     <item><see cref="HeaderBlock"/>: Appears once at the beginning (e.g., title, summary)</item>
///     <item><see cref="DetailBlock"/>: Repeats for each data row</item>
///     <item><see cref="FooterBlock"/>: Appears once at the end (e.g., totals, signature)</item>
///   </list>
///   These blocks have special semantics for report structure.
///   </description>
/// </item>
/// <item>
///   <term>Page-level header/footer (repeated on every page)</term>
///   <description>
///   Use <see cref="PageHeaderBlock"/> or <see cref="PageFooterBlock"/> (subtypes of <see cref="ReportBlock"/>).
///   Example: Page numbers "Page {PageNumber} of {TotalPages}", company name, page dividers.
///   These blocks are rendered on every page after pagination.
///   </description>
/// </item>
/// <item>
///   <term>Grouping sections within detail rows</term>
///   <description>
///   Use <see cref="GroupHeaderBlock"/> or <see cref="GroupFooterBlock"/> (subtypes of <see cref="ReportBlock"/>).
///   Example: Group sales by region with region subtotals.
///   These blocks are repeated for each group in grouped data.
///   </description>
/// </item>
/// <item>
///   <term>Page boundaries (automatic)</term>
///   <description>
///   Use <see cref="PageBlock"/> — created automatically by the layout engine.
///   You typically do not create <see cref="PageBlock"/> instances directly.
///   The layout engine inserts page blocks during pagination.
///   </description>
/// </item>
/// <item>
///   <term>Manual section boundary within a page</term>
///   <description>
///   Use <see cref="PageSectionBlock"/>.
///   Example: Manually define subsections within a page layout.
///   Useful when automatic pagination isn't sufficient.
///   </description>
/// </item>
/// </list>
/// 
/// <para>
/// <strong>Important Notes:</strong>
/// <list type="bullet">
/// <item>
///   <strong>All block types inherit from <see cref="LayoutBlock"/>.</strong>
///   Each block implements the Measure/Arrange pattern for layout sizing and positioning.
/// </item>
/// <item>
///   <strong><see cref="ReportBlock"/> is a semantic container.</strong>
///   <see cref="ReportBlock"/> has subtypes (Header, Detail, Footer, PageHeader, PageFooter, GroupHeader, GroupFooter)
///   for express report structure. Most content uses other block types.
/// </item>
/// <item>
///   <strong>Table structure is hierarchical.</strong>
///   Never create a <see cref="CellBlock"/> outside a <see cref="RowBlock"/>.
///   Tables always follow: <see cref="TableBlock"/> → <see cref="RowBlock"/> → <see cref="CellBlock"/>.
/// </item>
/// <item>
///   <strong><see cref="ContainerBlock"/> is flexible.</strong>
///   Use containers when you need non-table layouts with multiple child blocks.
/// </item>
/// <item>
///   <strong>All blocks are immutable after Arrange pass.</strong>
///   The Bounds, DesiredSize, and Style properties are read-only after layout completes (ADR-011).
/// </item>
/// </list>
/// </para>
/// 
/// <para>
/// <strong>Common Layout Patterns:</strong>
/// <list type="bullet">
/// <item>
///   <strong>Simple Report:</strong>
///   HeaderBlock → (DetailBlock with TextBlocks) → FooterBlock
/// </item>
/// <item>
///   <strong>Tabular Report:</strong>
///   HeaderBlock → (DetailBlock with TableBlock) → FooterBlock
/// </item>
/// <item>
///   <strong>Multi-Section Report:</strong>
///   HeaderBlock → GroupHeaderBlock → (DetailBlock) → GroupFooterBlock → FooterBlock
/// </item>
/// <item>
///   <strong>Decorated Report:</strong>
///   (ShapeBlock decorations) → content blocks → (ShapeBlock footer line)
/// </item>
/// </list>
/// </para>
/// </remarks>
public static class BlockTypeHierarchyGuide
{
    // This is a marker class for documentation and IDE discoverability.
    // No runtime behavior is needed—it serves purely as a reference guide.
    //
    // How to use this guide in your IDE:
    // 1. Type "BlockTypeHierarchyGuide" in your code to access this documentation.
    // 2. View the detailed remarks above to understand block types and scenarios.
    // 3. Hover over block class names to see their specific documentation.
    // 4. Refer to the decision tree table when choosing a block type for your layout.
}
