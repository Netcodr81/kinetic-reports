namespace KineticReports.Core.Data;

/// <summary>
/// Associates an <see cref="IDataProvider"/> with a provider-type key used by
/// <c>DataSourceDefinition.ProviderType</c>.
/// </summary>
public interface INamedDataProvider
{
    /// <summary>
    /// Gets the provider-type key (for example: Poco, SqlServer, SqlLite).
    /// </summary>
    string ProviderType { get; }

    /// <summary>
    /// Gets the underlying data provider.
    /// </summary>
    IDataProvider Provider { get; }
}
