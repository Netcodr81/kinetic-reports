namespace KineticReports.Core.Tests.Styling;

using Shouldly;
using Xunit;
using KineticReports.Core.Definition;
using KineticReports.Core.Geometry;
using KineticReports.Core.Layout;
using KineticReports.Core.Styling;

/// <summary>
/// Integration tests for StyleCascadeResolver with real ReportDefinition and layout element scenarios.
/// These tests verify that the cascade resolver integrates correctly with the report definition
/// and layout engine, ensuring styles cascade properly through realistic workflows.
/// </summary>
public class StyleCascadeResolverIntegrationTests
{
    /// <summary>
    /// Tests that a named style defined in a ReportDefinition can be resolved and applied to a text block.
    /// </summary>
    [Fact]
    public void Cascade_WithNamedStyleInReportDefinition_ResolvesToTextBlockStyle()
    {
        // Arrange: Create a report with a named heading style
        var headingStyle = new StyleDefinition
        {
            Id = "heading1",
            Name = "Heading 1",
            Typography = new Typography
            {
                Family = "Georgia",
                Size = 24f,
                Weight = FontWeight.Bold
            },
            Margin = new Thickness(0, 10, 0, 10)
        };

        var reportDef = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "test-report",
            Name = "Test Report",
            Styles = new[] { headingStyle }
        };

