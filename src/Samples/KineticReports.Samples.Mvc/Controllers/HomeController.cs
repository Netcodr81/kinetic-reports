using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using KineticReports.Core.Definition;
using KineticReports.Core.Engine;
using KineticReports.Core.Viewer.Services;
using KineticReports.Samples.Mvc.Models;
using KineticReports.Samples.Mvc.Services;
using KineticReports.Samples.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace KineticReports.Samples.Mvc.Controllers;

public sealed class HomeController : Controller
{
    private static readonly IReadOnlySet<string> LayoutItemTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        nameof(ReportLayoutItemKind.Text),
        nameof(ReportLayoutItemKind.Barcode),
        nameof(ReportLayoutItemKind.Table),
        nameof(ReportLayoutItemKind.PageBreak)
    };

    private const string WatermarkPluginId = "kinetic.watermark";

    private readonly ISampleReportCatalogService _reportCatalogService;
    private readonly IReportEngine _reportEngine;
    private readonly IReportService _reportService;

    public HomeController(
        ISampleReportCatalogService reportCatalogService,
        IReportEngine reportEngine,
        IReportService reportService)
    {
        _reportCatalogService = reportCatalogService ?? throw new ArgumentNullException(nameof(reportCatalogService));
        _reportEngine = reportEngine ?? throw new ArgumentNullException(nameof(reportEngine));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] HomeRequest? request, CancellationToken cancellationToken)
    {
        var model = await BuildPageModelAsync(request ?? new HomeRequest(), autoLoad: true, cancellationToken).ConfigureAwait(false);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(HomePageViewModel form, CancellationToken cancellationToken)
    {
        var request = ToRequest(form);
        var model = await BuildPageModelAsync(request, autoLoad: true, cancellationToken).ConfigureAwait(false);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> DownloadJson([FromQuery] HomeRequest request, CancellationToken cancellationToken)
    {
        var definition = await CreateEffectiveDefinitionAsync(request, cancellationToken).ConfigureAwait(false);
        var json = BuildDisplayDefinitionJson(definition);

        var fileName = string.IsNullOrWhiteSpace(definition.Name)
            ? "report-definition.json"
            : $"{SanitizeFileNameStem(definition.Name)}.json";

        return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Export([FromQuery] HomeRequest request, [FromQuery] string formatId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(formatId))
            return BadRequest("formatId is required.");

        var definition = await CreateEffectiveDefinitionAsync(request, cancellationToken).ConfigureAwait(false);
        var document = await _reportEngine.RunAsync(definition, cancellationToken: cancellationToken).ConfigureAwait(false);
        var exportResult = await _reportService
            .ExportAsync(document, definition, formatId, cancellationToken)
            .ConfigureAwait(false);

        var fileName = string.IsNullOrWhiteSpace(definition.Name)
            ? $"report.{exportResult.FileExtension}"
            : $"{SanitizeFileNameStem(definition.Name)}.{exportResult.FileExtension}";

        return File(exportResult.Content, exportResult.MimeType, fileName);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<HomePageViewModel> BuildPageModelAsync(
        HomeRequest request,
        bool autoLoad,
        CancellationToken cancellationToken)
    {
        var templates = _reportCatalogService.GetTemplates();

        var selectedTemplateId = string.IsNullOrWhiteSpace(request.SelectedTemplateId)
            ? templates.FirstOrDefault()?.Id ?? string.Empty
            : request.SelectedTemplateId;

        var model = new HomePageViewModel
        {
            Templates = templates,
            SelectedTemplateId = selectedTemplateId,
            SelectedProviderType = string.IsNullOrWhiteSpace(request.SelectedProviderType) ? "InMemory" : request.SelectedProviderType,
            IncludePageHeader = request.IncludePageHeader,
            IncludePageFooter = request.IncludePageFooter,
            IncludePageNumbers = request.IncludePageFooter && request.IncludePageNumbers,
            WatermarkPluginEnabled = request.WatermarkPluginEnabled,
            WatermarkEnabled = request.WatermarkEnabled,
            WatermarkMessage = string.IsNullOrWhiteSpace(request.WatermarkMessage) ? "KineticReports" : request.WatermarkMessage,
            WatermarkImageUrl = request.WatermarkImageUrl,
            WatermarkOpacity = request.WatermarkOpacity,
            WatermarkRotationDegrees = request.WatermarkRotationDegrees,
            WatermarkFontSizePx = request.WatermarkFontSizePx,
            WatermarkTextColorHex = string.IsNullOrWhiteSpace(request.WatermarkTextColorHex) ? "#000000" : request.WatermarkTextColorHex
        };

        if (!autoLoad)
            return model;

        try
        {
            var effectiveRequest = ToRequest(model);
            var effectiveDefinition = await CreateEffectiveDefinitionAsync(effectiveRequest, cancellationToken).ConfigureAwait(false);
            var reportDocument = await _reportEngine.RunAsync(effectiveDefinition, cancellationToken: cancellationToken).ConfigureAwait(false);

            model.ActiveDefinition = effectiveDefinition;
            model.ActiveReportDocument = reportDocument;
            model.ActiveReportName = effectiveDefinition.Name;
            model.DefinitionJson = BuildDisplayDefinitionJson(effectiveDefinition);
            model.Status = $"Loaded '{effectiveDefinition.Name}' using provider '{effectiveRequest.SelectedProviderType}' and built {reportDocument.PageCount} page(s).";
            ApplyActionUrls(model, effectiveRequest);
        }
        catch (Exception ex)
        {
            model.Error = ex.Message;
            model.Status = null;
        }

        return model;
    }

    private async Task<ReportDefinition> CreateEffectiveDefinitionAsync(HomeRequest request, CancellationToken cancellationToken)
    {
        var loadedDefinition = await _reportCatalogService
            .LoadAsync(request.SelectedTemplateId, request.SelectedProviderType, cancellationToken)
            .ConfigureAwait(false);

        var definitionWithLayout = ApplyLayoutDisplayOptions(loadedDefinition, request.IncludePageHeader, request.IncludePageFooter, request.IncludePageNumbers);
        var definitionWithPlugins = ApplyPluginToggles(definitionWithLayout, request);

        var watermarkSettings = BuildWatermarkSettingsFromRequest(request);
        WatermarkSettingsStore.SetOverride(watermarkSettings);

        return definitionWithPlugins;
    }

    private void ApplyActionUrls(HomePageViewModel model, HomeRequest request)
    {
        model.DownloadJsonUrl = Url.Action(nameof(DownloadJson), new
        {
            request.SelectedTemplateId,
            request.SelectedProviderType,
            request.IncludePageHeader,
            request.IncludePageFooter,
            request.IncludePageNumbers,
            request.WatermarkPluginEnabled,
            request.WatermarkEnabled,
            request.WatermarkMessage,
            request.WatermarkImageUrl,
            request.WatermarkOpacity,
            request.WatermarkRotationDegrees,
            request.WatermarkFontSizePx,
            request.WatermarkTextColorHex
        });

        model.ExportHtmlUrl = Url.Action(nameof(Export), new
        {
            formatId = "html",
            request.SelectedTemplateId,
            request.SelectedProviderType,
            request.IncludePageHeader,
            request.IncludePageFooter,
            request.IncludePageNumbers,
            request.WatermarkPluginEnabled,
            request.WatermarkEnabled,
            request.WatermarkMessage,
            request.WatermarkImageUrl,
            request.WatermarkOpacity,
            request.WatermarkRotationDegrees,
            request.WatermarkFontSizePx,
            request.WatermarkTextColorHex
        });

        model.ExportPdfUrl = Url.Action(nameof(Export), new
        {
            formatId = "pdf",
            request.SelectedTemplateId,
            request.SelectedProviderType,
            request.IncludePageHeader,
            request.IncludePageFooter,
            request.IncludePageNumbers,
            request.WatermarkPluginEnabled,
            request.WatermarkEnabled,
            request.WatermarkMessage,
            request.WatermarkImageUrl,
            request.WatermarkOpacity,
            request.WatermarkRotationDegrees,
            request.WatermarkFontSizePx,
            request.WatermarkTextColorHex
        });
    }

    private static HomeRequest ToRequest(HomePageViewModel model)
    {
        return new HomeRequest
        {
            SelectedTemplateId = model.SelectedTemplateId,
            SelectedProviderType = model.SelectedProviderType,
            IncludePageHeader = model.IncludePageHeader,
            IncludePageFooter = model.IncludePageFooter,
            IncludePageNumbers = model.IncludePageFooter && model.IncludePageNumbers,
            WatermarkPluginEnabled = model.WatermarkPluginEnabled,
            WatermarkEnabled = model.WatermarkEnabled,
            WatermarkMessage = model.WatermarkMessage,
            WatermarkImageUrl = model.WatermarkImageUrl,
            WatermarkOpacity = model.WatermarkOpacity,
            WatermarkRotationDegrees = model.WatermarkRotationDegrees,
            WatermarkFontSizePx = model.WatermarkFontSizePx,
            WatermarkTextColorHex = model.WatermarkTextColorHex
        };
    }

    private static ReportDefinition ApplyLayoutDisplayOptions(
        ReportDefinition definition,
        bool includeHeader,
        bool includeFooter,
        bool includePageNumbers)
    {
        if (definition.Layout is null)
            return definition;

        var header = includeHeader
            ? definition.Layout.PageHeader.ToList()
            : [];

        var footer = includeFooter
            ? definition.Layout.PageFooter.ToList()
            : [];

        if (includeFooter)
        {
            if (!includePageNumbers)
            {
                footer = footer
                    .Where(item => !IsPageNumberFooterItem(item))
                    .ToList();
            }
            else if (!footer.Any(IsPageNumberFooterItem))
            {
                footer.Add(new ReportLayoutItemDefinition
                {
                    Id = "runtime-page-number",
                    Kind = ReportLayoutItemKind.Text,
                    Text = "Page {PageNumber}"
                });
            }
        }

        return definition with
        {
            Layout = definition.Layout with
            {
                PageHeader = header,
                PageFooter = footer
            }
        };
    }

    private static bool IsPageNumberFooterItem(ReportLayoutItemDefinition item)
    {
        if (item.Kind != ReportLayoutItemKind.Text)
            return false;

        return !string.IsNullOrWhiteSpace(item.Text)
            && item.Text.Contains("{PageNumber}", StringComparison.OrdinalIgnoreCase);
    }

    private static ReportDefinition ApplyPluginToggles(ReportDefinition definition, HomeRequest request)
    {
        var plugins = definition.Plugins
            .Where(toggle => !string.Equals(toggle.PluginId, WatermarkPluginId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var metadata = new Dictionary<string, object>(definition.Metadata, StringComparer.Ordinal)
        {
            ["plugins.kinetic.watermark.enabled"] = request.WatermarkEnabled,
            ["plugins.kinetic.watermark.message"] = string.IsNullOrWhiteSpace(request.WatermarkMessage) ? "KineticReports" : request.WatermarkMessage.Trim(),
            ["plugins.kinetic.watermark.opacity"] = Clamp(request.WatermarkOpacity, 0.01f, 1f),
            ["plugins.kinetic.watermark.rotationDegrees"] = Clamp(request.WatermarkRotationDegrees, -360f, 360f),
            ["plugins.kinetic.watermark.fontSizePx"] = Clamp(request.WatermarkFontSizePx, 10f, 300f),
            ["plugins.kinetic.watermark.textColorHex"] = string.IsNullOrWhiteSpace(request.WatermarkTextColorHex) ? "#000000" : request.WatermarkTextColorHex.Trim()
        };

        plugins.Add(new ReportPluginToggleDefinition
        {
            PluginId = WatermarkPluginId,
            Enabled = request.WatermarkPluginEnabled
        });

        return definition with
        {
            Plugins = plugins,
            Metadata = metadata
        };
    }

    private static WatermarkSettings BuildWatermarkSettingsFromRequest(HomeRequest request)
    {
        return new WatermarkSettings
        {
            Enabled = request.WatermarkEnabled,
            Message = string.IsNullOrWhiteSpace(request.WatermarkMessage) ? "KineticReports" : request.WatermarkMessage.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.WatermarkImageUrl) ? null : request.WatermarkImageUrl.Trim(),
            Opacity = Clamp(request.WatermarkOpacity, 0.01f, 1f),
            RotationDegrees = Clamp(request.WatermarkRotationDegrees, -360f, 360f),
            FontSizePx = Clamp(request.WatermarkFontSizePx, 10f, 300f),
            TextColorHex = string.IsNullOrWhiteSpace(request.WatermarkTextColorHex) ? "#000000" : request.WatermarkTextColorHex.Trim(),
            FontFamily = "Arial, sans-serif",
            ImageMaxWidthPx = 420f,
            ImageMaxHeightPx = 420f
        };
    }

    private static string BuildDisplayDefinitionJson(ReportDefinition definition)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new JsonStringEnumConverter());

        var node = JsonSerializer.SerializeToNode(definition, options)
            ?? throw new InvalidOperationException("Failed to build JSON preview for the active report definition.");

        RenameLayoutItemKindToType(node);

        return node.ToJsonString(options);
    }

    private static void RenameLayoutItemKindToType(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            if (obj.TryGetPropertyValue("kind", out var kindNode)
                && kindNode is JsonValue kindValue
                && kindValue.TryGetValue<string>(out var kindText)
                && LayoutItemTypes.Contains(kindText))
            {
                obj.Remove("kind");
                obj["type"] = kindText;
            }

            foreach (var child in obj.ToList())
            {
                if (child.Value is not null)
                    RenameLayoutItemKindToType(child.Value);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var child in arr)
            {
                if (child is not null)
                    RenameLayoutItemKindToType(child);
            }
        }
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private static string SanitizeFileNameStem(string value)
    {
        var sanitized = value;
        foreach (var invalid in Path.GetInvalidFileNameChars())
            sanitized = sanitized.Replace(invalid, '-');

        return string.IsNullOrWhiteSpace(sanitized) ? "report" : sanitized.Trim();
    }
}
