namespace KineticReports.Samples.Blazor.DependencyInjection;

using KineticReports.Core.Authoring.DependencyInjection;
using KineticReports.Core.Data.DependencyInjection;
using KineticReports.Core.Engine;
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Engine.Data;
using KineticReports.Core.Engine.DependencyInjection;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.Export.Html;
using KineticReports.Core.Plugins;
using KineticReports.Core.Rendering.Skia;
using KineticReports.Core.Typography;
using KineticReports.Core.Visual;
using KineticReports.Samples.Blazor.Services;
using KineticReports.Viewer.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ViewerRenderingPipelineOptions = KineticReports.Viewer.Blazor.Services.RenderingPipelineOptions;

/// <summary>
/// Registers the default KineticReports sample stack with a single entrypoint.
/// </summary>
public static class KeneticReportsServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default KineticReports registrations used by the Blazor sample.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional callback to override default options.</param>
    /// <returns>A fluent builder that can extend or replace defaults.</returns>
    public static KeneticReportsBuilder AddKeneticReports(
        this IServiceCollection services,
        Action<KeneticReportsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new KeneticReportsOptions();
        configure?.Invoke(options);

        services.AddKineticReportsAuthoring();

        services.AddScoped<IPluginService, PluginService>();
        services.AddScoped<IPluginManager, PluginServicePluginManagerAdapter>();

        services.AddKineticReportsEngine(configureBuilder: options.ConfigureDefaultReportBuilder);
        services.RemoveAll<IExpressionEvaluator>();
        services.AddScoped<IExpressionEvaluator, DefaultExpressionEvaluator>();

        services.AddScoped<ITextLayout, SkiaTextLayout>();
        services.AddOptions<HtmlExportOptions>();

        services.AddScoped<IHtmlExporter, HtmlExporter>();
        services.AddScoped<IVisualHtmlExporter, VisualHtmlExporter>();
        services.AddScoped<IVisualDocumentBuilder, DefaultVisualDocumentBuilder>();

        return new KeneticReportsBuilder(services, options);
    }
}

/// <summary>
/// Options for <see cref="KeneticReportsServiceCollectionExtensions.AddKeneticReports"/>.
/// </summary>
public sealed class KeneticReportsOptions
{
    /// <summary>
    /// Gets or sets an optional callback for configuring the default <see cref="DefaultReportBuilder"/>.
    /// </summary>
    public Action<DefaultReportBuilder>? ConfigureDefaultReportBuilder { get; set; }

    /// <summary>
    /// Gets or sets an optional callback for advanced viewer pipeline option customization.
    /// </summary>
    public Action<ViewerRenderingPipelineOptions>? ConfigureViewerOptions { get; set; }
}

/// <summary>
/// Fluent builder used to extend or replace default AddKeneticReports registrations.
/// </summary>
public sealed class KeneticReportsBuilder
{
    private readonly KeneticReportsOptions options;

    internal KeneticReportsBuilder(IServiceCollection services, KeneticReportsOptions options)
    {
        Services = services;
        this.options = options;
    }

    /// <summary>
    /// Gets the underlying service collection for advanced customization.
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Replaces the default <see cref="IDataResolver"/> registration.
    /// </summary>
    public KeneticReportsBuilder ReplaceDataResolver<TImplementation>()
        where TImplementation : class, IDataResolver
    {
        Services.RemoveAll<IDataResolver>();
        Services.AddScoped<IDataResolver, TImplementation>();
        return this;
    }

    /// <summary>
    /// Adds an additional data-source resolver contribution.
    /// </summary>
    public KeneticReportsBuilder AddDataResolver<TImplementation>()
        where TImplementation : class, IDataSourceResolver
    {
        Services.TryAddEnumerable(ServiceDescriptor.Scoped<IDataSourceResolver, TImplementation>());
        return this;
    }

    /// <summary>
    /// Registers a SQL Server data provider mapped to provider type <c>SqlServer</c>.
    /// </summary>
    public KeneticReportsBuilder AddSqlServerDataProvider(string connectionString)
    {
        Services.AddSqlServerDataProvider(connectionString);
        return this;
    }

    /// <summary>
    /// Registers a SQLite data provider mapped to provider type <c>SqlLite</c>/<c>SQLite</c>.
    /// </summary>
    public KeneticReportsBuilder AddSqlLiteDataProvider(string connectionString)
    {
        Services.AddSqlLiteDataProvider(connectionString);
        return this;
    }

    /// <summary>
    /// Replaces the default <see cref="IReportBuilder"/> registration.
    /// </summary>
    public KeneticReportsBuilder ReplaceReportBuilder<TImplementation>()
        where TImplementation : class, IReportBuilder
    {
        Services.RemoveAll<IReportBuilder>();
        Services.AddScoped<IReportBuilder, TImplementation>();
        return this;
    }

    /// <summary>
    /// Replaces the default <see cref="ITextLayout"/> registration.
    /// </summary>
    public KeneticReportsBuilder ReplaceTextLayout<TImplementation>()
        where TImplementation : class, ITextLayout
    {
        Services.RemoveAll<ITextLayout>();
        Services.AddScoped<ITextLayout, TImplementation>();
        return this;
    }

    /// <summary>
    /// Adds the Blazor viewer services.
    /// </summary>
    /// <param name="configure">Optional callback to configure viewer pipeline options.</param>
    public KeneticReportsBuilder AddBlazorViewer(Action<ViewerRenderingPipelineOptions>? configure = null)
    {
        Services.AddKineticReportsViewerBlazor(viewerOptions =>
        {
            options.ConfigureViewerOptions?.Invoke(viewerOptions);
            configure?.Invoke(viewerOptions);
        });

        return this;
    }

    /// <summary>
    /// Configures the HTML exporter stylesheet link for the sample host.
    /// </summary>
    public KeneticReportsBuilder ConfigureHtmlExporter(Action<HtmlExportOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        Services.Configure(configure);
        return this;
    }

    /// <summary>
    /// Adds viewer services using a custom registration callback.
    /// This can be used to register MVC viewer services or any custom implementation.
    /// </summary>
    /// <param name="configure">Viewer service registration callback.</param>
    public KeneticReportsBuilder AddViewer(Action<IServiceCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(Services);
        return this;
    }

    /// <summary>
    /// Applies additional viewer pipeline options.
    /// </summary>
    public KeneticReportsBuilder ConfigureViewer(Action<ViewerRenderingPipelineOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        Services.Configure(configure);
        return this;
    }

    /// <summary>
    /// Adds an additional scoped registration.
    /// </summary>
    public KeneticReportsBuilder AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Services.AddScoped<TService, TImplementation>();
        return this;
    }

    /// <summary>
    /// Adds an additional singleton registration.
    /// </summary>
    public KeneticReportsBuilder AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        Services.AddSingleton<TService, TImplementation>();
        return this;
    }
}
