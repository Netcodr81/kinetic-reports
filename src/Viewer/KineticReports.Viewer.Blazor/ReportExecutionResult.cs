namespace KineticReports.Viewer.Blazor;

/// <summary>
/// Result of a report execution in a Blazor component.
/// </summary>
public sealed class ReportExecutionResult
{
    /// <summary>Gets whether the execution was successful.</summary>
    public required bool Success { get; set; }

    /// <summary>Gets the timestamp of execution.</summary>
    public required DateTime ExecutedAt { get; set; }

    /// <summary>Gets HTML preview content for the executed report.</summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>Gets trace entries captured during execution/export.</summary>
    public IReadOnlyList<string> Trace { get; set; } = [];

    /// <summary>Gets the error message if execution failed.</summary>
    public string? ErrorMessage { get; set; }
}
