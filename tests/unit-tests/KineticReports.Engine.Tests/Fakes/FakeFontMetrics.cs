namespace KineticReports.Engine.Tests.Fakes;

using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;
using KineticReports.Core.Typography;

/// <summary>
/// Deterministic, dependency-free <see cref="IFontMetrics"/> for Engine tests.
/// </summary>
public sealed class FakeFontMetrics : IFontMetrics
{
    public Size MeasureText(string text, ResolvedStyle style, float maxWidth)
        => new Size(text.Length * style.FontSize * 0.6f, style.FontSize * 1.2f);

    public float GetAscent(FontDescriptor descriptor) => descriptor.Size * 0.8f;
    public float GetDescent(FontDescriptor descriptor) => descriptor.Size * 0.2f;
    public float GetLineGap(FontDescriptor descriptor) => 0f;
}
