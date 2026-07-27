namespace KineticReports.Samples.Plugins;

using KineticReports.Core.Plugins;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net;
using System.Text;

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
    public override string Id => "kinetic.sample.watermark";
    public override string Name => "Watermark Plugin";
    public override string Version => "1.0.0";
    public override string? Author => "KineticReports Team";
    public override string? Description => "Adds custom watermark rendering to reports";

    /// <summary>
    /// Gets deterministic execution order for all implemented plugin seams.
    /// </summary>
    public int Order => 100;

    private WatermarkSettings _configuredSettings = WatermarkSettings.Default;

    protected override Task OnInitializeAsync(CancellationToken cancellationToken)
    {
        var configuration = ServiceProvider?.GetService(typeof(IConfiguration)) as IConfiguration;
        _configuredSettings = WatermarkSettings.FromConfiguration(configuration);

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
        var settings = WatermarkSettingsStore.Resolve(_configuredSettings);

        if (!settings.Enabled || string.IsNullOrWhiteSpace(html))
            return ValueTask.FromResult(html);

        if (html.Contains("id=\"kr-watermark-style\"", StringComparison.Ordinal))
            return ValueTask.FromResult(html);

        var watermarkStyle = BuildWatermarkStyle(settings);
        var watermarkContent = BuildWatermarkContent(settings);
        var watermarkDiv = $"<div class=\"kr-watermark\" aria-hidden=\"true\">{watermarkContent}</div>";

        var withStyle = html.Contains("</head>", StringComparison.OrdinalIgnoreCase)
            ? html.Replace("</head>", watermarkStyle + "</head>", StringComparison.OrdinalIgnoreCase)
            : watermarkStyle + html;

        var withWatermark = withStyle.Contains("</body>", StringComparison.OrdinalIgnoreCase)
            ? withStyle.Replace("</body>", watermarkDiv + "</body>", StringComparison.OrdinalIgnoreCase)
            : withStyle + watermarkDiv;

        return ValueTask.FromResult(withWatermark);
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

            var markdown = Encoding.UTF8.GetString(artifact);
            if (markdown.Contains("processed-by:kinetic.sample.watermark", StringComparison.Ordinal))
            {
                return Task.FromResult(artifact);
            }

            markdown += "\n\n<!-- processed-by:kinetic.sample.watermark -->";
            return Task.FromResult(Encoding.UTF8.GetBytes(markdown));
        }

        var html = Encoding.UTF8.GetString(artifact);
        if (html.Contains("<!-- processed-by:kinetic.sample.watermark -->", StringComparison.Ordinal))
        {
            return Task.FromResult(artifact);
        }

        html += "\n<!-- processed-by:kinetic.sample.watermark -->";
        return Task.FromResult(Encoding.UTF8.GetBytes(html));
    }

    private static string BuildWatermarkStyle(WatermarkSettings options)
    {
        var styleBuilder = new StringBuilder();
        styleBuilder.Append("<style id=\"kr-watermark-style\">");
        styleBuilder.Append(".kr-watermark{position:fixed;left:50%;top:50%;transform:translate(-50%,-50%) rotate(");
        styleBuilder.Append(options.RotationDegrees.ToString("0.##", CultureInfo.InvariantCulture));
        styleBuilder.Append("deg);opacity:");
        styleBuilder.Append(options.Opacity.ToString("0.###", CultureInfo.InvariantCulture));
        styleBuilder.Append(";pointer-events:none;z-index:2147483647;user-select:none;display:flex;align-items:center;justify-content:center;}");
        styleBuilder.Append(".kr-watermark-text{font-size:");
        styleBuilder.Append(options.FontSizePx.ToString("0.##", CultureInfo.InvariantCulture));
        styleBuilder.Append("px;font-weight:700;letter-spacing:2px;color:");
        styleBuilder.Append(options.TextColorHex);
        styleBuilder.Append(";font-family:");
        styleBuilder.Append(options.FontFamily);
        styleBuilder.Append(";white-space:nowrap;}");
        styleBuilder.Append(".kr-watermark-image{max-width:");
        styleBuilder.Append(options.ImageMaxWidthPx.ToString("0.##", CultureInfo.InvariantCulture));
        styleBuilder.Append("px;max-height:");
        styleBuilder.Append(options.ImageMaxHeightPx.ToString("0.##", CultureInfo.InvariantCulture));
        styleBuilder.Append("px;object-fit:contain;}");
        styleBuilder.Append("</style>");

        return styleBuilder.ToString();
    }

    private static string BuildWatermarkContent(WatermarkSettings options)
    {
        if (!string.IsNullOrWhiteSpace(options.ImageUrl))
        {
            var encodedUrl = WebUtility.HtmlEncode(options.ImageUrl);
            return $"<img class=\"kr-watermark-image\" src=\"{encodedUrl}\" alt=\"\" />";
        }

        var encodedText = WebUtility.HtmlEncode(options.Message);
        return $"<span class=\"kr-watermark-text\">{encodedText}</span>";
    }

}
