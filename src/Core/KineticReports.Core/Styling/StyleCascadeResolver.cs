namespace KineticReports.Core.Styling;

using KineticReports.Core.Geometry;

/// <summary>
/// Resolves the final applied style by combining theme defaults, report styles, inheritance, and local overrides.
/// Centralizes the 6-level style cascade documented in spec §10.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Style Cascade Levels (Priority Order, Lowest to Highest):</strong>
/// </para>
/// <list type="number">
/// <item>
///   <term>Theme Defaults</term>
///   <description>
///   Base styles provided by the report theme or engine defaults (e.g., Arial 12pt, black text).
///   Used when no other level provides a value.
///   </description>
/// </item>
/// <item>
///   <term>Report Defaults</term>
///   <description>
///   Default styles specified at the report level (e.g., "use Courier for all code blocks").
///   Overrides theme defaults but can be overridden by named styles.
///   </description>
/// </item>
/// <item>
///   <term>Named Style</term>
///   <description>
///   Styles defined in the report's style catalog and referenced by Id (e.g., "heading1", "body-text").
///   Supports inheritance via <see cref="StyleDefinition.BasedOn"/>.
///   </description>
/// </item>
/// <item>
///   <term>Parent Inheritance</term>
///   <description>
///   Styles inherited from a parent container (e.g., child text blocks inherit parent's text color).
///   Provides visual consistency within a section.
///   </description>
/// </item>
/// <item>
///   <term>Local Override</term>
///   <description>
///   Style properties explicitly set on the individual element (highest priority).
///   Allows per-element customization without changing the named style.
///   </description>
/// </item>
/// <item>
///   <term>Applied Style (Result)</term>
///   <description>
///   The final immutable <see cref="AppliedStyle"/> after all cascade levels are merged.
///   This is the only style format consumed by the layout engine and renderers.
///   </description>
/// </item>
/// </list>
/// 
/// <para>
/// <strong>Cascade Semantics:</strong>
/// <list type="bullet">
/// <item>
///   Properties are merged, not replaced—if a level doesn't specify a property, the previous level's value is used.
/// </item>
/// <item>
///   Null properties are treated as "not specified" and cascade down to the next level.
/// </item>
/// <item>
///   The final <see cref="AppliedStyle"/> has no null values—all properties have defaults.
/// </item>
/// <item>
///   Once created, <see cref="AppliedStyle"/> is immutable and cannot be changed.
/// </item>
/// </list>
/// </para>
/// 
/// <para>
/// <strong>Example Cascade Flow:</strong>
/// <code>
/// Theme Font: Arial 12pt
/// Report Default: Courier (inherits 12pt)
/// Named Style "code": Courier (already set) + Bold
/// Parent Style: Bold (already set)
/// Local Override: FontSize = 14pt (overrides default 12pt)
/// Result: Courier 14pt Bold
/// </code>
/// </para>
/// </remarks>
public sealed class StyleCascadeResolver
{
    /// <summary>
    /// Resolves the final applied style by combining theme, report, named, and local styles.
    /// </summary>
    /// <param name="themeFontFamily">Theme default font family (fallback).</param>
    /// <param name="themeFontSize">Theme default font size in points (fallback).</param>
    /// <param name="reportDefaults">Report-level style defaults, or <see langword="null"/> if none.</param>
    /// <param name="namedStyle">Named style (from style catalog), or <see langword="null"/> if not referenced.</param>
    /// <param name="parentStyle">Parent element's applied style for inheritance, or <see langword="null"/> if no parent.</param>
    /// <param name="localOverride">Local element-specific style overrides, or <see langword="null"/> if none.</param>
    /// <returns>
    /// An immutable <see cref="AppliedStyle"/> with all properties fully resolved.
    /// All properties have non-null values or defaults.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements the 6-level cascade in order:
    /// </para>
    /// <list type="number">
    /// <item>Start with theme defaults (fontFamily, fontSize always provided)</item>
    /// <item>Merge report defaults (reportDefaults)</item>
    /// <item>Merge named style (namedStyle)</item>
    /// <item>Merge parent inheritance (parentStyle)</item>
    /// <item>Merge local overrides (localOverride)</item>
    /// <item>Apply final defaults for any remaining null values</item>
    /// </list>
    /// <para>
    /// The caller should determine which level each style parameter represents.
    /// For simple use cases, you can call this with just theme, report, and local:
    /// <code>
    /// var resolved = StyleCascadeResolver.Resolve(
    ///     themeFontFamily: "Arial",
    ///     themeFontSize: 12f,
    ///     reportDefaults: null,
    ///     namedStyle: null,
    ///     parentStyle: null,
    ///     localOverride: myLocalStyle
    /// );
    /// </code>
    /// </para>
    /// </remarks>
    public static AppliedStyle Resolve(
        string themeFontFamily = "Arial",
        float themeFontSize = 12f,
        StyleDefinition? reportDefaults = null,
        StyleDefinition? namedStyle = null,
        AppliedStyle? parentStyle = null,
        StyleDefinition? localOverride = null)
    {
        // Level 1: Theme defaults
        string fontFamily = themeFontFamily;
        float fontSize = themeFontSize;
        FontWeight fontWeight = FontWeight.Normal;
        FontStyleValue fontStyle = FontStyleValue.Normal;
        float lineHeight = 1.2f;
        float letterSpacing = 0f;
        Color textColor = Color.Black;
        TextAlignment textAlignment = TextAlignment.Left;
        VerticalAlignment verticalAlignment = VerticalAlignment.Top;
        TextDecoration textDecoration = TextDecoration.None;
        Border? border = null;
        Color background = Color.Transparent;
        Thickness margin = Thickness.Zero;
        Thickness padding = Thickness.Zero;
        float opacity = 1f;
        Overflow overflow = Overflow.Hidden;
        if (reportDefaults != null)
        {
            fontFamily = MergeStringValue(fontFamily, reportDefaults.Typography?.Family);
            fontSize = MergeFloatValue(fontSize, reportDefaults.Typography?.Size);
            fontWeight = MergeFontWeightValue(fontWeight, reportDefaults.Typography?.Weight);
            fontStyle = MergeFontStyleValue(fontStyle, reportDefaults.Typography?.Style);
            lineHeight = MergeFloatValue(lineHeight, reportDefaults.Typography?.LineHeight);
            border = MergeBorderValue(border, reportDefaults.Border);
            background = MergeColorValue(background, reportDefaults.Background);
            margin = MergeThicknessValue(margin, reportDefaults.Margin);
            padding = MergeThicknessValue(padding, reportDefaults.Padding);
            opacity = MergeFloatValue(opacity, reportDefaults.Opacity);
            overflow = MergeOverflowValue(overflow, reportDefaults.Overflow);
        }

        // Level 3: Named style
        if (namedStyle != null)
        {
            fontFamily = MergeStringValue(fontFamily, namedStyle.Typography?.Family);
            fontSize = MergeFloatValue(fontSize, namedStyle.Typography?.Size);
            fontWeight = MergeFontWeightValue(fontWeight, namedStyle.Typography?.Weight);
            fontStyle = MergeFontStyleValue(fontStyle, namedStyle.Typography?.Style);
            lineHeight = MergeFloatValue(lineHeight, namedStyle.Typography?.LineHeight);
            border = MergeBorderValue(border, namedStyle.Border);
            background = MergeColorValue(background, namedStyle.Background);
            margin = MergeThicknessValue(margin, namedStyle.Margin);
            padding = MergeThicknessValue(padding, namedStyle.Padding);
            opacity = MergeFloatValue(opacity, namedStyle.Opacity);
            overflow = MergeOverflowValue(overflow, namedStyle.Overflow);
        }

        // Level 4: Parent inheritance
        if (parentStyle != null)
        {
            // Inherit text properties only, not typography or box model
            textColor = parentStyle.TextColor;
            textAlignment = parentStyle.TextAlignment;
            verticalAlignment = parentStyle.VerticalAlignment;
            textDecoration = parentStyle.TextDecoration;
            // Note: fontFamily, fontSize, fontWeight, fontStyle, lineHeight are NOT inherited from parent
            // Note: border, background, margin, padding do NOT inherit from parent
        }

        // Level 5: Local overrides
        if (localOverride != null)
        {
            fontFamily = MergeStringValue(fontFamily, localOverride.Typography?.Family);
            fontSize = MergeFloatValue(fontSize, localOverride.Typography?.Size);
            fontWeight = MergeFontWeightValue(fontWeight, localOverride.Typography?.Weight);
            fontStyle = MergeFontStyleValue(fontStyle, localOverride.Typography?.Style);
            lineHeight = MergeFloatValue(lineHeight, localOverride.Typography?.LineHeight);
            border = MergeBorderValue(border, localOverride.Border);
            background = MergeColorValue(background, localOverride.Background);
            margin = MergeThicknessValue(margin, localOverride.Margin);
            padding = MergeThicknessValue(padding, localOverride.Padding);
            opacity = MergeFloatValue(opacity, localOverride.Opacity);
            overflow = MergeOverflowValue(overflow, localOverride.Overflow);
        }

        // Level 6: Final defaults and construct immutable AppliedStyle
        return new AppliedStyle
        {
            FontFamily = fontFamily,
            FontSize = fontSize,
            FontWeight = fontWeight,
            FontStyle = fontStyle,
            LineHeight = lineHeight,
            LetterSpacing = letterSpacing,
            TextColor = textColor,
            TextAlignment = textAlignment,
            VerticalAlignment = verticalAlignment,
            TextDecoration = textDecoration,
            Border = border,
            Background = background,
            Margin = margin,
            Padding = padding,
            Opacity = opacity,
            Overflow = overflow
        };
    }

    // Merge helper methods: if the new value is not null, use it; otherwise keep the current value.
    private static string MergeStringValue(string current, string? newValue) =>
        newValue ?? current;

    private static float MergeFloatValue(float current, float? newValue) =>
        newValue ?? current;

    private static FontWeight MergeFontWeightValue(FontWeight current, FontWeight? newValue) =>
        newValue ?? current;

    private static FontStyleValue MergeFontStyleValue(FontStyleValue current, FontStyleValue? newValue) =>
        newValue ?? current;

    private static Overflow MergeOverflowValue(Overflow current, Overflow? newValue) =>
        newValue ?? current;

    private static Color MergeColorValue(Color current, Color? newValue) =>
        newValue ?? current;

    private static Thickness MergeThicknessValue(Thickness current, Thickness? newValue) =>
        newValue ?? current;

    private static Border? MergeBorderValue(Border? current, Border? newValue) =>
        newValue ?? current;
}
