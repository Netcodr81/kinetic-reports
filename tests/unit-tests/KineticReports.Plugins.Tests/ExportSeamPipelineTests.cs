namespace KineticReports.Plugins.Tests;

using Shouldly;
using Xunit;

/// <summary>
/// Tests for <see cref="ExportSeamPipeline"/> deterministic behavior.
/// </summary>
public class ExportSeamPipelineTests
{
    [Fact]
    public void BuildRegistry_WithPluginContributors_IsDeterministicAndDeduplicated()
    {
        // Arrange
        IReadOnlyList<ExportFormatDescriptor> coreFormats =
        [
            new ExportFormatDescriptor("html", "HTML", "text/html", "html")
        ];

        IReadOnlyList<IPlugin> plugins =
        [
            new FakeRegistryPlugin("z.plugin", 100, [new("json", "JSON", "application/json", "json")]),
            new FakeRegistryPlugin("a.plugin", 100, [new("json", "JSON Override", "application/json", "json")]),
            new FakeRegistryPlugin("b.plugin", 10, [new("csv", "CSV", "text/csv", "csv")])
        ];

        // Act
        var result = ExportSeamPipeline.BuildRegistry(coreFormats, plugins);

        // Assert
        result.Select(f => f.FormatId).ShouldBe(["html", "csv", "json"]);

        var json = result.Single(f => f.FormatId == "json");
        json.DisplayName.ShouldBe("JSON Override");
    }

    [Fact]
    public void Negotiate_WhenPluginReturnsChoice_UsesFirstDeterministicMatch()
    {
        // Arrange
        IReadOnlyList<IPlugin> plugins =
        [
            new FakeNegotiationPlugin("z.plugin", 20, _ => "html"),
            new FakeNegotiationPlugin("a.plugin", 20, _ => "pdf")
        ];

        IReadOnlyList<ExportFormatDescriptor> formats =
        [
            new ExportFormatDescriptor("html", "HTML", "text/html", "html"),
            new ExportFormatDescriptor("pdf", "PDF", "application/pdf", "pdf")
        ];

        // Act
        var chosen = ExportSeamPipeline.Negotiate("md", formats, plugins);

        // Assert
        chosen.ShouldBe("pdf");
    }

    [Fact]
    public void Negotiate_WhenNoPluginHandlesRequest_ReturnsRequestedFormat()
    {
        // Arrange
        IReadOnlyList<IPlugin> plugins =
        [
            new FakeNegotiationPlugin("a.plugin", 1, _ => null)
        ];

        IReadOnlyList<ExportFormatDescriptor> formats =
        [
            new ExportFormatDescriptor("html", "HTML", "text/html", "html")
        ];

        // Act
        var chosen = ExportSeamPipeline.Negotiate("html", formats, plugins);

        // Assert
        chosen.ShouldBe("html");
    }

    [Fact]
    public async Task PostProcessArtifactAsync_AppliesProcessorsInDeterministicOrder()
    {
        // Arrange
        var original = "start"u8.ToArray();
        IReadOnlyList<IPlugin> plugins =
        [
            new FakeArtifactPlugin("z.plugin", 10, "-z"),
            new FakeArtifactPlugin("a.plugin", 10, "-a"),
            new FakeArtifactPlugin("b.plugin", 5, "-b")
        ];

        // Act
        var result = await ExportSeamPipeline.PostProcessArtifactAsync("html", original, plugins);
        var text = System.Text.Encoding.UTF8.GetString(result);

        // Assert
        text.ShouldBe("start-b-a-z");
    }

    private sealed class FakeRegistryPlugin(
        string id,
        int order,
        IReadOnlyList<ExportFormatDescriptor> formats) :
        PluginBase,
        IExportFormatRegistryPlugin
    {
        public override string Id => id;
        public override string Name => id;
        public override string Version => "1.0.0";
        public int Order => order;

        public IReadOnlyList<ExportFormatDescriptor> GetFormats() => formats;
    }

    private sealed class FakeNegotiationPlugin(
        string id,
        int order,
        Func<string, string?> choose) :
        PluginBase,
        IExportNegotiationPlugin
    {
        public override string Id => id;
        public override string Name => id;
        public override string Version => "1.0.0";
        public int Order => order;

        public string? Negotiate(string requestedFormat, IReadOnlyList<ExportFormatDescriptor> availableFormats)
            => choose(requestedFormat);
    }

    private sealed class FakeArtifactPlugin(
        string id,
        int order,
        string suffix) :
        PluginBase,
        IExportArtifactPostProcessorPlugin
    {
        public override string Id => id;
        public override string Name => id;
        public override string Version => "1.0.0";
        public int Order => order;

        public Task<byte[]> ProcessArtifactAsync(
            string formatId,
            byte[] artifact,
            CancellationToken cancellationToken = default)
        {
            var text = System.Text.Encoding.UTF8.GetString(artifact);
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(text + suffix));
        }
    }
}
