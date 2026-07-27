namespace KineticReports.Samples.Plugins;

using Microsoft.Extensions.Configuration;
using System.Globalization;

/// <summary>
/// Runtime watermark settings used by the sample watermark plugin.
/// </summary>
public sealed record WatermarkSettings
{
    /// <summary>
    /// Gets the default settings.
    /// </summary>
    public static WatermarkSettings Default => new();

    /// <summary>
    /// Enables or disables watermark injection.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Watermark text. Used when <see cref="ImageUrl"/> is empty.
    /// </summary>
    public string Message { get; init; } = "KineticReports";

    /// <summary>
    /// Optional watermark image URL. When present, image is used instead of text.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Overlay opacity in the inclusive range [0.01, 1.0].
    /// </summary>
    public float Opacity { get; init; } = 0.12f;

    /// <summary>
    /// Clockwise rotation in degrees.
    /// </summary>
    public float RotationDegrees { get; init; } = -30f;

    /// <summary>
    /// Text watermark font size in pixels.
    /// </summary>
    public float FontSizePx { get; init; } = 72f;

    /// <summary>
    /// Text watermark color as hex.
    /// </summary>
    public string TextColorHex { get; init; } = "#000000";

    /// <summary>
    /// Text watermark font family CSS value.
    /// </summary>
    public string FontFamily { get; init; } = "Arial, sans-serif";

    /// <summary>
    /// Maximum image width in pixels.
    /// </summary>
    public float ImageMaxWidthPx { get; init; } = 420f;

    /// <summary>
    /// Maximum image height in pixels.
    /// </summary>
    public float ImageMaxHeightPx { get; init; } = 420f;

    /// <summary>
    /// Loads watermark settings from KineticReports:Watermark configuration.
    /// </summary>
    public static WatermarkSettings FromConfiguration(IConfiguration? configuration)
    {
        if (configuration == null)
            return Default;

        var section = configuration.GetSection("KineticReports:Watermark");
        if (!section.Exists())
            return Default;

        return new WatermarkSettings
        {
            Enabled = ParseBool(section["Enabled"], true),
            Message = string.IsNullOrWhiteSpace(section["Message"]) ? "KineticReports" : section["Message"]!,
            ImageUrl = NormalizeNull(section["ImageUrl"]),
            Opacity = Clamp(ParseFloat(section["Opacity"], 0.12f), 0.01f, 1f),
            RotationDegrees = ParseFloat(section["RotationDegrees"], -30f),
            FontSizePx = Clamp(ParseFloat(section["FontSizePx"], 72f), 10f, 300f),
            TextColorHex = string.IsNullOrWhiteSpace(section["TextColorHex"]) ? "#000000" : section["TextColorHex"]!,
            FontFamily = string.IsNullOrWhiteSpace(section["FontFamily"]) ? "Arial, sans-serif" : section["FontFamily"]!,
            ImageMaxWidthPx = Clamp(ParseFloat(section["ImageMaxWidthPx"], 420f), 20f, 2400f),
            ImageMaxHeightPx = Clamp(ParseFloat(section["ImageMaxHeightPx"], 420f), 20f, 2400f)
        };
    }

    private static bool ParseBool(string? value, bool fallback)
        => bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static float ParseFloat(string? value, float fallback)
        => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;

    private static float Clamp(float value, float min, float max)
        => Math.Min(max, Math.Max(min, value));

    private static string? NormalizeNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}

/// <summary>
/// Thread-safe runtime override store for watermark settings.
/// </summary>
public static class WatermarkSettingsStore
{
    private static readonly object SyncRoot = new();
    private static WatermarkSettings? _runtimeOverride;

    /// <summary>
    /// Sets runtime override settings.
    /// </summary>
    public static void SetOverride(WatermarkSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (SyncRoot)
            _runtimeOverride = settings;
    }

    /// <summary>
    /// Clears runtime override settings.
    /// </summary>
    public static void ClearOverride()
    {
        lock (SyncRoot)
            _runtimeOverride = null;
    }

    /// <summary>
    /// Resolves effective settings from runtime override or fallback.
    /// </summary>
    public static WatermarkSettings Resolve(WatermarkSettings fallback)
    {
        lock (SyncRoot)
            return _runtimeOverride ?? fallback;
    }
}
