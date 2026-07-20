# Rendering Pipeline Specification

**Status:** Draft v0.1

## Purpose

This specification defines how a computed LayoutTree is transformed into
device-specific output while guaranteeing consistent visual results
across all renderers.

------------------------------------------------------------------------

# Rendering Philosophy

The rendering pipeline **never performs layout**.

Responsibilities are strictly separated:

    Report Definition
          ↓
    Engine
          ↓
    Layout Tree (Immutable)
          ↓
    Renderer
          ↓
    Graphics Context
          ↓
    Output Device

------------------------------------------------------------------------

# Pipeline Stages

## Stage 1 -- Resource Resolution

Resolve fonts, images, colors, brushes, and reusable resources.

## Stage 2 -- Render Tree Traversal

Traverse the immutable LayoutTree in deterministic order.

## Stage 3 -- Draw Commands

Convert layout elements into graphics primitives:

-   DrawText
-   DrawImage
-   DrawRectangle
-   DrawEllipse
-   DrawPath
-   DrawLine
-   PushClip
-   PopClip
-   PushTransform
-   PopTransform

## Stage 4 -- Backend Translation

Each backend converts primitives into native commands.

Supported backends:

-   SkiaSharp
-   HTML/CSS
-   PDF
-   SVG

Future:

-   Canvas2D
-   WebGPU
-   Direct2D

------------------------------------------------------------------------

# Rendering Order

1.  Page background
2.  Containers
3.  Shapes
4.  Images
5.  Text
6.  Charts
7.  Barcodes
8.  Foreground overlays

Children always render after parents.

------------------------------------------------------------------------

# Graphics Abstraction

Every renderer targets:

``` csharp
interface IGraphicsContext
{
    void DrawText(...);
    void DrawImage(...);
    void DrawPath(...);
    void FillRectangle(...);
    void PushClip(...);
    void PopClip();
    void PushTransform(...);
    void PopTransform();
}
```

No renderer accesses report objects directly.

------------------------------------------------------------------------

# Text Rendering

Requirements

-   Unicode
-   RTL
-   BiDi
-   Font fallback
-   Ligatures
-   Kerning
-   Hinting
-   Sub-pixel positioning

Text measurement must match layout metrics.

------------------------------------------------------------------------

# Image Pipeline

Supported formats:

-   PNG
-   JPEG
-   SVG
-   WebP

Future:

-   AVIF
-   HEIF

Images are decoded once and cached.

------------------------------------------------------------------------

# Layers

Every element belongs to a layer.

Future support:

-   Blend modes
-   Opacity groups
-   Effects
-   Masks

------------------------------------------------------------------------

# Clipping

Clip regions are hierarchical.

Rules:

-   Child clips inherit parent clips.
-   Clips never expand.
-   Nested clips intersect.

------------------------------------------------------------------------

# Transparency

Opacity is inherited.

EffectiveOpacity = ParentOpacity × LocalOpacity

------------------------------------------------------------------------

# Resource Caching

Caches:

-   Fonts
-   Glyphs
-   Brushes
-   Images
-   Pens
-   Paths

Goals:

-   Zero duplicate decoding
-   Shared immutable resources
-   Thread-safe lookup

------------------------------------------------------------------------

# Performance

Targets

-   O(n) tree traversal
-   Zero layout calculations
-   Streaming page rendering
-   Lazy image decoding
-   Incremental rendering

------------------------------------------------------------------------

# Renderer Contract

Input:

-   Immutable LayoutTree

Output:

-   Drawing commands only

Forbidden:

-   SQL access
-   Expression evaluation
-   Layout changes
-   Pagination

------------------------------------------------------------------------

# Error Handling

Missing fonts: → Fallback chain

Missing images: → Placeholder rendering

Unknown controls: → Plugin resolution → Placeholder if unavailable

------------------------------------------------------------------------

# Testing

Golden image comparison

PDF byte validation

HTML snapshot tests

Cross-render consistency tests

Performance benchmarks

------------------------------------------------------------------------

# ADRs

ADR-021 Renderers never mutate layout.

ADR-022 All rendering targets IGraphicsContext.

ADR-023 Rendering order is deterministic.

ADR-024 Shared resource caches are immutable.

ADR-025 Renderers are stateless.

------------------------------------------------------------------------

# Future Specifications

-   Font Engine
-   Text Measurement
-   HTML Renderer
-   Skia Backend
-   PDF Backend
-   Excel Export Architecture
-   Viewer Specification
