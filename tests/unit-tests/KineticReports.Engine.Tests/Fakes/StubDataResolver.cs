namespace KineticReports.Engine.Tests.Fakes;

using KineticReports.Core.Definition;
using KineticReports.Core.Engine.Data;

/// <summary>
/// A configurable <see cref="IDataResolver"/> stub that returns pre-set rows.
/// </summary>
public sealed class StubDataResolver : IDataResolver
{
    private readonly Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object?>>> _data = [];

    /// <summary>Registers rows to return for the given data-source ID.</summary>
    public void AddRows(string dataSourceId, IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
        => _data[dataSourceId] = rows;

    /// <summary>Gets the number of times <see cref="ResolveAsync"/> was called.</summary>
    public int CallCount { get; private set; }

    /// <inheritdoc/>
    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ResolveAsync(
        DataSourceDefinition definition,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        if (_data.TryGetValue(definition.Id, out var rows))
            return Task.FromResult(rows);

        IReadOnlyList<IReadOnlyDictionary<string, object?>> empty = [];
        return Task.FromResult(empty);
    }
}
