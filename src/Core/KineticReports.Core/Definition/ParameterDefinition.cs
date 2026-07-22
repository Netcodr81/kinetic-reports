namespace KineticReports.Core.Definition;

/// <summary>
/// Describes a parameter that can be supplied to the report engine at runtime.
/// Parameters are declared in the <see cref="ReportDefinition"/> and resolved
/// before the layout pipeline begins.
/// </summary>
public sealed record ParameterDefinition
{
    /// <summary>Gets or inits the unique identifier of this parameter.</summary>
    public required string Id { get; init; }

    /// <summary>Gets or inits the human-readable display name shown in parameter pickers.</summary>
    public required string Name { get; init; }

    /// <summary>Gets or inits the data type of this parameter.</summary>
    public ParameterType Type { get; init; } = ParameterType.String;

    /// <summary>
    /// Gets or inits a value indicating whether this parameter must be provided
    /// before the report can be executed.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets or inits the default value as a string representation.
    /// <see langword="null"/> means no default is applied.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>Gets or inits a user-friendly description of the parameter's purpose.</summary>
    public string? Description { get; init; }
}
