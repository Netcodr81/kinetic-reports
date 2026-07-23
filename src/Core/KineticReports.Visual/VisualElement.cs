namespace KineticReports.Visual;

using KineticReports.Core.Geometry;
using System.Text.Json.Serialization;

/// <summary>
/// Base type for all immutable visual elements in a <see cref="VisualDocument"/>.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(VisualContainer), "container")]
[JsonDerivedType(typeof(VisualText), "text")]
[JsonDerivedType(typeof(VisualImage), "image")]
[JsonDerivedType(typeof(VisualShape), "shape")]
[JsonDerivedType(typeof(VisualTablePlaceholder), "tablePlaceholder")]
[JsonDerivedType(typeof(VisualUnsupportedElement), "unsupported")]
public abstract record VisualElement
{
    /// <summary>
    /// Gets the stable visual element id.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the element bounds in page-local DIPs.
    /// </summary>
    public required Rect Bounds { get; init; }

    /// <summary>
    /// Gets the clipping regions applied to this element.
    /// </summary>
    public IReadOnlyList<VisualClip> Clips { get; init; } = [];

    /// <summary>
    /// Gets transforms applied to this element in order.
    /// </summary>
    public IReadOnlyList<VisualTransform> Transforms { get; init; } = [];

    /// <summary>
    /// Gets optional metadata used for diagnostics and source mapping.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
