namespace KineticReports.Samples.Plugins;

using KineticReports.Core.Plugins;

/// <summary>
/// Sample plugin demonstrating custom watermark rendering.
/// Shows how to create a simple plugin that extends report rendering capabilities.
/// </summary>
public sealed class WatermarkPlugin :
    PluginBase,
    IHtmlReportPostProcessorPlugin,
    IExportFormatRegistryPlugin,
    IExportNegotiationPlugin,
    IExportArtifactPostProcessorPlugin
{
    public override string Id => "kinetic.watermark";
    public override string Name => "Watermark Plugin";
    public override string Version => "1.0.0";
    public override string? Author => "KineticReports Team";
    public override string? Description => "Adds custom watermark rendering to reports";

    /// <summary>
    /// Gets deterministic execution order for all implemented plugin seams.
    /// </summary>
    public int Order => 100;

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        // Example: Register custom services in the DI container
        // ServiceProvider?.GetRequiredService<IServiceCollection>().AddScoped<IWatermarkRenderer, CustomWatermarkRenderer>();

        System.Diagnostics.Debug.WriteLine($"Initialized: {Name} v{Version}");
        return Task.CompletedTask;
    }

    protected override Task OnUnloadAsync(CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine($"Unloading: {Name}");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask<string> ProcessHtmlAsync(string html, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(html))
            return ValueTask.FromResult(html);

        const string watermarkStyle = "<style id=\"kr-watermark-style\">.kinetic-page.page-block{position:relative;overflow:hidden;}.kinetic-page.page-block::after{content:\"KineticReports\";position:absolute;inset:0;display:flex;align-items:center;justify-content:center;transform:rotate(-30deg);font-size:72px;font-weight:700;letter-spacing:2px;color:rgba(0,0,0,0.08);pointer-events:none;user-select:none;z-index:5;}</style>";

        if (html.Contains("id=\"kr-watermark-style\"", StringComparison.Ordinal))
            return ValueTask.FromResult(html);

        var withStyle = html.Contains("</head>", StringComparison.OrdinalIgnoreCase)
            ? html.Replace("</head>", watermarkStyle + "</head>", StringComparison.OrdinalIgnoreCase)
            : watermarkStyle + html;

        return ValueTask.FromResult(withStyle);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ExportFormatDescriptor> GetFormats()
    {
        return
        [
            new ExportFormatDescriptor(
                "markdown",
                "Markdown Alias",
                "text/markdown",
                "md")
        ];
    }

    /// <inheritdoc/>
    public string? Negotiate(string requestedFormat, IReadOnlyList<ExportFormatDescriptor> availableFormats)
    {
        if (string.Equals(requestedFormat, "markdown", StringComparison.OrdinalIgnoreCase))
        {
            // Alias to the concrete markdown exporter format ID.
            return "md";
        }

        return null;
    }

    /// <inheritdoc/>
    public Task<byte[]> ProcessArtifactAsync(
        string formatId,
        byte[] artifact,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(formatId, "html", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(formatId, "md", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(artifact);
            }

            var markdown = System.Text.Encoding.UTF8.GetString(artifact);
            if (markdown.Contains("processed-by:kinetic.watermark", StringComparison.Ordinal))
            {
                return Task.FromResult(artifact);
            }

            markdown += "\n\n<!-- processed-by:kinetic.watermark -->";
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(markdown));
        }

        var html = System.Text.Encoding.UTF8.GetString(artifact);
        if (html.Contains("<!-- processed-by:kinetic.watermark -->", StringComparison.Ordinal))
        {
            return Task.FromResult(artifact);
        }

        html += "\n<!-- processed-by:kinetic.watermark -->";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(html));
    }
}
