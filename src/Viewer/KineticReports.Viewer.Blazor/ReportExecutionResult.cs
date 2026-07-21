namespace KineticReports.Viewer.Blazor;

using KineticReports.Core.Layout;

/// <summary>
/// Result of a report execution in a Blazor component.
/// </summary>
public sealed class ReportExecutionResult
{
    /// <summary>Gets whether the execution was successful.</summary>
    public required bool Success { get; set; }

    /// <summary>Gets the timestamp of execution.</summary>
    public required DateTime ExecutedAt { get; set; }

    /// <summary>Gets the resulting report layout (null if unsuccessful).</summary>
    public ReportLayout? ReportLayout { get; set; }

    /// <summary>Gets the error message if execution failed.</summary>
    public string? ErrorMessage { get; set; }
}
