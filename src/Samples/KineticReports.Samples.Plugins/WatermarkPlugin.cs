namespace KineticReports.Samples.Plugins;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;
using KineticReports.Plugins;

/// <summary>
/// Sample plugin demonstrating custom watermark rendering.
/// Shows how to create a simple plugin that extends report rendering capabilities.
/// </summary>
public sealed class WatermarkPlugin : PluginBase, IHtmlReportPostProcessorPlugin, IReportBandsPostProcessorPlugin
{
    public override string Id => "kinetic.sample.watermark";
    public override string Name => "Watermark Plugin";
    public override string Version => "1.0.0";
    public override string? Author => "KineticReports Team";
    public override string? Description => "Adds custom watermark rendering to reports";

    /// <inheritdoc/>
    public int Order => 100;

    /// <inheritdoc/>
    public IReadOnlyList<BandElement> ProcessBands(IReadOnlyList<BandElement> bands)
    {
        if (bands == null) throw new ArgumentNullException(nameof(bands));

        var result = bands.ToList();
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

    private static BandElement CreateFooterBand()
    {
        var bandStyle = new ResolvedStyle
        {
            FontFamily = "Arial",
            FontSize = 10f,
            TextColor = Color.FromRgb(110, 110, 110)
        };

        return new BandElement
        {
            Id = "plugin-watermark-footer",
            Kind = BandKind.PageFooter,
            Style = bandStyle,
            Children =
            [
                new TextElement
                {
                    Id = "plugin-watermark-footer-text",
                    Style = bandStyle,
                    Text = "Watermark plugin seam active"
                }
            ]
        };
    }
}
