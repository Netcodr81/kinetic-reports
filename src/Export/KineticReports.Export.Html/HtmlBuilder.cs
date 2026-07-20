namespace KineticReports.Export.Html;

using System.Net;
using System.Text;

/// <summary>
/// Helper for building semantic HTML elements.
/// </summary>
public sealed class HtmlBuilder
{
    private readonly StringBuilder _html = new();

    /// <summary>
    /// Appends an opening tag.
    /// </summary>
    public HtmlBuilder OpenTag(string tag, string? style = null, string? id = null, string? classAttr = null, Dictionary<string, string>? attributes = null)
    {
        _html.Append($"<{tag}");
        if (id != null) _html.Append($" id=\"{EscapeAttribute(id)}\"");
        if (classAttr != null) _html.Append($" class=\"{EscapeAttribute(classAttr)}\"");
        if (style != null) _html.Append($" style=\"{EscapeAttribute(style)}\"");
        if (attributes != null)
            foreach (var (k, v) in attributes)
                _html.Append($" {k}=\"{EscapeAttribute(v)}\"");
        _html.Append(">");
        return this;
    }

    /// <summary>
    /// Appends a closing tag.
    /// </summary>
    public HtmlBuilder CloseTag(string tag)
    {
        _html.Append($"</{tag}>");
        return this;
    }

    /// <summary>
    /// Appends text content (HTML-escaped).
    /// </summary>
    public HtmlBuilder Text(string? content)
    {
        if (content != null)
            _html.Append(WebUtility.HtmlEncode(content));
        return this;
    }

    /// <summary>
    /// Appends raw HTML (not escaped).
    /// </summary>
    public HtmlBuilder Raw(string html)
    {
        _html.Append(html);
        return this;
    }

    /// <summary>
    /// Appends a self-closing tag.
    /// </summary>
    public HtmlBuilder VoidTag(string tag, string? style = null, string? id = null, Dictionary<string, string>? attributes = null)
    {
        _html.Append($"<{tag}");
        if (id != null) _html.Append($" id=\"{EscapeAttribute(id)}\"");
        if (style != null) _html.Append($" style=\"{EscapeAttribute(style)}\"");
        if (attributes != null)
            foreach (var (k, v) in attributes)
                _html.Append($" {k}=\"{EscapeAttribute(v)}\"");
        _html.Append(" />");
        return this;
    }

    /// <summary>
    /// Returns the generated HTML string.
    /// </summary>
    public string Build() => _html.ToString();

    private static string EscapeAttribute(string text) =>
        text.Replace("\"", "&quot;").Replace("'", "&#39;");
}
