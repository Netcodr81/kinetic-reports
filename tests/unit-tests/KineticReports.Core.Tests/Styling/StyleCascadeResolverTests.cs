namespace KineticReports.Core.Tests.Styling;

using Shouldly;
using Xunit;
using KineticReports.Core.Geometry;
using KineticReports.Core.Styling;

public class StyleCascadeResolverTests
{
    [Fact]
    public void Resolve_WithThemeDefaults_ReturnsThemeValues()
    {
        // Arrange
        var expectedFontFamily = "Arial";
        var expectedFontSize = 12f;

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: expectedFontFamily,
            themeFontSize: expectedFontSize);

        // Assert
        result.FontFamily.ShouldBe(expectedFontFamily);
        result.FontSize.ShouldBe(expectedFontSize);
        result.FontWeight.ShouldBe(FontWeight.Normal);
        result.TextColor.ShouldBe(Color.Black);
    }

    [Fact]
    public void Resolve_WithReportDefaults_OverridesThemeDefaults()
    {
        // Arrange
        var reportDefaults = new StyleDefinition
        {
            Id = "report-default",
            Typography = new Typography
            {
                Family = "Courier",
                Size = 14f
            }
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults);

        // Assert
        result.FontFamily.ShouldBe("Courier");
        result.FontSize.ShouldBe(14f);
    }

    [Fact]
    public void Resolve_WithNamedStyle_OverridesReportDefaults()
    {
        // Arrange
        var reportDefaults = new StyleDefinition
        {
            Id = "report-default",
            Typography = new Typography { Family = "Courier" }
        };

        var namedStyle = new StyleDefinition
        {
            Id = "heading",
            Typography = new Typography
            {
                Family = "Times New Roman",
                Weight = FontWeight.Bold
            }
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults,
            namedStyle: namedStyle);

        // Assert
        result.FontFamily.ShouldBe("Times New Roman");
        result.FontWeight.ShouldBe(FontWeight.Bold);
    }

    [Fact]
    public void Resolve_WithParentStyle_InheritsTextProperties()
    {
        // Arrange: Parent style provides text properties (color, alignment, decoration)
        // but typography (fontFamily, fontSize, weight) are NOT inherited from parent
        var parentStyle = new AppliedStyle
        {
            FontFamily = "Verdana",
            FontSize = 16f,
            FontWeight = FontWeight.SemiBold,
            TextColor = Color.FromRgb(50, 50, 50),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
            TextDecoration = TextDecoration.Underline,
            LineHeight = 1.5f
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            parentStyle: parentStyle);

        // Assert
        // Typography is NOT inherited from parent
        result.FontFamily.ShouldBe("Arial"); // From theme, not parent
        result.FontSize.ShouldBe(12f); // From theme, not parent
        result.FontWeight.ShouldBe(FontWeight.Normal); // From theme, not parent
        result.LineHeight.ShouldBe(1.2f); // From theme default, not parent

        // Text properties ARE inherited from parent
        result.TextColor.ShouldBe(Color.FromRgb(50, 50, 50));
        result.TextAlignment.ShouldBe(TextAlignment.Center);
        result.VerticalAlignment.ShouldBe(VerticalAlignment.Middle);
        result.TextDecoration.ShouldBe(TextDecoration.Underline);
    }

    [Fact]
    public void Resolve_WithLocalOverride_HasHighestPriority()
    {
        // Arrange
        var namedStyle = new StyleDefinition
        {
            Id = "body-text",
            Typography = new Typography
            {
                Family = "Georgia",
                Size = 14f,
                Weight = FontWeight.Normal
            }
        };

        var localOverride = new StyleDefinition
        {
            Id = "local-override",
            Typography = new Typography
            {
                Size = 18f,
                Weight = FontWeight.Bold
            }
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            namedStyle: namedStyle,
            localOverride: localOverride);

        // Assert
        result.FontFamily.ShouldBe("Georgia"); // From named style
        result.FontSize.ShouldBe(18f); // From local override
        result.FontWeight.ShouldBe(FontWeight.Bold); // From local override
    }

    [Fact]
    public void Resolve_WithAllLevels_RespectsCascadePriority()
    {
        // Arrange
        var reportDefaults = new StyleDefinition
        {
            Id = "report",
            Typography = new Typography { Family = "Arial", Size = 12f },
            Background = Color.FromRgb(240, 240, 240),
            Padding = new Thickness(5f)
        };

        var namedStyle = new StyleDefinition
        {
            Id = "heading",
            Typography = new Typography { Size = 16f },
            Background = Color.FromRgb(100, 149, 237)
        };

        var parentStyle = new AppliedStyle
        {
            FontFamily = "Arial",
            FontSize = 16f,
            Background = Color.FromRgb(200, 200, 200),
            Padding = new Thickness(5f),
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyleValue.Normal,
            LineHeight = 1.2f,
            LetterSpacing = 0,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextDecoration = TextDecoration.None,
            TextColor = Color.Black,
            Margin = Thickness.Zero,
            Opacity = 1f,
            Overflow = Overflow.Hidden
        };

        var localOverride = new StyleDefinition
        {
            Id = "local",
            Typography = new Typography { Weight = FontWeight.Bold },
            Padding = new Thickness(10f)
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults,
            namedStyle: namedStyle,
            parentStyle: parentStyle,
            localOverride: localOverride);

        // Assert
        result.FontFamily.ShouldBe("Arial"); // From report defaults
        result.FontSize.ShouldBe(16f); // From named style (not overridden by parent)
        result.FontWeight.ShouldBe(FontWeight.Bold); // From local override
        result.Background.ShouldBe(Color.FromRgb(100, 149, 237)); // From named style (not inherited from parent)
        result.Padding.ShouldBe(new Thickness(10f)); // From local override
    }

    [Fact]
    public void Resolve_WithNullTypographyProperties_FallsBackToPreviousLevel()
    {
        // Arrange
        var namedStyle = new StyleDefinition
        {
            Id = "partial-style",
            Typography = new Typography
            {
                Family = "Consolas"
                // Size, Weight, etc. are null
            }
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            namedStyle: namedStyle);

        // Assert
        result.FontFamily.ShouldBe("Consolas"); // From named style
        result.FontSize.ShouldBe(12f); // Falls back to theme
        result.FontWeight.ShouldBe(FontWeight.Normal); // Falls back to default
    }

    [Fact]
    public void Resolve_WithCustomColors_PreservesColorCascade()
    {
        // Arrange
        var customRed = Color.FromRgb(255, 0, 0);
        var customBlue = Color.FromRgb(0, 0, 255);

        var reportDefaults = new StyleDefinition
        {
            Id = "report",
            Background = customRed
        };

        var namedStyle = new StyleDefinition
        {
            Id = "special",
            Background = customBlue
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            reportDefaults: reportDefaults,
            namedStyle: namedStyle);

        // Assert
        result.Background.ShouldBe(customBlue); // Named style overrides report defaults
    }

    [Fact]
    public void Resolve_WithBorder_CascadesCorrectly()
    {
        // Arrange
        var border = Border.Uniform(2f, Color.Black);

        var localOverride = new StyleDefinition
        {
            Id = "bordered",
            Border = border
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            localOverride: localOverride);

        // Assert
        result.Border.ShouldNotBeNull();
        result.Border.ShouldBe(border);
    }

    [Fact]
    public void Resolve_WithMarginAndPadding_CascadesCorrectly()
    {
        // Arrange
        var margin = new Thickness(10, 15, 10, 15);
        var padding = new Thickness(5, 5, 5, 5);

        var namedStyle = new StyleDefinition
        {
            Id = "spaced",
            Margin = margin,
            Padding = padding
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            namedStyle: namedStyle);

        // Assert
        result.Margin.ShouldBe(margin);
        result.Padding.ShouldBe(padding);
    }

    [Fact]
    public void Resolve_WithOpacityAndOverflow_CascadesCorrectly()
    {
        // Arrange
        var namedStyle = new StyleDefinition
        {
            Id = "translucent",
            Opacity = 0.8f,
            Overflow = Overflow.Visible
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 12f,
            namedStyle: namedStyle);

        // Assert
        result.Opacity.ShouldBe(0.8f);
        result.Overflow.ShouldBe(Overflow.Visible);
    }

    [Fact]
    public void Resolve_ReturnsAppliedStyleWithAllRequiredProperties()
    {
        // Act
        var result = StyleCascadeResolver.Resolve();

        // Assert
        result.FontFamily.ShouldNotBeNullOrEmpty();
        result.FontSize.ShouldBeGreaterThan(0);
        result.FontWeight.ShouldBe(FontWeight.Normal);
        result.FontStyle.ShouldBe(FontStyleValue.Normal);
        result.TextColor.ShouldBe(Color.Black);
        result.TextAlignment.ShouldBe(TextAlignment.Left);
        result.VerticalAlignment.ShouldBe(VerticalAlignment.Top);
    }

    [Fact]
    public void Resolve_ResultIsImmutable()
    {
        // Act
        var result1 = StyleCascadeResolver.Resolve();
        var result2 = result1 with { FontSize = 16f };

        // Assert
        result1.FontSize.ShouldNotBe(16f);
        result2.FontSize.ShouldBe(16f);
    }

    [Fact]
    public void Resolve_WithComplexHierarchy_ShouldCorrectlyMergeAllLevels()
    {
        // Arrange: Create a realistic hierarchy
        var reportDefaults = new StyleDefinition
        {
            Id = "report-base",
            Typography = new Typography
            {
                Family = "Segoe UI",
                Size = 11f
            },
            Background = Color.FromRgb(240, 240, 240),
            Padding = new Thickness(5f)
        };

        var tableHeaderStyle = new StyleDefinition
        {
            Id = "table-header",
            Typography = new Typography
            {
                Weight = FontWeight.Bold,
                Size = 12f
            },
            Background = Color.FromRgb(31, 58, 111),
            Padding = new Thickness(8f)
        };

        var parentTableStyle = new AppliedStyle
        {
            FontFamily = "Segoe UI",
            FontSize = 11f,
            Background = Color.FromRgb(240, 240, 240),
            Padding = new Thickness(5f),
            TextColor = Color.FromRgb(64, 64, 64),
            FontWeight = FontWeight.Normal,
            FontStyle = FontStyleValue.Normal,
            LineHeight = 1.2f,
            LetterSpacing = 0,
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            TextDecoration = TextDecoration.None,
            Margin = Thickness.Zero,
            Opacity = 1f,
            Overflow = Overflow.Hidden
        };

        var cellOverride = new StyleDefinition
        {
            Id = "cell-override",
            Padding = new Thickness(6f)
        };

        // Act
        var result = StyleCascadeResolver.Resolve(
            themeFontFamily: "Arial",
            themeFontSize: 10f,
            reportDefaults: reportDefaults,
            namedStyle: tableHeaderStyle,
            parentStyle: parentTableStyle,
            localOverride: cellOverride);

        // Assert
        result.FontFamily.ShouldBe("Segoe UI"); // From parent
        result.FontSize.ShouldBe(12f); // From named style
        result.FontWeight.ShouldBe(FontWeight.Bold); // From named style
        result.Background.ShouldBe(Color.FromRgb(31, 58, 111)); // From named style
        result.Padding.ShouldBe(new Thickness(6f)); // From local override
    }
}
