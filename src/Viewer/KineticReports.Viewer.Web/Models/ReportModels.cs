namespace KineticReports.Viewer.Web.Models;

/// <summary>
/// Request to execute a report with parameters.
/// </summary>
public sealed class ExecuteReportRequest
{
    /// <summary>Gets or sets the report definition JSON.</summary>
    public required string Definition { get; set; }

    /// <summary>Gets or sets the execution parameters by name.</summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; set; } = new Dictionary<string, object?>();
}

/// <summary>
/// Response containing the report execution result.
/// </summary>
public sealed class ExecuteReportResponse
{
    /// <summary>Gets the operation ID for tracking this execution.</summary>
    public required string OperationId { get; set; }

    /// <summary>Gets the execution status.</summary>
    public required string Status { get; set; }

    /// <summary>Gets the error message if execution failed.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Gets the execution timestamp.</summary>
    public DateTime ExecutedAt { get; set; }
}

/// <summary>
/// Response for a visual hit-test query.
/// </summary>
public sealed class ReportHitTestResponse
{
    /// <summary>Gets whether a visual element was hit.</summary>
    public required bool Hit { get; set; }

    /// <summary>Gets the page number used for the query.</summary>
    public required int PageNumber { get; set; }

    /// <summary>Gets the queried X coordinate in page-local DIPs.</summary>
    public required float X { get; set; }

    /// <summary>Gets the queried Y coordinate in page-local DIPs.</summary>
    public required float Y { get; set; }

    /// <summary>Gets the hit element id when <see cref="Hit"/> is true.</summary>
    public string? ElementId { get; set; }

    /// <summary>Gets the hit layer name when <see cref="Hit"/> is true.</summary>
    public string? LayerName { get; set; }

    /// <summary>Gets source metadata for the hit element when available.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// One text match returned by report search.
/// </summary>
public sealed class ReportTextSearchMatchResponse
{
    /// <summary>Gets the one-based page number containing the match.</summary>
    public required int PageNumber { get; set; }

    /// <summary>Gets the layer name containing the match.</summary>
    public required string LayerName { get; set; }

    /// <summary>Gets the matched visual element id.</summary>
    public required string ElementId { get; set; }

    /// <summary>Gets the matched text value.</summary>
    public required string Text { get; set; }
}

/// <summary>
/// Response payload for report text search.
/// </summary>
public sealed class ReportTextSearchResponse
{
    /// <summary>Gets the requested query string.</summary>
    public required string Query { get; set; }

    /// <summary>Gets optional page filter used during search.</summary>
    public int? PageNumber { get; set; }

    /// <summary>Gets total number of returned matches.</summary>
    public required int MatchCount { get; set; }

    /// <summary>Gets ordered text matches.</summary>
    public IReadOnlyList<ReportTextSearchMatchResponse> Matches { get; set; } = [];
}
