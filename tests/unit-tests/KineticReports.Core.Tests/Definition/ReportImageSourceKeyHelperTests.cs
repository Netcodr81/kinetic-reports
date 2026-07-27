namespace KineticReports.Core.Tests.Definition;

using KineticReports.Core.Authoring.Serialization;

public class ReportImageSourceKeyHelperTests
{
    [Fact]
    public void CreateEmbeddedSourceKey_WithBytes_ReturnsDataUri()
    {
        var bytes = new byte[] { 0x01, 0x02, 0x03 };

        var sourceKey = ReportImageSourceKeyHelper.CreateEmbeddedSourceKey(bytes, "image/png");

        sourceKey.ShouldStartWith("data:image/png;base64,");
    }

    [Fact]
    public void CreateEmbeddedSourceKeyFromFile_WithPngFile_InfersMimeType()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"kr-img-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(tempFile, new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        try
        {
            var sourceKey = ReportImageSourceKeyHelper.CreateEmbeddedSourceKeyFromFile(tempFile);
            sourceKey.ShouldStartWith("data:image/png;base64,");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void CreateEmbeddedSourceKeyFromFile_WithUnknownExtension_Throws()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"kr-img-{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(tempFile, new byte[] { 0x00 });

        try
        {
            Should.Throw<InvalidOperationException>(() => ReportImageSourceKeyHelper.CreateEmbeddedSourceKeyFromFile(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void CreateFileSourceKey_WithRelativePath_ReturnsFileUri()
    {
        var sourceKey = ReportImageSourceKeyHelper.CreateFileSourceKey("images/logo.png");

        sourceKey.ShouldStartWith("file:///");

        var isUri = Uri.TryCreate(sourceKey, UriKind.Absolute, out var uri);
        isUri.ShouldBeTrue();
        uri.ShouldNotBeNull();
        uri.IsFile.ShouldBeTrue();
        uri.AbsolutePath.ShouldContain("images");
        uri.AbsolutePath.ShouldContain("logo.png");
    }
}
