namespace KineticReports.Samples.Plugins;

using System.Globalization;
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

        var settings = WatermarkSettingsStore.Resolve(WatermarkSettings.Default);
        if (!settings.Enabled)
            return ValueTask.FromResult(html);

        var opacity = Clamp(settings.Opacity, 0.01f, 1f).ToString("0.###", CultureInfo.InvariantCulture);
        var rotation = Clamp(settings.RotationDegrees, -360f, 360f).ToString("0.###", CultureInfo.InvariantCulture);
        var fontSize = Clamp(settings.FontSizePx, 10f, 300f).ToString("0.###", CultureInfo.InvariantCulture);
        var textColorHex = string.IsNullOrWhiteSpace(settings.TextColorHex) ? "#000000" : settings.TextColorHex.Trim();

        string overlayCss;
        if (!string.IsNullOrWhiteSpace(settings.ImageUrl))
        {
            var imageUrl = EscapeCssString(settings.ImageUrl.Trim());
            var imageMaxWidth = Clamp(settings.ImageMaxWidthPx, 20f, 2400f).ToString("0.###", CultureInfo.InvariantCulture);
            var imageMaxHeight = Clamp(settings.ImageMaxHeightPx, 20f, 2400f).ToString("0.###", CultureInfo.InvariantCulture);

            overlayCss = string.Create(CultureInfo.InvariantCulture,
                $"content:\"\";transform:rotate({rotation}deg);opacity:{opacity};background-image:url(\"{imageUrl}\");background-repeat:no-repeat;background-position:center;background-size:contain;max-width:{imageMaxWidth}px;max-height:{imageMaxHeight}px;");
        }
        else
        {
            var message = EscapeCssString(string.IsNullOrWhiteSpace(settings.Message) ? "KineticReports" : settings.Message.Trim());
            var fontFamily = EscapeCssString(string.IsNullOrWhiteSpace(settings.FontFamily) ? "Arial, sans-serif" : settings.FontFamily.Trim());

            overlayCss = string.Create(CultureInfo.InvariantCulture,
                $"content:\"{message}\";transform:rotate({rotation}deg);font-size:{fontSize}px;font-weight:700;letter-spacing:2px;color:{textColorHex};opacity:{opacity};font-family:{fontFamily};");
        }

        var watermarkStyle = string.Create(CultureInfo.InvariantCulture,
            $"<style id=\"kr-watermark-style\">.kinetic-page.page-block{{position:relative;overflow:hidden;}}.kinetic-page.page-block::after{{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;pointer-events:none;user-select:none;z-index:5;{overlayCss}}}</style>");

        if (html.Contains("id=\"kr-watermark-style\"", StringComparison.Ordinal))
            return ValueTask.FromResult(html);

        var withStyle = html.Contains("</head>", StringComparison.OrdinalIgnoreCase)
            ? html.Replace("</head>", watermarkStyle + "</head>", StringComparison.OrdinalIgnoreCase)
            : watermarkStyle + html;

        return ValueTask.FromResult(withStyle);
    }

    private static float Clamp(float value, float min, float max)
        => Math.Min(max, Math.Max(min, value));

    private static string EscapeCssString(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

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
