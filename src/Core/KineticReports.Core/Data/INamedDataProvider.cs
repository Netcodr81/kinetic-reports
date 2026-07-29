namespace KineticReports.Core.Data;

/// <summary>
/// Associates an <see cref="IDataProvider"/> with an explicit source name and provider type.
/// </summary>
public interface INamedDataProvider
{
    /// <summary>
    /// Gets the required source name used by <c>DataSourceDefinition.SourceName</c>.
    /// </summary>
    string SourceName { get; }

    /// <summary>
    /// Gets the provider type category (for example: Poco, SqlServer, SqlLite).
    /// </summary>
    string ProviderType { get; }

    /// <summary>
    /// Gets the underlying data provider.
    /// </summary>
    IDataProvider Provider { get; }
}
