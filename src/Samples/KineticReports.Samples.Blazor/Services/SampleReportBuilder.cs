namespace KineticReports.Samples.Blazor.Services;

using System.Globalization;
using KineticReports.Core.Authoring.Compilation;
using KineticReports.Core.Engine.Building;
using KineticReports.Core.Engine.Data;
using KineticReports.Core.Engine.Expressions;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Builds simple, data-backed report blocks for the Blazor sample reports.
/// </summary>
internal sealed class SampleReportBuilder : IReportBuilder
{
    private const float MaxAuthoredCanvasHeight = 860f;
    private const float AuthoredGridColumnGap = 12f;
    private const float AuthoredGridRowGap = 12f;
    private const float FallbackAuthoredPageWidth = 816f;
    private const float FallbackAuthoredPageHeight = 1056f;

    private const float HeaderBlockBottomSpacing = 12f;
    private const float SectionTitleBlockTopSpacing = 4f;
    private const float SectionTitleBlockBottomSpacing = 8f;
    private const float DetailBlockBottomSpacing = 6f;
    private const float TableBlockBottomSpacing = 12f;

    private static readonly AppliedStyle HeaderTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 20f,
        FontWeight = FontWeight.Bold
    };

    private static readonly AppliedStyle SectionTitleStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 14f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(36, 70, 140)
    };

    private static readonly AppliedStyle DetailTextStyle = new()
    {
        FontFamily = "Consolas",
        FontSize = 11f,
        LineHeight = 1.45f
    };

    private static readonly AppliedStyle GroupHeaderTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 12f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(31, 58, 111)
    };

    private static readonly AppliedStyle EmphasisTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 12f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(44, 94, 69)
    };

    private static readonly AppliedStyle SubtleTextStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        TextColor = Color.FromRgb(80, 88, 102)
    };

    private static readonly AppliedStyle TableHeaderCellStyle = new()
    {
        FontFamily = "Arial",
        FontSize = 11f,
        FontWeight = FontWeight.SemiBold,
        TextColor = Color.FromRgb(255, 255, 255),
        Background = Color.FromRgb(31, 58, 111),
        Padding = new Core.Geometry.Thickness(6f, 8f, 6f, 8f),
        Border = Border.Uniform(1f, Color.FromRgb(199, 210, 230))
    };

    private static readonly AppliedStyle TableCellStyle = new()
    {
        FontFamily = "Consolas",
        FontSize = 11f,
        TextColor = Color.FromRgb(28, 32, 42),
        Padding = new Core.Geometry.Thickness(5f, 8f, 5f, 8f),
        Border = Border.Uniform(1f, Color.FromRgb(220, 226, 239))
    };

    /// <inheritdoc/>
    public IReadOnlyList<ReportBlock> Build(DataContext dataContext, IExpressionEvaluator evaluator)
    {
        if (TryBuildAuthoredComponentBlocks(dataContext, out var authoredBlocks))
            return authoredBlocks;

        var builder = new ReportLayoutBuilder()
            .Add(CreateSingleTextBlock(
                "sample-header",
                BlockType.PageHeader,
                HeaderTextStyle,
                "KineticReports Sample Data Preview"));

        foreach (var dataSource in dataContext.DataSources)
        {
            var sourceId = dataSource.Key;
            var rows = dataSource.Value;

            builder.Add(CreateSingleTextBlock(
                $"source-{sourceId}-title",
                BlockType.ReportHeader,
                SectionTitleStyle,
                $"Data Source: {sourceId} ({rows.Count} row(s))"));

            if (rows.Count == 0)
            {
                builder.Add(CreateSingleTextBlock(
                    $"source-{sourceId}-empty",
                    BlockType.Detail,
                    DetailTextStyle,
                    "No rows returned."));

                continue;
            }

            if (string.Equals(sourceId, "sales-orders", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(CreateSalesOrdersTableBlock(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "quote-daily", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateQuoteDailyBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "customer-activity", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateCustomerActivityGroupedBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "kpi-summary", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateKpiSummaryBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "regional-performance", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(CreateRegionalPerformanceTableBlock(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "style-showcase", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddRange(CreateStyleShowcaseBlocks(sourceId, rows));
                continue;
            }

            if (string.Equals(sourceId, "expression-demo", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(CreateExpressionDemoTableBlock(sourceId, rows, evaluator));

                continue;
            }

            builder.Add(CreateGenericTableBlock(sourceId, rows));
        }

        if (dataContext.DataSources.Count == 0)
        {
            builder.Add(CreateSingleTextBlock(
                "sample-empty",
                BlockType.Detail,
                DetailTextStyle,
                "No data sources were resolved for this report."));
        }

        return builder.Build();
    }

    private static bool TryBuildAuthoredComponentBlocks(
        DataContext dataContext,
        out IReadOnlyList<ReportBlock> blocks)
    {
        blocks = [];

        var definition = dataContext.Definition;
        var metadata = definition?.Metadata;
        if (metadata is null)
            return false;

        if (!metadata.TryGetValue("authoring.compiledComponents", out var rawComponents) || rawComponents is null)
            return false;

        IReadOnlyList<CompiledComponentMetadata>? compiledComponents = rawComponents switch
        {
            IReadOnlyList<CompiledComponentMetadata> typed => typed,
            IEnumerable<CompiledComponentMetadata> sequence => sequence.ToList(),
            _ => null
        };

        if (compiledComponents is null || compiledComponents.Count == 0)
            return false;

        var reportName = string.IsNullOrWhiteSpace(definition?.Name)
            ? "Authored Report"
            : definition.Name;

        var providerSummary = string.Join(
            ", ",
            definition?.DataSources.Select(source => $"{source.Id}:{source.ProviderType}") ?? []);

        var builder = new ReportLayoutBuilder()
            .Add(CreateSingleTextBlock(
                "authored-header",
                BlockType.PageHeader,
                HeaderTextStyle,
                reportName))
            .Add(CreateSingleTextBlock(
                "authored-provider-summary",
                BlockType.ReportHeader,
                SubtleTextStyle,
                string.IsNullOrWhiteSpace(providerSummary)
                    ? "Provider: (none)"
                    : $"Provider(s): {providerSummary}"));

        foreach (var headerBand in CreateAuthoredBandBlocks(compiledComponents, "Header", BlockType.PageHeader, dataContext))
            builder.Add(headerBand);

        foreach (var footerBand in CreateAuthoredBandBlocks(compiledComponents, "Footer", BlockType.PageFooter, dataContext))
            builder.Add(footerBand);

        var flattened = FlattenBodyComponentsWithOffsets(compiledComponents)
            .OrderBy(entry => entry.AbsoluteY)
            .ThenBy(entry => entry.AbsoluteX)
            .ToList();

        var (authoredPageWidth, authoredPageHeight) = ResolveAuthoredPageSize(compiledComponents);
        var availableGridWidth = Math.Max(240f, authoredPageWidth);
        var availableGridHeight = Math.Max(320f, Math.Min(authoredPageHeight, MaxAuthoredCanvasHeight));

        var segmentIndex = 1;
        var hasRenderableBodyContent = false;
        var layoutChildren = new List<LayoutBlock>();
        var placements = new Dictionary<string, Core.Geometry.Rect>(StringComparer.Ordinal);
        var currentGridRow = 1;
        var currentX = 0f;
        var currentY = 0f;
        var currentRowHeight = 0f;

        void AdvanceGridRow()
        {
            currentY += currentRowHeight + AuthoredGridRowGap;
            currentX = 0f;
            currentRowHeight = 0f;
            currentGridRow++;
        }

        void ResetSegmentState()
        {
            layoutChildren = new List<LayoutBlock>();
            placements = new Dictionary<string, Core.Geometry.Rect>(StringComparer.Ordinal);
            currentGridRow = 1;
            currentX = 0f;
            currentY = 0f;
            currentRowHeight = 0f;
        }

        void FinalizeCurrentSegment()
        {
            if (!TryCreateCanvasBlock(layoutChildren, placements, segmentIndex, out var canvasBlock))
                return;

            builder.Add(canvasBlock);
            hasRenderableBodyContent = true;
            segmentIndex++;
        }

        for (var index = 0; index < flattened.Count; index++)
        {
            var entry = flattened[index];
            var component = entry.Component;

            if (string.Equals(component.CanonicalType, "PageBreak", StringComparison.OrdinalIgnoreCase))
            {
                FinalizeCurrentSegment();

                builder.Add(ReportBlockFactory.CreatePageBreak($"authored-page-break-{index + 1}", CreateBlockStyle(BlockType.Detail)));
                ResetSegmentState();
                continue;
            }

            var element = CreateComponentLayoutElement(component, index + 1, dataContext);
            if (element is null)
                continue;

            var width = component.Placement.Width > 0f ? component.Placement.Width : Math.Max(120f, element.DesiredSize.Width);
            var height = component.Placement.Height > 0f ? component.Placement.Height : Math.Max(26f, element.DesiredSize.Height);

            width = Math.Clamp(width, 24f, availableGridWidth);

            var explicitRow = TryGetExplicitGridRow(component);
            if (explicitRow.HasValue)
            {
                var targetRow = Math.Max(1, explicitRow.Value);
                while (currentGridRow < targetRow)
                    AdvanceGridRow();
            }
            else if (HasNextRowDirective(component) && currentX > 0f)
            {
                AdvanceGridRow();
            }

            if (currentX > 0f && currentX + width > availableGridWidth)
                AdvanceGridRow();

            if (currentY > 0f && currentY + height > availableGridHeight)
            {
                FinalizeCurrentSegment();
                builder.Add(ReportBlockFactory.CreatePageBreak($"authored-auto-page-break-{index + 1}", CreateBlockStyle(BlockType.Detail)));
                ResetSegmentState();
            }

            layoutChildren.Add(element);
            placements[element.Id] = new Core.Geometry.Rect(currentX, currentY, width, height);
            currentX += width + AuthoredGridColumnGap;
            currentRowHeight = Math.Max(currentRowHeight, height);
        }

        FinalizeCurrentSegment();

        if (!hasRenderableBodyContent)
        {
            builder.Add(CreateSingleTextBlock(
                "authored-no-components",
                BlockType.Detail,
                SubtleTextStyle,
                "No renderable authored components found."));
        }

        blocks = builder.Build();
        return true;
    }

    private static IEnumerable<FlattenedComponent> FlattenComponentsWithOffsets(
        IEnumerable<CompiledComponentMetadata> components,
        float offsetX = 0f,
        float offsetY = 0f)
    {
        foreach (var component in components)
        {
            var absoluteX = offsetX + component.Placement.X;
            var absoluteY = offsetY + component.Placement.Y;

            yield return new FlattenedComponent(component, absoluteX, absoluteY);

            foreach (var child in FlattenComponentsWithOffsets(component.Children, absoluteX, absoluteY))
                yield return child;
        }
    }

    private static IReadOnlyList<FlattenedComponent> FlattenBodyComponentsWithOffsets(
        IEnumerable<CompiledComponentMetadata> components)
    {
        var flattened = new List<FlattenedComponent>();

        AppendBodyComponents(flattened, components, 0f, 0f);

        return flattened;
    }

    private static void AppendBodyComponents(
        List<FlattenedComponent> flattened,
        IEnumerable<CompiledComponentMetadata> components,
        float offsetX,
        float offsetY)
    {
        foreach (var component in components)
        {
            var absoluteX = offsetX + component.Placement.X;
            var absoluteY = offsetY + component.Placement.Y;
            var canonicalType = component.CanonicalType;

            if (string.Equals(canonicalType, "Header", StringComparison.OrdinalIgnoreCase)
                || string.Equals(canonicalType, "Footer", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!ShouldSkipComponent(component))
                flattened.Add(new FlattenedComponent(component, absoluteX, absoluteY));

            if (ShouldSuppressDescendantsInBody(canonicalType))
                continue;

            AppendBodyComponents(flattened, component.Children, absoluteX, absoluteY);
        }
    }

    private static bool ShouldSuppressDescendantsInBody(string canonicalType)
    {
        return string.Equals(canonicalType, "Group", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "List", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Table", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Chart", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Barcode", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Image", StringComparison.OrdinalIgnoreCase);
    }

    private static (float Width, float Height) ResolveAuthoredPageSize(IEnumerable<CompiledComponentMetadata> components)
    {
        var pagePlacements = EnumerateComponentsByCanonicalType(components, "Page")
            .Select(component => component.Placement)
            .Where(placement => placement.Width > 0f && placement.Height > 0f)
            .ToList();

        if (pagePlacements.Count == 0)
            return (FallbackAuthoredPageWidth, FallbackAuthoredPageHeight);

        return (pagePlacements.Max(placement => placement.Width), pagePlacements.Max(placement => placement.Height));
    }

    private static int? TryGetExplicitGridRow(CompiledComponentMetadata component)
    {
        foreach (var key in new[] { "layout.row", "grid.row", "row" })
        {
            if (!component.Properties.TryGetValue(key, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
                continue;

            if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rowIndex) && rowIndex > 0)
                return rowIndex;
        }

        return null;
    }

    private static bool HasNextRowDirective(CompiledComponentMetadata component)
    {
        foreach (var key in new[] { "layout.newRow", "grid.newRow", "newRow", "nextRow", "row.break" })
        {
            if (!component.Properties.TryGetValue(key, out var rawValue) || string.IsNullOrWhiteSpace(rawValue))
                continue;

            if (bool.TryParse(rawValue, out var asBool))
                return asBool;

            if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryCreateCanvasBlock(
        IReadOnlyList<LayoutBlock> layoutChildren,
        IReadOnlyDictionary<string, Core.Geometry.Rect> placements,
        int segmentIndex,
        out ReportBlock block)
    {
        block = null!;

        if (layoutChildren.Count == 0 || placements.Count == 0)
            return false;

        var minX = placements.Values.Min(rect => rect.X);
        var minY = placements.Values.Min(rect => rect.Y);

        var rebasedPlacements = placements.ToDictionary(
            pair => pair.Key,
            pair => new Core.Geometry.Rect(
                pair.Value.X - minX,
                pair.Value.Y - minY,
                pair.Value.Width,
                pair.Value.Height),
            StringComparer.Ordinal);

        block = new DetailBlock
        {
            Id = $"report-page-body-block-{segmentIndex}",
            Style = new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, 12f)
            },
            Children =
            [
                new ContainerBlock
                {
                    Id = $"report-page-body-container-{segmentIndex}",
                    Style = new AppliedStyle
                    {
                        FontFamily = "Arial",
                        FontSize = 12f,
                        Border = Border.Uniform(1f, Color.FromRgb(210, 216, 228)),
                        Background = Color.FromRgb(250, 252, 255)
                    },
                    Children = layoutChildren.ToList(),
                    ChildPlacements = rebasedPlacements
                }
            ]
        };

        return true;
    }

    private static bool ShouldSkipComponent(CompiledComponentMetadata component)
    {
        return string.Equals(component.CanonicalType, "Page", StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.CanonicalType, "Section", StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.CanonicalType, "Panel", StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.CanonicalType, "Header", StringComparison.OrdinalIgnoreCase)
            || string.Equals(component.CanonicalType, "Footer", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<ReportBlock> CreateAuthoredBandBlocks(
        IReadOnlyList<CompiledComponentMetadata> components,
        string canonicalType,
        BlockType blockType,
        DataContext dataContext)
    {
        var bands = new List<ReportBlock>();
        var index = 1;

        foreach (var component in EnumerateComponentsByCanonicalType(components, canonicalType))
        {
            var flattened = FlattenComponentsWithOffsets(component.Children).ToList();
            if (flattened.Count == 0)
                continue;

            var children = new List<LayoutBlock>();
            var placements = new Dictionary<string, Core.Geometry.Rect>(StringComparer.Ordinal);

            for (var itemIndex = 0; itemIndex < flattened.Count; itemIndex++)
            {
                var entry = flattened[itemIndex];
                var childComponent = entry.Component;

                if (ShouldSkipComponent(childComponent)
                    || string.Equals(childComponent.CanonicalType, "PageBreak", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(childComponent.CanonicalType, "Background", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var element = CreateComponentLayoutElement(childComponent, itemIndex + 1, dataContext);
                if (element is null)
                    continue;

                var width = childComponent.Placement.Width > 0f
                    ? childComponent.Placement.Width
                    : Math.Max(100f, element.DesiredSize.Width);

                var height = childComponent.Placement.Height > 0f
                    ? childComponent.Placement.Height
                    : Math.Max(24f, element.DesiredSize.Height);

                children.Add(element);
                placements[element.Id] = new Core.Geometry.Rect(entry.AbsoluteX, entry.AbsoluteY, width, height);
            }

            if (children.Count == 0)
                continue;

            var bandChildren = new List<LayoutBlock>
            {
                new ContainerBlock
                {
                    Id = $"authored-{canonicalType.ToLowerInvariant()}-band-{index}-container",
                    Style = new AppliedStyle
                    {
                        FontFamily = "Arial",
                        FontSize = 12f
                    },
                    Children = children,
                    ChildPlacements = placements
                }
            };

            ReportBlock bandBlock = blockType switch
            {
                BlockType.PageHeader => new PageHeaderBlock
                {
                    Id = $"authored-{canonicalType.ToLowerInvariant()}-band-{index}",
                    Style = CreateBlockStyle(blockType),
                    Children = bandChildren
                },
                BlockType.PageFooter => new PageFooterBlock
                {
                    Id = $"authored-{canonicalType.ToLowerInvariant()}-band-{index}",
                    Style = CreateBlockStyle(blockType),
                    Children = bandChildren
                },
                _ => ReportBlockFactory.Create(
                    blockType,
                    $"authored-{canonicalType.ToLowerInvariant()}-band-{index}",
                    CreateBlockStyle(blockType))
            };

            bands.Add(bandBlock);
            index++;
        }

        return bands;
    }

    private static IEnumerable<CompiledComponentMetadata> EnumerateComponentsByCanonicalType(
        IEnumerable<CompiledComponentMetadata> components,
        string canonicalType)
    {
        foreach (var component in components)
        {
            if (string.Equals(component.CanonicalType, canonicalType, StringComparison.OrdinalIgnoreCase))
                yield return component;

            foreach (var child in EnumerateComponentsByCanonicalType(component.Children, canonicalType))
                yield return child;
        }
    }

    private static LayoutBlock? CreateComponentLayoutElement(
        CompiledComponentMetadata component,
        int index,
        DataContext dataContext)
    {
        var canonicalType = component.CanonicalType;

        if (string.Equals(canonicalType, "Text", StringComparison.OrdinalIgnoreCase))
        {
            var text = component.BoundFields.TryGetValue("Text", out var boundText)
                ? boundText
                : component.Name ?? component.Id;

            return new TextBlock
            {
                Id = $"authored-component-{index}-text",
                Style = DetailTextStyle,
                Text = text,
            };
        }

        if (string.Equals(canonicalType, "Table", StringComparison.OrdinalIgnoreCase))
            return CreateAuthoredTableElement(component, index, dataContext);

        if (string.Equals(canonicalType, "Image", StringComparison.OrdinalIgnoreCase))
        {
            var source = component.BoundFields.TryGetValue("Source", out var src)
                ? src
                : "https://example.invalid/placeholder.png";

            return new ImageBlock
            {
                Id = $"authored-component-{index}-image",
                Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                SourceKey = source,
                Stretch = ImageStretch.Uniform,
            };
        }

        if (string.Equals(canonicalType, "Rectangle", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Line", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Ellipse", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Divider", StringComparison.OrdinalIgnoreCase))
        {
            var kind = ShapeKind.Rectangle;

            if (string.Equals(canonicalType, "Line", StringComparison.OrdinalIgnoreCase)
                || string.Equals(canonicalType, "Divider", StringComparison.OrdinalIgnoreCase))
            {
                kind = ShapeKind.Line;
            }
            else if (string.Equals(canonicalType, "Ellipse", StringComparison.OrdinalIgnoreCase))
            {
                kind = ShapeKind.Ellipse;
            }

            if (component.Properties.TryGetValue("shape.kind", out var shapeKind))
            {
                if (string.Equals(shapeKind, "Line", StringComparison.OrdinalIgnoreCase))
                    kind = ShapeKind.Line;
                else if (string.Equals(shapeKind, "Ellipse", StringComparison.OrdinalIgnoreCase))
                    kind = ShapeKind.Ellipse;
            }

            return new ShapeBlock
            {
                Id = $"authored-component-{index}-shape",
                Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                Kind = kind,
                Fill = kind == ShapeKind.Line ? null : Color.FromRgb(220, 232, 250),
                Stroke = Color.FromRgb(70, 104, 160),
                StrokeWidth = 1.5f,
            };
        }

        if (string.Equals(canonicalType, "PageNumber", StringComparison.OrdinalIgnoreCase))
        {
            return new TextBlock
            {
                Id = $"authored-component-{index}-page-number",
                Style = GroupHeaderTextStyle,
                Text = "{PageNumber}"
            };
        }

        if (string.Equals(canonicalType, "TotalPages", StringComparison.OrdinalIgnoreCase))
        {
            return new TextBlock
            {
                Id = $"authored-component-{index}-total-pages",
                Style = GroupHeaderTextStyle,
                Text = "{TotalPages}"
            };
        }

        if (string.Equals(canonicalType, "CurrentDate", StringComparison.OrdinalIgnoreCase))
        {
            return new TextBlock
            {
                Id = $"authored-component-{index}-current-date",
                Style = SubtleTextStyle,
                Text = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        if (string.Equals(canonicalType, "CurrentTime", StringComparison.OrdinalIgnoreCase))
        {
            return new TextBlock
            {
                Id = $"authored-component-{index}-current-time",
                Style = SubtleTextStyle,
                Text = DateTime.UtcNow.ToString("HH:mm:ss 'UTC'", CultureInfo.InvariantCulture)
            };
        }

        if (string.Equals(canonicalType, "DocumentInfo", StringComparison.OrdinalIgnoreCase))
        {
            var field = component.Properties.TryGetValue("document.field", out var configuredField)
                ? configuredField
                : "Name";

            return new TextBlock
            {
                Id = $"authored-component-{index}-document-info",
                Style = SubtleTextStyle,
                Text = $"Document {field}: {dataContext.Definition?.Name ?? "Authored Report"}"
            };
        }

        if (string.Equals(canonicalType, "Spacer", StringComparison.OrdinalIgnoreCase)
            || string.Equals(canonicalType, "Background", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (string.Equals(canonicalType, "Barcode", StringComparison.OrdinalIgnoreCase))
        {
            var symbology = component.Properties.TryGetValue("barcode.format", out var format)
                ? format
                : "Code128";
            var value = component.BoundFields.TryGetValue("Value", out var boundValue)
                ? boundValue
                : component.Id;

            return new BarcodeBlock
            {
                Id = $"authored-component-{index}-barcode",
                Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                Symbology = symbology,
                Value = value,
                ShowText = true,
            };
        }

        if (string.Equals(canonicalType, "Chart", StringComparison.OrdinalIgnoreCase))
        {
            var chartType = component.Properties.TryGetValue("chart.type", out var configuredType)
                ? configuredType
                : "Bar";

            return new ChartBlock
            {
                Id = $"authored-component-{index}-chart",
                Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                ChartType = chartType,
                ChartData = component.DataSourceId is { Length: > 0 } && dataContext.DataSources.TryGetValue(component.DataSourceId, out var rows)
                    ? rows
                    : null,
            };
        }

        return new TextBlock
        {
            Id = $"authored-component-{index}-placeholder",
            Style = SubtleTextStyle,
            Text = $"{component.SourceType} ({component.CanonicalType})"
        };
    }

    private static TableBlock CreateAuthoredTableElement(
        CompiledComponentMetadata component,
        int index,
        DataContext dataContext)
    {
        var rows = component.DataSourceId is { Length: > 0 } && dataContext.DataSources.TryGetValue(component.DataSourceId, out var dataRows)
            ? dataRows
            : [];

        var columnNames = rows.FirstOrDefault()?.Keys.ToList()
            ?? ["Info"];

        var columns = columnNames
            .Select(_ => new TableColumn { MinWidth = 100f, Grow = 1f })
            .ToList();

        var tableRows = new List<RowBlock>
        {
            new()
            {
                Id = $"authored-component-{index}-table-header",
                Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                RowType = RowType.Header,
                Cells = columnNames.Select((name, colIndex) =>
                    new CellBlock
                    {
                        Id = $"authored-component-{index}-table-header-cell-{colIndex + 1}",
                        Style = TableHeaderCellStyle,
                        ColumnIndex = colIndex,
                        Children =
                        [
                            new TextBlock
                            {
                                Id = $"authored-component-{index}-table-header-text-{colIndex + 1}",
                                Style = TableHeaderCellStyle,
                                Text = name
                            }
                        ]
                    }).ToList()
            }
        };

        if (rows.Count == 0)
        {
            tableRows.Add(new RowBlock
            {
                Id = $"authored-component-{index}-table-empty-row",
                Style = new AppliedStyle { FontFamily = "Consolas", FontSize = 11f },
                RowType = RowType.Data,
                Cells =
                [
                    new CellBlock
                    {
                        Id = $"authored-component-{index}-table-empty-cell",
                        Style = TableCellStyle,
                        ColumnIndex = 0,
                        ColSpan = columnNames.Count,
                        Children =
                        [
                            new TextBlock
                            {
                                Id = $"authored-component-{index}-table-empty-text",
                                Style = TableCellStyle,
                                Text = "No rows returned for this data source."
                            }
                        ]
                    }
                ]
            });
        }
        else
        {
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];

                tableRows.Add(new RowBlock
                {
                    Id = $"authored-component-{index}-table-row-{rowIndex + 1}",
                    Style = new AppliedStyle { FontFamily = "Consolas", FontSize = 11f },
                    RowType = RowType.Data,
                    Cells = columnNames.Select((name, colIndex) =>
                        new CellBlock
                        {
                            Id = $"authored-component-{index}-table-cell-{rowIndex + 1}-{colIndex + 1}",
                            Style = TableCellStyle,
                            ColumnIndex = colIndex,
                            Children =
                            [
                                new TextBlock
                                {
                                    Id = $"authored-component-{index}-table-text-{rowIndex + 1}-{colIndex + 1}",
                                    Style = TableCellStyle,
                                    Text = GetRowValue(row, name)
                                }
                            ]
                        }).ToList()
                });
            }
        }

        return new TableBlock
        {
            Id = $"authored-component-{index}-table",
            Style = new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            Columns = columns,
            Rows = tableRows,
            RepeatHeaders = true,
        };
    }

    private sealed record FlattenedComponent(CompiledComponentMetadata Component, float AbsoluteX, float AbsoluteY);

    private static ReportBlock CreateSingleTextBlock(
        string id,
        BlockType blockType,
        AppliedStyle textStyle,
        string text)
    {
        return ReportDsl.TextRegion(id, blockType, CreateBlockStyle(blockType), textStyle, text);
    }

    private static IReadOnlyList<ReportBlock> CreateQuoteDailyBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var row = rows[0];

        return
        [
            CreateSingleTextBlock(
                $"source-{sourceId}-heading",
                BlockType.Detail,
                HeaderTextStyle,
                GetRowValue(row, "Heading")),
            CreateSingleTextBlock(
                $"source-{sourceId}-quote",
                BlockType.Detail,
                EmphasisTextStyle,
                GetRowValue(row, "Quote")),
            CreateSingleTextBlock(
                $"source-{sourceId}-meta",
                BlockType.Detail,
                SubtleTextStyle,
                $"Generated: {GetRowValue(row, "GeneratedOn")} | Author: {GetRowValue(row, "Author")}")
        ];
    }

    private static IReadOnlyList<ReportBlock> CreateCustomerActivityGroupedBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var contentRegions = new List<ReportBlock>();

        var groupedRows = rows
            .GroupBy(row => GetRowValue(row, "Tier"), StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groupedRows)
        {
            contentRegions.Add(CreateSingleTextBlock(
                $"source-{sourceId}-group-{SanitizeForId(group.Key)}",
                BlockType.GroupHeader,
                GroupHeaderTextStyle,
                $"Tier: {group.Key} ({group.Count()} customer(s))"));

            var customerRows = group
                .OrderBy(row => GetRowValue(row, "CustomerName"), StringComparer.OrdinalIgnoreCase)
                .ToList();

            for (var index = 0; index < customerRows.Count; index++)
            {
                var row = customerRows[index];
                var detail =
                    $"{index + 1:00}. {GetRowValue(row, "CustomerName")} | " +
                    $"Rep: {GetRowValue(row, "AssignedRep")} | " +
                    $"Balance: {GetRowValue(row, "Balance")} | " +
                    $"Last Invoice: {GetRowValue(row, "LastInvoice")}";

                contentRegions.Add(CreateSingleTextBlock(
                    $"source-{sourceId}-{SanitizeForId(group.Key)}-row-{index + 1}",
                    BlockType.Detail,
                    DetailTextStyle,
                    detail));
            }
        }

        return contentRegions;
    }

    private static IReadOnlyList<ReportBlock> CreateKpiSummaryBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        return rows
            .Select((row, index) =>
                CreateSingleTextBlock(
                    $"source-{sourceId}-kpi-{index + 1}",
                    BlockType.Detail,
                    EmphasisTextStyle,
                    $"{GetRowValue(row, "Metric")}: {GetRowValue(row, "Value")} ({GetRowValue(row, "Trend")})"))
            .ToList();
    }

    private static ReportBlock CreateRegionalPerformanceTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columns = new[]
        {
            new TableColumn { Width = 140f },
            new TableColumn { Width = 140f },
            new TableColumn { Width = 140f },
            new TableColumn { Width = 120f }
        };

        var tableRegion = ReportLayoutBuilder.CreateTableRegion(
            regionId: $"source-{sourceId}-table",
            regionStyle: new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBlockBottomSpacing)
            },
            tableId: $"source-{sourceId}-table-element",
            tableStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            columns: columns,
            blockType: BlockType.Detail,
            repeatHeaders: true);

        tableRegion.AddHeaderRow(
            rowId: $"source-{sourceId}-header-row",
            rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            cellStyle: TableHeaderCellStyle,
            values: ["Region", "Quota", "Actual", "Attainment"]);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            tableRegion.AddDataRow(
                rowId: $"source-{sourceId}-row-{rowIndex + 1}",
                rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                cellStyle: TableCellStyle,
                values:
                [
                    GetRowValue(row, "Region"),
                    GetRowValue(row, "Quota"),
                    GetRowValue(row, "Actual"),
                    GetRowValue(row, "Attainment")
                ]);
        }

        return tableRegion.BuildBlock();
    }

    private static IReadOnlyList<ReportBlock> CreateStyleShowcaseBlocks(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var contentRegions = new List<ReportBlock>();

        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var label = GetRowValue(row, "Label");
            var preview = GetRowValue(row, "Preview");

            contentRegions.Add(CreateSingleTextBlock(
                $"source-{sourceId}-label-{index + 1}",
                BlockType.Detail,
                ResolveShowcaseLabelStyle(label),
                label));

            contentRegions.Add(CreateSingleTextBlock(
                $"source-{sourceId}-preview-{index + 1}",
                BlockType.Detail,
                ResolveShowcasePreviewStyle(label),
                preview));
        }

        return contentRegions;
    }

    private static ReportBlock CreateSalesOrdersTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columns = new[]
        {
            new TableColumn { Width = 120f },
            new TableColumn { Width = 120f },
            new TableColumn { Width = 140f },
            new TableColumn { Width = 120f },
            new TableColumn { Width = 120f }
        };

        var tableRegion = ReportLayoutBuilder.CreateTableRegion(
            regionId: $"source-{sourceId}-table",
            regionStyle: new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBlockBottomSpacing)
            },
            tableId: $"source-{sourceId}-table-element",
            tableStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            columns: columns,
            blockType: BlockType.Detail,
            repeatHeaders: true);

        tableRegion.AddHeaderRow(
            rowId: $"source-{sourceId}-header-row",
            rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            cellStyle: TableHeaderCellStyle,
            values: ["OrderId", "Region", "SalesPerson", "Amount", "Status"]);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];

            tableRegion.AddDataRow(
                rowId: $"source-{sourceId}-row-{rowIndex + 1}",
                rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                cellStyle: TableCellStyle,
                values:
                [
                    GetRowValue(row, "OrderId"),
                    GetRowValue(row, "Region"),
                    GetRowValue(row, "SalesPerson"),
                    GetRowValue(row, "Amount"),
                    GetRowValue(row, "Status")
                ]);
        }

        return tableRegion.BuildBlock();
    }

    private static ReportBlock CreateExpressionDemoTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows,
        IExpressionEvaluator evaluator)
    {
        var expressionRows = new List<IReadOnlyDictionary<string, object?>>(rows.Count);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var context = new ExpressionContext
            {
                CurrentRow = row,
                Parameters = new Dictionary<string, object?>
                {
                    ["ReportLabel"] = "Expression Demo"
                },
                RowIndex = rowIndex,
                PageNumber = 1
            };

            expressionRows.Add(new Dictionary<string, object?>
            {
                ["Row"] = rowIndex + 1,
                ["{CustomerName}"] = EvaluateToText(evaluator, "{CustomerName}", context),
                ["{Tier}"] = EvaluateToText(evaluator, "{Tier}", context),
                ["{AssignedRep}"] = EvaluateToText(evaluator, "{AssignedRep}", context),
                ["{ReportLabel}"] = EvaluateToText(evaluator, "{ReportLabel}", context),
                ["{DoesNotExist}"] = EvaluateToText(evaluator, "{DoesNotExist}", context),
                ["Literal"] = EvaluateToText(evaluator, "Literal text", context)
            });
        }

        return CreateGenericTableBlock(sourceId, expressionRows);
    }

    private static ReportBlock CreateGenericTableBlock(
        string sourceId,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var columnNames = rows[0].Keys.ToList();

        var columns = columnNames
            .Select(_ => new TableColumn { MinWidth = 110f, Grow = 1f })
            .ToArray();

        var tableRegion = ReportLayoutBuilder.CreateTableRegion(
            regionId: $"source-{sourceId}-table",
            regionStyle: new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, TableBlockBottomSpacing)
            },
            tableId: $"source-{sourceId}-table-element",
            tableStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            columns: columns,
            blockType: BlockType.Detail,
            repeatHeaders: true);

        tableRegion.AddHeaderRow(
            rowId: $"source-{sourceId}-header-row",
            rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
            cellStyle: TableHeaderCellStyle,
            values: columnNames);

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            var values = columnNames
                .Select(columnName => GetRowValue(row, columnName))
                .ToList();

            tableRegion.AddDataRow(
                rowId: $"source-{sourceId}-row-{rowIndex + 1}",
                rowStyle: new AppliedStyle { FontFamily = "Arial", FontSize = 11f },
                cellStyle: TableCellStyle,
                values: values);
        }

        return tableRegion.BuildBlock();
    }

    private static string GetRowValue(IReadOnlyDictionary<string, object?> row, string fieldName)
    {
        return row.TryGetValue(fieldName, out var value)
            ? FormatValue(value)
            : string.Empty;
    }

    private static string SanitizeForId(string value)
    {
        var chars = value
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        return string.Join(string.Empty, chars).Trim('-');
    }

    private static AppliedStyle ResolveShowcaseLabelStyle(string label)
    {
        if (string.Equals(label, "Primary Heading", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 18f,
                FontWeight = FontWeight.Bold,
                TextColor = Color.FromRgb(27, 43, 74)
            };
        }

        if (string.Equals(label, "Accent Note", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 13f,
                FontWeight = FontWeight.SemiBold,
                TextColor = Color.FromRgb(130, 67, 0)
            };
        }

        return GroupHeaderTextStyle;
    }

    private static AppliedStyle ResolveShowcasePreviewStyle(string label)
    {
        if (string.Equals(label, "Primary Heading", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Georgia",
                FontSize = 12f,
                FontStyle = FontStyleValue.Italic,
                TextColor = Color.FromRgb(55, 64, 82)
            };
        }

        if (string.Equals(label, "Accent Note", StringComparison.OrdinalIgnoreCase))
        {
            return new AppliedStyle
            {
                FontFamily = "Arial",
                FontSize = 12f,
                TextColor = Color.FromRgb(122, 74, 25)
            };
        }

        return SubtleTextStyle;
    }

    private static AppliedStyle CreateBlockStyle(BlockType kind)
    {
        var baseStyle = new AppliedStyle { FontFamily = "Arial", FontSize = 12f };

        return kind switch
        {
            BlockType.PageHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, HeaderBlockBottomSpacing)
            },
            BlockType.ReportHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, SectionTitleBlockTopSpacing, 0f, SectionTitleBlockBottomSpacing)
            },
            BlockType.GroupHeader => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 8f, 0f, 4f)
            },
            BlockType.Detail => baseStyle with
            {
                Padding = new Core.Geometry.Thickness(0f, 0f, 0f, DetailBlockBottomSpacing)
            },
            _ => baseStyle
        };
    }

    private static string EvaluateToText(
        IExpressionEvaluator evaluator,
        string expression,
        ExpressionContext context)
    {
        var result = evaluator.Evaluate(expression, context);
        return FormatValue(result);
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "(null)",
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture),
            decimal amount => amount.ToString("N2", CultureInfo.InvariantCulture),
            double number => number.ToString("N2", CultureInfo.InvariantCulture),
            float number => number.ToString("N2", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }
}
