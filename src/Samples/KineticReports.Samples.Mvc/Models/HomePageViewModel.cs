namespace KineticReports.Samples.Mvc.Models;

using KineticReports.Core.Definition;
using KineticReports.Core.Layout;
using KineticReports.Samples.Mvc.Services;
using KineticReports.Viewer.Mvc;

/// <summary>
/// MVC page model backing the sample report viewer workflow.
/// </summary>
public sealed class HomePageViewModel
{
    public IReadOnlyList<SampleReportTemplateDescriptor> Templates { get; set; } = [];

    public IReadOnlyList<ProviderOption> Providers { get; set; } =
    [
        new ProviderOption("InMemory", "In-Memory (Poco provider)"),
        new ProviderOption("SqlLite", "SQLite")
    ];

    public string SelectedTemplateId { get; set; } = string.Empty;

    public string SelectedProviderType { get; set; } = "InMemory";

    public bool IncludePageHeader { get; set; } = true;

    public bool IncludePageFooter { get; set; } = true;

    public bool IncludePageNumbers { get; set; } = true;

    public bool WatermarkPluginEnabled { get; set; } = true;

    public bool WatermarkEnabled { get; set; } = true;

    public string WatermarkMessage { get; set; } = "KineticReports";

    public string WatermarkImageUrl { get; set; } = string.Empty;

    public float WatermarkOpacity { get; set; } = 0.12f;

    public float WatermarkRotationDegrees { get; set; } = -30f;

    public float WatermarkFontSizePx { get; set; } = 72f;

    public string WatermarkTextColorHex { get; set; } = "#000000";

    public string? ActiveReportName { get; set; }

    public string? DefinitionJson { get; set; }

    public string? Error { get; set; }

    public string? Status { get; set; }

    public ReportDefinition? ActiveDefinition { get; set; }

    public ReportDocument? ActiveReportDocument { get; set; }

    public string? DownloadJsonUrl { get; set; }

    public string? ExportHtmlUrl { get; set; }

    public string? ExportPdfUrl { get; set; }

    public string? SelectedTemplateDescription => Templates
        .FirstOrDefault(template => string.Equals(template.Id, SelectedTemplateId, StringComparison.Ordinal))
        ?.Description;

    public bool CanExport => ActiveDefinition is not null && ActiveReportDocument is not null;

    public KineticReportViewerOptions BuildViewerOptions()
    {
        return new KineticReportViewerOptions
        {
            Density = ReportViewerDensity.Comfortable,
            Theme = ReportViewerTheme.Light,
            AccentColor = "#2563eb",
            SurfaceColor = "#f8fafc",
            BorderColor = "#dbe2ea",
            PageBackgroundColor = "#ffffff",
            ExportHtmlUrl = ExportHtmlUrl,
            ExportPdfUrl = ExportPdfUrl,
            ShowToolbar = true
        };
    }

    public sealed record ProviderOption(string ProviderType, string DisplayName);
}

/// <summary>
/// Bound request shape used by load/export/download endpoints.
/// </summary>
public sealed class HomeRequest
{
    public string SelectedTemplateId { get; set; } = string.Empty;

    public string SelectedProviderType { get; set; } = "InMemory";

    public bool IncludePageHeader { get; set; } = true;

    public bool IncludePageFooter { get; set; } = true;

    public bool IncludePageNumbers { get; set; } = true;

    public bool WatermarkPluginEnabled { get; set; } = true;

    public bool WatermarkEnabled { get; set; } = true;

    public string WatermarkMessage { get; set; } = "KineticReports";

    public string WatermarkImageUrl { get; set; } = string.Empty;

    public float WatermarkOpacity { get; set; } = 0.12f;

    public float WatermarkRotationDegrees { get; set; } = -30f;

    public float WatermarkFontSizePx { get; set; } = 72f;

    public string WatermarkTextColorHex { get; set; } = "#000000";
}
