# Style System Specification

**Specification Version:** 0.1 **Status:** Draft

# Purpose

The Style System defines the visual appearance of every report object.
It provides a deterministic, CSS-inspired cascading model while
remaining optimized for report generation.

------------------------------------------------------------------------

# Goals

-   Consistent styling across all renderers
-   Immutable resolved styles
-   Theme support
-   Fast lookup
-   Minimal duplication
-   Export-independent

------------------------------------------------------------------------

# Architecture

``` text
Theme
   ↓
Report Defaults
   ↓
Named Style
   ↓
Inherited Style
   ↓
Local Overrides
   ↓
Resolved Style (Immutable)
```

The renderer consumes only resolved styles.

------------------------------------------------------------------------

# Style Categories

## Typography

-   Font Family
-   Font Size
-   Font Weight
-   Font Style
-   Line Height
-   Letter Spacing
-   Word Spacing
-   Text Color
-   Text Alignment
-   Vertical Alignment
-   Text Decoration

## Box Model

-   Margin
-   Border
-   Padding
-   Background
-   Corner Radius

## Layout

-   Width
-   Height
-   Min/Max Width
-   Min/Max Height
-   Visibility
-   Overflow
-   Opacity
-   Rotation
-   ZIndex

------------------------------------------------------------------------

# Style Inheritance

Inheritance order:

1.  Theme
2.  Report
3.  Parent Container
4.  Named Style
5.  Conditional Style
6.  Local Override

Rules:

-   Explicit values replace inherited values.
-   Null means "inherit".
-   Unset values fall back to defaults.

------------------------------------------------------------------------

# Themes

A theme contains:

-   Color palette
-   Typography
-   Border presets
-   Spacing scale
-   Default styles

Themes are versioned and may be shared across reports.

------------------------------------------------------------------------

# Conditional Styles

Conditional styles evaluate expressions.

Example:

    If(Order.Total > 1000)
        Background = Red

Conditional styles execute after base style resolution but before
rendering.

------------------------------------------------------------------------

# Color Model

Internal representation:

RGBA

Supported input formats:

-   Hex
-   RGB
-   RGBA
-   Named colors

Future: - HSL - Theme variables

------------------------------------------------------------------------

# Font Resolution

Resolution pipeline:

Requested Font ↓ Installed Font ↓ Fallback Chain ↓ Default Font

Glyph fallback occurs per text run.

------------------------------------------------------------------------

# Borders

Each edge supports:

-   Width
-   Style
-   Color

Styles:

-   Solid
-   Dashed
-   Dotted
-   Double

Future: - Custom dash patterns

------------------------------------------------------------------------

# Brushes

Supported:

-   Solid
-   Linear Gradient
-   Radial Gradient

Future:

-   Image brushes
-   Pattern brushes

------------------------------------------------------------------------

# Spacing

All spacing is stored as Device Independent Pixels.

Supported units during authoring:

-   px
-   pt
-   mm
-   cm
-   in

------------------------------------------------------------------------

# Style Resolution Cache

Resolved styles are cached by hash.

Key:

Theme + Named Style + Overrides + Conditional State

Resolved styles are immutable and reusable.

------------------------------------------------------------------------

# Performance Targets

-   Constant-time style lookup after resolution
-   Zero renderer-side inheritance
-   Shared immutable instances

------------------------------------------------------------------------

# Accessibility

Themes SHOULD support:

-   High contrast
-   Large fonts
-   Color-blind friendly palettes

------------------------------------------------------------------------

# Testing

-   Style inheritance
-   Theme overrides
-   Conditional formatting
-   Font fallback
-   Border rendering
-   Gradient rendering
-   Cache correctness

------------------------------------------------------------------------

# ADRs

ADR-036 Resolved styles are immutable.

ADR-037 Renderers never evaluate inheritance.

ADR-038 Conditional styles execute before rendering.

ADR-039 Themes are versioned assets.

ADR-040 Font fallback is centralized.

------------------------------------------------------------------------

# Future Specifications

-   Plugin SDK
-   Runtime Context
-   Graphics & Typography
-   Viewer Architecture
-   Uno Designer Property System
