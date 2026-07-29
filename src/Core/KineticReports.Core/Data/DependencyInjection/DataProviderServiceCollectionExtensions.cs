namespace KineticReports.Core.Data.DependencyInjection;

using KineticReports.Core.Data.SqlLite;
using KineticReports.Core.Data.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Dependency injection helpers for KineticReports data providers.
/// </summary>
public static class DataProviderServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="PocoDataProvider"/> as a named source.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="sourceName">Required source name used by report data sources.</param>
    /// <param name="configure">Optional callback used to seed/modify the provider instance at startup.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddPocoDataProvider(
        this IServiceCollection services,
        string sourceName,
        Action<PocoDataProvider>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new ArgumentException("Source name is required.", nameof(sourceName));

        var provider = new PocoDataProvider();
        configure?.Invoke(provider);

        services.AddSingleton<INamedDataProvider>(new NamedDataProvider(sourceName, "Poco", provider));
        services.TryAddSingleton<ILinqDataProvider>(provider);

        return services;
    }

    /// <summary>
    /// Registers a singleton <see cref="SqlServerDataProvider"/> as a named source.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="sourceName">Required source name used by report data sources.</param>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddSqlServerDataProvider(
        this IServiceCollection services,
        string sourceName,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new ArgumentException("Source name is required.", nameof(sourceName));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        var provider = new SqlServerDataProvider(connectionString);
        services.AddSingleton<INamedDataProvider>(new NamedDataProvider(sourceName, "SqlServer", provider));

        return services;
    }

    /// <summary>
    /// Registers a singleton <see cref="SqlLiteDataProvider"/> as a named source.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="sourceName">Required source name used by report data sources.</param>
    /// <param name="connectionString">SQLite connection string.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddSqlLiteDataProvider(
        this IServiceCollection services,
        string sourceName,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new ArgumentException("Source name is required.", nameof(sourceName));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        var provider = new SqlLiteDataProvider(connectionString);
        services.AddSingleton<INamedDataProvider>(new NamedDataProvider(sourceName, "SqlLite", provider));

        return services;
    }

    /// <summary>
    /// Registers an existing provider instance as a named source.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="sourceName">Required source name used by report data sources.</param>
    /// <param name="providerType">Provider type category (for example: SqlServer, SqlLite, Poco).</param>
    /// <param name="provider">Provider instance.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddNamedDataProvider(
        this IServiceCollection services,
        string sourceName,
        string providerType,
        IDataProvider provider)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(provider);
        if (string.IsNullOrWhiteSpace(sourceName))
            throw new ArgumentException("Source name is required.", nameof(sourceName));
        if (string.IsNullOrWhiteSpace(providerType))
            throw new ArgumentException("Provider type is required.", nameof(providerType));

        services.AddSingleton<INamedDataProvider>(new NamedDataProvider(sourceName, providerType, provider));
        return services;
    }

    private sealed record NamedDataProvider(string SourceName, string ProviderType, IDataProvider Provider) : INamedDataProvider;
}
