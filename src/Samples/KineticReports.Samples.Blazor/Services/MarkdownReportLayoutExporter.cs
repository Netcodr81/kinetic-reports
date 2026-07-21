namespace KineticReports.Samples.Blazor.Services;

using System.Text;
using KineticReports.Core.Layout;
using KineticReports.Plugins;

/// <summary>
/// Exports report layout content to a simple markdown artifact.
/// </summary>
public sealed class MarkdownReportLayoutExporter : IReportLayoutExporter
{
    /// <inheritdoc/>
    public ExportFormatDescriptor Format => new("md", "Markdown", "text/markdown", "md");

    /// <inheritdoc/>
    public Task<byte[]> ExportAsync(ReportLayout reportLayout, CancellationToken cancellationToken = default)
    {
        if (reportLayout == null) throw new ArgumentNullException(nameof(reportLayout));

        var builder = new StringBuilder();
        builder.AppendLine("# KineticReports Export");
        builder.AppendLine();
        builder.AppendLine($"- Page count: {reportLayout.PageCount}");
        builder.AppendLine();

        foreach (var page in reportLayout.Pages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            builder.AppendLine($"## Page {page.PageNumber}");
            builder.AppendLine($"- Size: {page.PageWidth:F1} x {page.PageHeight:F1} DIP");
            builder.AppendLine($"- Body elements: {page.Children.Count}");

            if (page.Header != null)
            {
                builder.AppendLine($"- Header elements: {page.Header.Children.Count}");
            }

            if (page.Footer != null)
            {
                builder.AppendLine($"- Footer elements: {page.Footer.Children.Count}");
            }

            builder.AppendLine();
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(builder.ToString()));
    }
}
