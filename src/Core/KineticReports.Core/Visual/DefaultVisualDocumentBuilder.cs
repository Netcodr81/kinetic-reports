namespace KineticReports.Core.Visual;

using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Default implementation of <see cref="IVisualDocumentBuilder"/>.
/// </summary>
public sealed class DefaultVisualDocumentBuilder : IVisualDocumentBuilder
{
    private const string UnsupportedMetadataKey = "diagnostic.unsupported";

    /// <inheritdoc/>
    public VisualDocument Build(ReportDocument reportDocument)
    {
        if (reportDocument == null)
            throw new ArgumentNullException(nameof(reportDocument));

        var metrics = new BuildMetrics();
        var pages = reportDocument.Pages.Select(page => BuildPage(page, metrics)).ToList();

        return new VisualDocument
        {
            SchemaVersion = "1.0",
            Pages = pages,
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source.pageCount"] = reportDocument.PageCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["diagnostic.totalElements"] = metrics.TotalElementCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["diagnostic.normalizedBounds"] = metrics.NormalizedBoundsCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["diagnostic.clippedElements"] = metrics.ClippedElementCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["diagnostic.unsupportedElements"] = metrics.UnsupportedElementCount.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };
    }

    private static VisualPage BuildPage(PageBlock page, BuildMetrics metrics)
    {
        var layers = new List<VisualLayer>();
        var pageBounds = new Geometry.Rect(0f, 0f, page.PageWidth, page.PageHeight);

        if (page.Header != null)
        {
            layers.Add(new VisualLayer
            {
                Name = "header",
                Elements = BuildElements(page.Header.Children, pageBounds, metrics)
            });
        }

        layers.Add(new VisualLayer
        {
            Name = "body",
            Elements = BuildElements(page.Children, pageBounds, metrics)
        });

        if (page.Footer != null)
        {
            layers.Add(new VisualLayer
            {
                Name = "footer",
                Elements = BuildElements(page.Footer.Children, pageBounds, metrics)
            });
        }

        return new VisualPage
        {
            PageNumber = page.PageNumber,
            Width = page.PageWidth,
            Height = page.PageHeight,
            Layers = layers,
            Metadata = CreatePageMetadata(page)
        };
    }

    private static IReadOnlyList<VisualElement> BuildElements(
        IEnumerable<LayoutBlock> blocks,
        Geometry.Rect pageBounds,
        BuildMetrics metrics)
    {
        var elements = new List<VisualElement>();

        foreach (var block in blocks)
            elements.Add(BuildElement(block, pageBounds, metrics));

        return elements;
    }

    private static VisualElement BuildElement(
        LayoutBlock block,
        Geometry.Rect pageBounds,
        BuildMetrics metrics)
    {
        var normalizedBounds = NormalizeBounds(block.Bounds, pageBounds, out var boundsWereNormalized);
        var clip = CreateClipIfNeeded(block.Bounds, pageBounds, normalizedBounds, out var wasClipped);

        metrics.TotalElementCount++;
        if (boundsWereNormalized)
            metrics.NormalizedBoundsCount++;
        if (wasClipped)
            metrics.ClippedElementCount++;

        VisualElement element = block switch
        {
            TextBlock text => BuildText(text, normalizedBounds),
            ImageBlock image => BuildImage(image, normalizedBounds),
            ShapeBlock shape => BuildShape(shape, normalizedBounds),
            TableBlock table => BuildTable(table, normalizedBounds),
            ContainerBlock container => BuildContainer(container, normalizedBounds, pageBounds, metrics),
            SectionBlock section => BuildContainerLike(section, section.Children, normalizedBounds, pageBounds, metrics),
            ReportBlock reportBlock => BuildContainerLike(reportBlock, reportBlock.Children, normalizedBounds, pageBounds, metrics),
            RowBlock row => BuildContainerLike(row, row.Cells.Cast<LayoutBlock>(), normalizedBounds, pageBounds, metrics),
            CellBlock cell => BuildContainerLike(cell, cell.Children, normalizedBounds, pageBounds, metrics),
            ChartBlock chart => BuildUnsupported(chart, normalizedBounds, "Chart visual primitive not mapped in phase 1."),
            BarcodeBlock barcode => BuildUnsupported(barcode, normalizedBounds, "Barcode visual primitive not mapped in phase 1."),
            _ => BuildUnsupported(block, normalizedBounds, "Layout block type is not recognized by Visual mapper.")
        };

        if (clip != null)
            element = element with { Clips = [clip] };

        if (boundsWereNormalized || wasClipped)
        {
            var metadata = new Dictionary<string, string>(element.Metadata, StringComparer.Ordinal);
            if (boundsWereNormalized)
                metadata["diagnostic.boundsNormalized"] = "true";
            if (wasClipped)
                metadata["diagnostic.pageClipApplied"] = "true";
            element = element with { Metadata = metadata };
        }

        if (element is VisualUnsupportedElement)
            metrics.UnsupportedElementCount++;

        return element;
    }

    private static VisualText BuildText(TextBlock block, Geometry.Rect bounds)
    {
        return new VisualText
        {
            Id = block.Id,
            Bounds = bounds,
            Text = block.Text,
            FontFamily = block.Style.FontFamily,
            FontSize = block.Style.FontSize,
            FontWeight = block.Style.FontWeight.ToString(),
            TextColorHex = ToHex(block.Style.TextColor),
            Metadata = CreateBlockMetadata(block)
        };
    }

    private static VisualImage BuildImage(ImageBlock block, Geometry.Rect bounds)
    {
        return new VisualImage
        {
            Id = block.Id,
            Bounds = bounds,
            SourceKey = block.SourceKey,
            Stretch = MapStretch(block.Stretch),
            Metadata = CreateBlockMetadata(block)
        };
    }

    private static VisualShape BuildShape(ShapeBlock block, Geometry.Rect bounds)
    {
        return new VisualShape
        {
            Id = block.Id,
            Bounds = bounds,
            Kind = MapShape(block.Kind),
            FillColorHex = block.Fill.HasValue ? ToHex(block.Fill.Value) : null,
            StrokeColorHex = block.Stroke.HasValue ? ToHex(block.Stroke.Value) : null,
            StrokeWidth = block.StrokeWidth,
            Metadata = CreateBlockMetadata(block)
        };
    }

    private static VisualTablePlaceholder BuildTable(TableBlock block, Geometry.Rect bounds)
    {
        return new VisualTablePlaceholder
        {
            Id = block.Id,
            Bounds = bounds,
            RowCount = block.Rows.Count,
            ColumnCount = block.Columns.Count,
            Metadata = CreateBlockMetadata(block)
        };
    }

    private static VisualContainer BuildContainer(
        ContainerBlock block,
        Geometry.Rect bounds,
        Geometry.Rect pageBounds,
        BuildMetrics metrics)
    {
        return new VisualContainer
        {
            Id = block.Id,
            Bounds = bounds,
            Children = BuildElements(block.Children, pageBounds, metrics),
            Metadata = CreateBlockMetadata(block)
        };
    }

    private static VisualContainer BuildContainerLike(
        LayoutBlock source,
        IEnumerable<LayoutBlock> children,
        Geometry.Rect bounds,
        Geometry.Rect pageBounds,
        BuildMetrics metrics)
    {
        return new VisualContainer
        {
            Id = source.Id,
            Bounds = bounds,
            Children = BuildElements(children, pageBounds, metrics),
            Metadata = CreateBlockMetadata(source)
        };
    }

    private static VisualUnsupportedElement BuildUnsupported(
        LayoutBlock block,
        Geometry.Rect bounds,
        string reason)
    {
        var metadata = new Dictionary<string, string>(CreateBlockMetadata(block), StringComparer.Ordinal)
        {
            [UnsupportedMetadataKey] = "true"
        };

        return new VisualUnsupportedElement
        {
            Id = block.Id,
            Bounds = bounds,
            SourceType = block.GetType().Name,
            Reason = reason,
            Metadata = metadata
        };
    }

    private static Geometry.Rect NormalizeBounds(
        Geometry.Rect source,
        Geometry.Rect pageBounds,
        out bool changed)
    {
        var x = source.X;
        var y = source.Y;
        var width = source.Width;
        var height = source.Height;

        changed = false;

        if (float.IsNaN(x) || float.IsInfinity(x))
        {
            x = 0f;
            changed = true;
        }

        if (float.IsNaN(y) || float.IsInfinity(y))
        {
            y = 0f;
            changed = true;
        }

        if (float.IsNaN(width) || float.IsInfinity(width) || width < 0f)
        {
            width = Math.Max(0f, width);
            if (float.IsNaN(width) || float.IsInfinity(width))
                width = 0f;
            changed = true;
        }

        if (float.IsNaN(height) || float.IsInfinity(height) || height < 0f)
        {
            height = Math.Max(0f, height);
            if (float.IsNaN(height) || float.IsInfinity(height))
                height = 0f;
            changed = true;
        }

        if (x < 0f)
        {
            x = 0f;
            changed = true;
        }

        if (y < 0f)
        {
            y = 0f;
            changed = true;
        }

        var maxWidth = Math.Max(0f, pageBounds.Width - x);
        var maxHeight = Math.Max(0f, pageBounds.Height - y);

        if (width > maxWidth)
        {
            width = maxWidth;
            changed = true;
        }

        if (height > maxHeight)
        {
            height = maxHeight;
            changed = true;
        }

        return new Geometry.Rect(x, y, width, height);
    }

    private static VisualClip? CreateClipIfNeeded(
        Geometry.Rect source,
        Geometry.Rect pageBounds,
        Geometry.Rect normalized,
        out bool clipped)
    {
        var overflowedLeft = source.X < 0f;
        var overflowedTop = source.Y < 0f;
        var overflowedRight = source.Right > pageBounds.Width;
        var overflowedBottom = source.Bottom > pageBounds.Height;

        clipped = overflowedLeft || overflowedTop || overflowedRight || overflowedBottom;
        if (!clipped)
            return null;

        return new VisualClip
        {
            Bounds = normalized
        };
    }

    private static IReadOnlyDictionary<string, string> CreatePageMetadata(PageBlock page)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source.layoutBlockType"] = page.LayoutBlockType.ToString(),
            ["source.blockId"] = page.Id
        };
    }

    private static IReadOnlyDictionary<string, string> CreateBlockMetadata(LayoutBlock block)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source.layoutBlockType"] = block.LayoutBlockType.ToString(),
            ["source.blockId"] = block.Id
        };

        if (block is ReportBlock reportBlock)
            metadata["source.blockKind"] = reportBlock.Kind.ToString();

        return metadata;
    }

    private static VisualImageStretch MapStretch(ImageStretch stretch)
    {
        return stretch switch
        {
            ImageStretch.None => VisualImageStretch.None,
            ImageStretch.Fill => VisualImageStretch.Fill,
            ImageStretch.Uniform => VisualImageStretch.Uniform,
            ImageStretch.UniformToFill => VisualImageStretch.UniformToFill,
            _ => VisualImageStretch.Uniform
        };
    }

    private static VisualShapeKind MapShape(ShapeKind shape)
    {
        return shape switch
        {
            ShapeKind.Rectangle => VisualShapeKind.Rectangle,
            ShapeKind.Ellipse => VisualShapeKind.Ellipse,
            ShapeKind.Line => VisualShapeKind.Line,
            _ => VisualShapeKind.Path
        };
    }

    private static string ToHex(Color color)
    {
        return string.Create(
            9,
            color,
            static (span, value) =>
            {
                span[0] = '#';
                value.A.TryFormat(span[1..3], out _, "X2");
                value.R.TryFormat(span[3..5], out _, "X2");
                value.G.TryFormat(span[5..7], out _, "X2");
                value.B.TryFormat(span[7..9], out _, "X2");
            });
    }

    private sealed class BuildMetrics
    {
        public int TotalElementCount { get; set; }

        public int NormalizedBoundsCount { get; set; }

        public int ClippedElementCount { get; set; }

        public int UnsupportedElementCount { get; set; }
    }
}
