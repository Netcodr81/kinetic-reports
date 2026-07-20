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
