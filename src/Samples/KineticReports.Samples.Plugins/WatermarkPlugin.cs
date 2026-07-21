namespace KineticReports.Samples.Plugins;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Plugins;

/// <summary>
/// Sample plugin demonstrating custom watermark rendering.
/// Shows how to create a simple plugin that extends report rendering capabilities.
/// </summary>
public sealed class WatermarkPlugin :
    PluginBase,
    IHtmlReportPostProcessorPlugin,
    IReportBlocksPostProcessorPlugin,
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

    /// <inheritdoc/>
    public IReadOnlyList<ReportBlock> ProcessBlocks(IReadOnlyList<ReportBlock> blocks)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));

        var result = blocks.ToList();
        result.Add(CreateFooterBand());
        return result;
    }

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

        const string watermarkStyle = "<style id=\"kr-watermark-style\">.kr-watermark{position:fixed;left:50%;top:50%;transform:translate(-50%,-50%) rotate(-30deg);font-size:72px;font-weight:700;letter-spacing:2px;color:rgba(0,0,0,0.08);pointer-events:none;z-index:2147483647;user-select:none;}</style>";
        const string watermarkDiv = "<div class=\"kr-watermark\" aria-hidden=\"true\">KineticReports</div>";

        if (html.Contains("id=\"kr-watermark-style\"", StringComparison.Ordinal))
            return ValueTask.FromResult(html);

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

            var markdown = System.Text.Encoding.UTF8.GetString(artifact);
            if (markdown.Contains("processed-by:kinetic.sample.watermark", StringComparison.Ordinal))
            {
                return Task.FromResult(artifact);
            }

            markdown += "\n\n<!-- processed-by:kinetic.sample.watermark -->";
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(markdown));
        }

        var html = System.Text.Encoding.UTF8.GetString(artifact);
        if (html.Contains("<!-- processed-by:kinetic.sample.watermark -->", StringComparison.Ordinal))
        {
            return Task.FromResult(artifact);
        }

        html += "\n<!-- processed-by:kinetic.sample.watermark -->";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(html));
    }

    private static PageFooterBlock CreateFooterBand()
    {
        var bandStyle = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 10f,
            TextColor = Color.FromRgb(110, 110, 110)
        };

        return new PageFooterBlock
        {
            Id = "plugin-watermark-footer",
            Style = bandStyle,
            Children =
            [
                new TextBlock
                {
                    Id = "plugin-watermark-footer-text",
                    Style = bandStyle,
                    Text = "Watermark plugin seam active"
                }
            ]
        };
    }
}
