namespace KineticReports.Export.Html.Tests;

using KineticReports.Export.Html;

public class HtmlBuilderTests
{
    [Fact]
    public void OpenTag_AddsOpeningTag()
    {
        var builder = new HtmlBuilder();
        var result = builder.OpenTag("div").Build();

        result.ShouldBe("<div>");
    }

    [Fact]
    public void CloseTag_AddsClosingTag()
    {
        var builder = new HtmlBuilder();
        var result = builder.OpenTag("div").CloseTag("div").Build();

        result.ShouldBe("<div></div>");
    }

    [Fact]
    public void Text_AddsEscapedText()
    {
        var builder = new HtmlBuilder();
        var result = builder.OpenTag("p").Text("Hello & goodbye").CloseTag("p").Build();

        result.ShouldContain("Hello &amp; goodbye");
    }

    [Fact]
    public void Text_EscapesQuotes()
    {
        var builder = new HtmlBuilder();
        var result = builder.Text("Say \"hello\"").Build();

        result.ShouldContain("Say &quot;hello&quot;");
    }

    [Fact]
    public void Raw_AddUneacapedHtml()
    {
        var builder = new HtmlBuilder();
        var result = builder.Raw("<span>raw</span>").Build();

        result.ShouldBe("<span>raw</span>");
    }

    [Fact]
    public void OpenTag_WithStyle_IncludesStyleAttribute()
    {
        var builder = new HtmlBuilder();
        var result = builder.OpenTag("div", style: "color: red;").Build();

        result.ShouldContain("style=\"color: red;\"");
    }

    [Fact]
    public void OpenTag_WithId_IncludesIdAttribute()
    {
        var builder = new HtmlBuilder();
        var result = builder.OpenTag("div", id: "my-id").Build();

        result.ShouldContain("id=\"my-id\"");
    }

    [Fact]
    public void OpenTag_WithClass_IncludesClassAttribute()
    {
        var builder = new HtmlBuilder();
        var result = builder.OpenTag("div", classAttr: "my-class").Build();

        result.ShouldContain("class=\"my-class\"");
    }

    [Fact]
    public void VoidTag_SelfCloses()
    {
        var builder = new HtmlBuilder();
        var result = builder.VoidTag("img").Build();

        result.ShouldContain(" />");
    }

    [Fact]
    public void VoidTag_WithAttributes_IncludesAttributes()
    {
        var builder = new HtmlBuilder();
        var result = builder.VoidTag("img", attributes: new() { ["src"] = "test.png", ["alt"] = "test" }).Build();

        result.ShouldContain("src=\"test.png\"");
        result.ShouldContain("alt=\"test\"");
    }

    [Fact]
    public void Complex_BuildsChainedStructure()
    {
        var builder = new HtmlBuilder();
        var result = builder
            .OpenTag("html")
            .OpenTag("body")
            .OpenTag("h1")
            .Text("Hello World")
            .CloseTag("h1")
            .CloseTag("body")
            .CloseTag("html")
            .Build();

        result.ShouldContain("<html><body><h1>Hello World</h1></body></html>");
    }
}
