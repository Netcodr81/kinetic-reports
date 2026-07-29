namespace KineticReports.Core.Engine.Expressions;

/// <summary>
/// Provides contextual data available during expression evaluation,
/// including the current data row, report parameters, and pagination state.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Overview:</strong>
/// The expression context is the evaluation environment for all report expressions (e.g., <c>{OrderDate}</c>, <c>{CustomerName}</c>).
/// When an expression is evaluated, the evaluator looks up field names, parameter names, and special tokens in this context.
/// </para>
/// 
/// <para>
/// <strong>Available in Expressions:</strong>
/// <list type="bullet">
/// <item>
///   <term>Data Fields (from <see cref="CurrentRow"/>):</term>
///   <description>
///   Use <c>{FieldName}</c> syntax to reference fields from the current data row.
///   Example expressions:
///   <list type="bullet">
///     <item><c>{OrderDate}</c> — Current row's OrderDate field</item>
///     <item><c>{Customer}</c> — Current row's Customer field</item>
///     <item><c>{Amount}</c> — Current row's Amount field</item>
///   </list>
///   If a field is not found in the current row, the evaluator returns <see langword="null"/> (silent failure).
///   </description>
/// </item>
/// <item>
///   <term>Report Parameters (from <see cref="Parameters"/>):</term>
///   <description>
///   Use <c>{ParameterName}</c> syntax to reference report parameters.
///   Example expressions:
///   <list type="bullet">
///     <item><c>{StartDate}</c> — Report's StartDate parameter</item>
///     <item><c>{ReportTitle}</c> — Report's ReportTitle parameter</item>
///     <item><c>{Currency}</c> — Report's Currency parameter</item>
///   </list>
///   Parameters are available throughout the report execution.
///   </description>
/// </item>
/// <item>
///   <term>Page Information (from <see cref="PageNumber"/> and <see cref="TotalPages"/>):</term>
///   <description>
///   Use special tokens for pagination information:
///   <list type="bullet">
///     <item><c>{PageNumber}</c> — Current page number (1-indexed)</item>
///     <item><c>{TotalPages}</c> — Total number of pages (available after pagination only)</item>
///   </list>
///   Page information is only available after the pagination pass completes.
///   During initial data resolution and expression evaluation, page numbers may not yet be determined.
///   </description>
/// </item>
/// <item>
///   <term>Row Position (from <see cref="RowIndex"/>):</term>
///   <description>
///   Use <c>{RowNumber}</c> to reference row position within the data source.
///   <c>{RowIndex}</c> is zero-based; to display as 1-based, add 1 in expression.
///   Example:
///   <list type="bullet">
///     <item><c>Row {RowIndex + 1}</c> — Displays "Row 1", "Row 2", etc.</item>
///   </list>
///   </description>
/// </item>
/// </list>
/// </para>
/// 
/// <para>
/// <strong>Evaluation Behavior:</strong>
/// <list type="bullet">
/// <item>Field lookup is case-sensitive.</item>
/// <item>Expressions can be plain literals: <c>Hello, World</c> (no <c>{}</c>) returns the literal string.</item>
/// <item>If a referenced field is not found, the evaluator returns <see langword="null"/> (not an exception).</item>
/// <item>To validate expressions at design-time, use <c>IExpressionValidator</c> interface.</item>
/// </list>
/// </para>
/// 
/// <para>
/// <strong>Availability Timeline:</strong>
/// <list type="table">
/// <listheader><term>Context Field</term><description>Available When</description></listheader>
/// <item><term><see cref="CurrentRow"/></term><description>During row processing (always)</description></item>
/// <item><term><see cref="Parameters"/></term><description>During entire report execution (always)</description></item>
/// <item><term><see cref="RowIndex"/></term><description>During row processing (always)</description></item>
/// <item><term><see cref="PageNumber"/></term><description>After pagination pass completes</description></item>
/// <item><term><see cref="TotalPages"/></term><description>After pagination pass completes; may be <see langword="null"/> if pagination not performed</description></item>
/// </list>
/// </para>
/// </remarks>
public sealed class ExpressionContext
{
    /// <summary>
    /// Gets the fields of the current data row as a name-to-value dictionary.
    /// Use <c>{FieldName}</c> syntax in expressions to reference these fields.
    /// </summary>
    /// <remarks>
    /// Field lookup is case-sensitive. If a field is not found, expression evaluation returns <see langword="null"/>.
    /// </remarks>
    public required IReadOnlyDictionary<string, object?> CurrentRow { get; init; }

    /// <summary>
    /// Gets the resolved report parameters as a name-to-value dictionary.
    /// Use <c>{ParameterName}</c> syntax in expressions to reference these parameters.
    /// Available throughout the entire report execution.
    /// </summary>
    /// <remarks>
    /// Parameters are passed at report execution time and remain constant during layout and rendering.
    /// </remarks>
    public required IReadOnlyDictionary<string, object?> Parameters { get; init; }

    /// <summary>
    /// Gets the zero-based index of the current data row within its data source.
    /// Use <c>{RowIndex + 1}</c> in expressions to display as 1-based row numbers.
    /// </summary>
    /// <remarks>
    /// This value is set during row enumeration and reflects the position in the current data set.
    /// To display as 1-based, add 1 in your expression: <c>Row {RowIndex + 1}</c>.
    /// </remarks>
    public int RowIndex { get; init; }

    /// <summary>
    /// Gets the one-based current page number as determined during the pagination pass.
    /// Use <c>{PageNumber}</c> in expressions. Only available after pagination completes.
    /// </summary>
    /// <remarks>
    /// Page numbers are zero during the initial layout passes (Measure/Arrange).
    /// This value is only meaningful after pagination has assigned pages to content.
    /// If pagination was not performed, this value may be 0.
    /// </remarks>
    public int PageNumber { get; init; }

    /// <summary>
    /// Gets the total page count when known; otherwise <see langword="null"/>.
    /// Use <c>{TotalPages}</c> in expressions (e.g., "Page {PageNumber} of {TotalPages}").
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is <see langword="null"/> if:
    /// <list type="bullet">
    /// <item>Pagination has not been performed yet.</item>
    /// <item>The report has a single page and pagination was optimized.</item>
    /// </list>
    /// </para>
    /// <para>
    /// If used in an expression before pagination completes, it evaluates to <see langword="null"/>.
    /// </para>
    /// </remarks>
    public int? TotalPages { get; init; }

    /// <summary>
    /// Returns an <see cref="ExpressionContext"/> with no data row or parameters.
    /// Use this for evaluating expressions that don't depend on row data or parameters.
    /// </summary>
    /// <remarks>
    /// The <see cref="Empty"/> context has:
    /// <list type="bullet">
    /// <item><see cref="CurrentRow"/>: Empty dictionary</item>
    /// <item><see cref="Parameters"/>: Empty dictionary</item>
    /// <item><see cref="RowIndex"/>: 0</item>
    /// <item><see cref="PageNumber"/>: 0</item>
    /// <item><see cref="TotalPages"/>: <see langword="null"/></item>
    /// </list>
    /// This is useful for testing expressions or evaluating static text expressions.
    /// </remarks>
    public static ExpressionContext Empty { get; } = new ExpressionContext
    {
        CurrentRow = new Dictionary<string, object?>(),
        Parameters = new Dictionary<string, object?>()
    };
}