        // Act: Resolve the style cascade for the heading
        var namedStyle = reportDef.Styles?.FirstOrDefault(s => s.Id == "heading1");
        var resolvedStyle = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: null,
            namedStyle: namedStyle,
            parentStyle: null,
            localOverride: null);

        // Create a text block with the resolved style
        var textBlock = new TextBlock
        {
            Id = "title",
            Style = resolvedStyle,
            Text = "Report Title"
        };

        // Assert
        textBlock.Style.FontFamily.ShouldBe("Georgia");
        textBlock.Style.FontSize.ShouldBe(24f);
        textBlock.Style.FontWeight.ShouldBe(FontWeight.Bold);
        textBlock.Style.Margin.ShouldBe(new Thickness(0, 10, 0, 10));
    }

    /// <summary>
    /// Tests cascade through a container hierarchy: parent container style flows to child element
    /// text properties only.
    /// </summary>
    [Fact]
    public void Cascade_WithContainerHierarchy_ChildInheritsParentTextProperties()
    {
        // Arrange: Parent container with styled background
        var parentAppliedStyle = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f,
            TextColor = Color.FromRgb(64, 64, 64),
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Background = Color.FromRgb(240, 240, 240),
            Padding = new Thickness(10),
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyleValue.Normal,
            LineHeight = 1.2f,
            LetterSpacing = 0,
            TextDecoration = TextDecoration.None,
            Margin = Thickness.Zero,
            Border = null,
            Opacity = 1f,
            Overflow = Overflow.Hidden
        };

        // Child element with local text size override
        var childLocalStyle = new StyleDefinition
        {
            Id = "child-override",
            Typography = new Typography { Size = 14f }
        };

        // Act: Cascade from parent + local override
        var resolvedStyle = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: null,
            namedStyle: null,
            parentStyle: parentAppliedStyle,
            localOverride: childLocalStyle);

        // Assert: Typography updated from local, text properties inherited from parent
        resolvedStyle.FontSize.ShouldBe(14f); // From local override
        resolvedStyle.FontFamily.ShouldBe("Arial"); // From theme (NOT parent)
        resolvedStyle.TextColor.ShouldBe(Color.FromRgb(64, 64, 64)); // Inherited from parent
        resolvedStyle.TextAlignment.ShouldBe(TextAlignment.Left); // Inherited from parent

        // Box model NOT inherited from parent
        resolvedStyle.Background.ShouldBe(Color.Transparent); // Not inherited
        resolvedStyle.Padding.ShouldBe(Thickness.Zero); // Not inherited
    }

    /// <summary>
    /// Tests report-level default styles that apply to all elements unless overridden.
    /// This simulates a report configuration like "all body text uses Segoe UI 11pt".
    /// </summary>
    [Fact]
    public void Cascade_WithReportDefaults_ApplyToAllElementsUnlessOverridden()
    {
        // Arrange: Report defines default font for all text
        var reportDefaults = new StyleDefinition
        {
            Id = "report-defaults",
            Typography = new Typography
            {
                Family = "Segoe UI",
                Size = 11f
            }
        };

        // Named heading style overrides report defaults
        var headingStyle = new StyleDefinition
        {
            Id = "heading",
            Typography = new Typography
            {
                Size = 18f,
                Weight = FontWeight.Bold
            }
        };

        // Act: Resolve body text (uses report defaults)
        var bodyResolved = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults);

        // Resolve heading (overrides report defaults with named style)
        var headingResolved = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults,
            namedStyle: headingStyle);

        // Assert
        bodyResolved.FontFamily.ShouldBe("Segoe UI");
        bodyResolved.FontSize.ShouldBe(11f);
        bodyResolved.FontWeight.ShouldBe(FontWeight.Normal);

        headingResolved.FontFamily.ShouldBe("Segoe UI"); // Inherited from report defaults
        headingResolved.FontSize.ShouldBe(18f); // Overridden by named style
        headingResolved.FontWeight.ShouldBe(FontWeight.Bold); // From named style
    }

    /// <summary>
    /// Tests a complete style cascade through a multi-level report structure.
    /// Simulates: Report Default → Named Style (table) → Parent Style (row) → Local Override (cell)
    /// </summary>
    [Fact]
    public void Cascade_MultiLevel_TableStructureWithRowAndCellStyling()
    {
        // Arrange: Report-level defaults
        var reportDefaults = new StyleDefinition
        {
            Id = "report-defaults",
            Typography = new Typography
            {
                Family = "Arial",
                Size = 11f
            }
        };

        // Named style for table rows (slightly larger, light gray background)
        var rowStyle = new StyleDefinition
        {
            Id = "table-row",
            Typography = new Typography { Size = 12f },
            Background = Color.FromRgb(240, 240, 240),
            Padding = new Thickness(5)
        };

        // Parent applied style (after row layout resolves)
        var rowAppliedStyle = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 12f,
            TextColor = Color.FromRgb(64, 64, 64),
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Middle,
            Background = Color.FromRgb(240, 240, 240),
            Padding = new Thickness(5),
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyleValue.Normal,
            LineHeight = 1.2f,
            LetterSpacing = 0,
            TextDecoration = TextDecoration.None,
            Margin = Thickness.Zero,
            Border = null,
            Opacity = 1f,
            Overflow = Overflow.Hidden
        };

        // Local override for a specific cell (right-align, bold)
        var cellOverride = new StyleDefinition
        {
            Id = "cell-total",
            Typography = new Typography { Weight = FontWeight.Bold },
            Padding = new Thickness(8)
        };

        // Act: Cascade through the structure
        var cellResolved = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults,
            namedStyle: rowStyle,
            parentStyle: rowAppliedStyle,
            localOverride: cellOverride);

        // Assert
        cellResolved.FontFamily.ShouldBe("Arial");
        cellResolved.FontSize.ShouldBe(12f);
        cellResolved.TextColor.ShouldBe(Color.FromRgb(64, 64, 64));
        cellResolved.TextAlignment.ShouldBe(TextAlignment.Left);
        cellResolved.VerticalAlignment.ShouldBe(VerticalAlignment.Middle);
        cellResolved.Background.ShouldBe(Color.FromRgb(240, 240, 240));
        cellResolved.Padding.ShouldBe(new Thickness(8)); // From cell override
        cellResolved.FontWeight.ShouldBe(FontWeight.Bold);
    }

    /// <summary>
    /// Tests that a complete report definition with multiple named styles works correctly
    /// for real report workflow scenarios.
    /// </summary>
    [Fact]
    public void Cascade_CompleteReportScenario_MultipleNamedStylesAndElements()
    {
        // Arrange: Create a complete report definition with named styles
        var heading1Style = new StyleDefinition
        {
            Id = "Heading1",
            Name = "Heading 1",
            Typography = new Typography
            {
                Family = "Verdana",
                Size = 20f,
                Weight = FontWeight.Bold
            },
            Margin = new Thickness(0, 0, 0, 12)
        };

        var bodyStyle = new StyleDefinition
        {
            Id = "Body",
            Name = "Body Text",
            Typography = new Typography
            {
                Family = "Calibri",
                Size = 11f
            }
        };

        var emphasisStyle = new StyleDefinition
        {
            Id = "Emphasis",
            Name = "Emphasized Text",
            Typography = new Typography { Style = FontStyleValue.Italic }
        };

        var reportDef = new ReportDefinition
        {
            SchemaVersion = "1.0",
            Id = "styled-report",
            Name = "Styled Report",
            Styles = new[] { heading1Style, bodyStyle, emphasisStyle }
        };

        // Act & Assert: Verify each style resolves correctly
        var heading1Resolved = ResolveStyleFromReport(reportDef, "Heading1");
        heading1Resolved.FontFamily.ShouldBe("Verdana");
        heading1Resolved.FontSize.ShouldBe(20f);
        heading1Resolved.FontWeight.ShouldBe(FontWeight.Bold);
        heading1Resolved.Margin.ShouldBe(new Thickness(0, 0, 0, 12));

        var bodyResolved = ResolveStyleFromReport(reportDef, "Body");
        bodyResolved.FontFamily.ShouldBe("Calibri");
        bodyResolved.FontSize.ShouldBe(11f);

        var emphasisResolved = ResolveStyleFromReport(reportDef, "Emphasis");
        emphasisResolved.FontStyle.ShouldBe(FontStyleValue.Italic);
        emphasisResolved.FontFamily.ShouldBe("Arial"); // Fallback to theme
    }

    /// <summary>
    /// Tests that local overrides properly override all previous levels in the cascade.
    /// </summary>
    [Fact]
    public void Cascade_LocalOverride_OverridesAllPreviousLevels()
    {
        // Arrange: Create a complex cascade with many levels
        var reportDefaults = new StyleDefinition
        {
            Id = "defaults",
            Typography = new Typography { Family = "Times New Roman", Size = 12f },
            Background = Color.FromRgb(200, 200, 200)
        };

        var namedStyle = new StyleDefinition
        {
            Id = "special",
            Typography = new Typography { Family = "Georgia", Size = 14f },
            Background = Color.FromRgb(150, 150, 150)
        };

        var parentStyle = new AppliedStyle
        {
            FontFamily = "Georgia",
            FontSize = 14f,
            TextColor = Color.White,
            Background = Color.FromRgb(100, 100, 100),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
            FontWeight = FontWeight.Bold,
            FontStyle = FontStyleValue.Normal,
            LineHeight = 1.5f,
            LetterSpacing = 0,
            TextDecoration = TextDecoration.None,
            Margin = Thickness.Zero,
            Padding = Thickness.Zero,
            Border = null,
            Opacity = 1f,
            Overflow = Overflow.Hidden
        };

        var localOverride = new StyleDefinition
        {
            Id = "override",
            Typography = new Typography { Family = "Courier New", Size = 10f },
            Background = Color.FromRgb(50, 50, 50)
        };

        // Act
        var resolved = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 11f,
            reportDefaults: reportDefaults,
            namedStyle: namedStyle,
            parentStyle: parentStyle,
            localOverride: localOverride);

        // Assert: Local override takes highest priority
        resolved.FontFamily.ShouldBe("Courier New");
        resolved.FontSize.ShouldBe(10f);
        resolved.Background.ShouldBe(Color.FromRgb(50, 50, 50));

        // Text properties from parent still inherited
        resolved.TextColor.ShouldBe(Color.White);
        resolved.TextAlignment.ShouldBe(TextAlignment.Center);
    }

    // Helper method: Simulates engine resolution of a style by name from report definition
    private static AppliedStyle ResolveStyleFromReport(
        ReportDefinition reportDef,
        string styleName,
        AppliedStyle? parentStyle = null)
    {
        var namedStyle = reportDef.Styles?.FirstOrDefault(s => s.Id == styleName);

        return StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: null,
            namedStyle: namedStyle,
            parentStyle: parentStyle,
            localOverride: null);
    }
}
