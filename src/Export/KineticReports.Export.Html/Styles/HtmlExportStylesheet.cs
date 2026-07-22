namespace KineticReports.Export.Html.Styles;

using System.Reflection;

/// <summary>
/// Provides the default stylesheet used by <see cref="HtmlExporter"/>.
/// </summary>
internal static class HtmlExportStylesheet
{
    private const string ResourceName = "KineticReports.Export.Html.Styles.kinetic-report.css";

    private static readonly Lazy<string> Cached = new(LoadCss);

    /// <summary>
    /// Gets the stylesheet text.
    /// </summary>
    public static string Css => Cached.Value;

    private static string LoadCss()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"Embedded stylesheet resource '{ResourceName}' was not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
