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
    /// Registers a singleton <see cref="PocoDataProvider"/> and exposes it as both
    /// <see cref="ILinqDataProvider"/> and <see cref="IDataProvider"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback used to seed/modify the provider instance at startup.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddPocoDataProvider(
        this IServiceCollection services,
        Action<PocoDataProvider>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<PocoDataProvider>(_ =>
        {
            var provider = new PocoDataProvider();
            configure?.Invoke(provider);
            return provider;
        });

        services.TryAddSingleton<ILinqDataProvider>(provider => provider.GetRequiredService<PocoDataProvider>());

        // Register as IDataProvider without replacing existing providers.
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDataProvider>(
            provider => provider.GetRequiredService<PocoDataProvider>()));

        AddNamedProviderAlias<PocoDataProvider>(services, "Poco");
        AddNamedProviderAlias<PocoDataProvider>(services, "InMemory");
        AddNamedProviderAlias<PocoDataProvider>(services, "SampleInMemory");

        return services;
    }

    /// <summary>
    /// Registers a singleton <see cref="SqlServerDataProvider"/> and maps it to provider type <c>SqlServer</c>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="connectionString">SQL Server connection string.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddSqlServerDataProvider(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        services.TryAddSingleton<SqlServerDataProvider>(_ => new SqlServerDataProvider(connectionString));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDataProvider>(
            provider => provider.GetRequiredService<SqlServerDataProvider>()));

        AddNamedProviderAlias<SqlServerDataProvider>(services, "SqlServer");
        AddNamedProviderAlias<SqlServerDataProvider>(services, "SQL Server");

        return services;
    }

    /// <summary>
    /// Registers a singleton <see cref="SqlLiteDataProvider"/> and maps it to provider type <c>SqlLite</c>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="connectionString">SQLite connection string.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddSqlLiteDataProvider(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        services.TryAddSingleton<SqlLiteDataProvider>(_ => new SqlLiteDataProvider(connectionString));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IDataProvider>(
            provider => provider.GetRequiredService<SqlLiteDataProvider>()));

        AddNamedProviderAlias<SqlLiteDataProvider>(services, "SqlLite");
        AddNamedProviderAlias<SqlLiteDataProvider>(services, "SQLite");

        return services;
    }

    /// <summary>
    /// Registers a provider-type alias for an already-registered <see cref="IDataProvider"/> implementation.
    /// </summary>
    /// <typeparam name="TProvider">Provider implementation type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="providerType">Provider type key matched from report definitions.</param>
    /// <returns>The input service collection.</returns>
    public static IServiceCollection AddDataProviderAlias<TProvider>(
        this IServiceCollection services,
        string providerType)
        where TProvider : class, IDataProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        AddNamedProviderAlias<TProvider>(services, providerType);
        return services;
    }

    private static void AddNamedProviderAlias<TProvider>(IServiceCollection services, string providerType)
        where TProvider : class, IDataProvider
    {
        if (string.IsNullOrWhiteSpace(providerType))
            return;

        services.TryAddEnumerable(ServiceDescriptor.Singleton<INamedDataProvider>(provider =>
            new NamedDataProvider(providerType, provider.GetRequiredService<TProvider>())));
    }

    private sealed record NamedDataProvider(string ProviderType, IDataProvider Provider) : INamedDataProvider;
}
