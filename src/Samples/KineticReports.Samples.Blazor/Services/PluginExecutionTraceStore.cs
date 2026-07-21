namespace KineticReports.Samples.Blazor.Services;

/// <summary>
/// Captures plugin execution trace entries for the current scoped render flow.
/// </summary>
public interface IPluginExecutionTraceStore
{
    /// <summary>Gets the current trace entries in append order.</summary>
    IReadOnlyList<string> Entries { get; }

    /// <summary>Clears all trace entries for a new render execution.</summary>
    void Clear();

    /// <summary>Adds a new trace entry.</summary>
    /// <param name="entry">The trace message.</param>
    void Add(string entry);
}

/// <summary>
/// Default scoped implementation of <see cref="IPluginExecutionTraceStore"/>.
/// </summary>
public sealed class PluginExecutionTraceStore : IPluginExecutionTraceStore
{
    private readonly List<string> _entries = [];

    /// <inheritdoc/>
    public IReadOnlyList<string> Entries => _entries;

    /// <inheritdoc/>
    public void Clear() => _entries.Clear();

    /// <inheritdoc/>
    public void Add(string entry)
    {
        if (string.IsNullOrWhiteSpace(entry))
            return;

        _entries.Add(entry);
    }
}
