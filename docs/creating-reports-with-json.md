# Creating Reports with JSON

This guide is a practical, complete reference for authoring report definitions as JSON.

Goal: a new user should be able to create, validate, execute, and export a report without knowing internal engine details.

## 1. What JSON authoring gives you

Use JSON authoring when you want:

- Report definitions stored as files (versionable and portable)
- Report editing outside compiled code
- Dynamic report loading at runtime

The JSON format maps to the canonical `ReportDefinition` model and is deserialized by `IReportDefinitionSerializer`.

## 2. Required setup

Register engine services and authoring serializer:

```csharp
using KineticReports.Core.Authoring.DependencyInjection;
using KineticReports.Core.Engine.DependencyInjection;

builder.Services.AddKineticReportsAuthoring();
builder.Services.AddKineticReportsEngine();
```

Run a report from JSON:

```csharp
using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Engine;

var json = await File.ReadAllTextAsync("reports/quarterly.report.json", ct);

var serializer = services.GetRequiredService<IReportDefinitionSerializer>();
var engine = services.GetRequiredService<IReportEngine>();

var definition = serializer.Deserialize(json);
var document = await engine.RunAsync(definition, ct);
```

Notes:

- JSON property names are camelCase.
- Enums are string values.
- Duplicate JSON keys are rejected during deserialization.

## 3. Complete JSON skeleton

```json
{
  "schemaVersion": "1.0",
  "id": "sales-quarterly-v1",
  "name": "Quarterly Sales Report",
  "author": "Reporting Team",
  "description": "Regional sales summary",
  "metadata": {
    "department": "Finance",
    "templateVersion": "2026.07"
  },
  "parameters": [],
  "dataSources": [],
  "styles": [],
  "plugins": [],
  "layout": {
    "pageHeader": [],
    "body": [],
    "pageFooter": []
  }
}
```

## 4. Top-level property reference

| Property | Type | Required | Description |
|---|---|---|---|
| `schemaVersion` | string | Yes | Schema marker, for example `1.0` |
| `id` | string | Yes | Stable unique report id |
| `name` | string | Yes | Report display name |
| `author` | string | No | Author display text |
| `description` | string | No | Report purpose/summary |
| `metadata` | object | No | Arbitrary key/value metadata bag |
| `parameters` | array | No | `ParameterDefinition` list |
| `dataSources` | array | No | `DataSourceDefinition` list |
| `styles` | array | No | Named reusable style definitions |
| `plugins` | array | No | Optional plugin enable/disable switches |
| `layout` | object | No | Canonical layout definition (recommended) |

## 5. Parameters

`parameters[]` shape:

```json
{
  "id": "asOfDate",
  "name": "As Of Date",
  "type": "DateTime",
  "isRequired": true,
  "defaultValue": "2026-06-30",
  "description": "Last date included in report"
}
```

| Property | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Parameter key used at runtime |
| `name` | string | Yes | Display name |
| `type` | enum string | No | Default `String` |
| `isRequired` | bool | No | Default `false` |
| `defaultValue` | string | No | String representation |
| `description` | string | No | User-facing guidance |

Supported `type` values:

- `String`
- `Integer`
- `Decimal`
- `Boolean`
- `DateTime`
- `Guid`
- `Enum`

## 6. Data sources

`dataSources[]` shape:

```json
{
  "id": "orders",
  "name": "Orders",
  "providerType": "SqlServer",
  "sourceName": "SalesDb",
  "properties": {
    "query": "SELECT Region, CustomerName, Amount FROM dbo.Orders"
  }
}
```

| Property | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Unique data source key |
| `name` | string | Yes | Display name |
| `providerType` | string | Yes | Provider family hint |
| `sourceName` | string | Yes | Registered provider source name |
| `properties` | object | No | Provider-specific key/value map |

Security note: do not store secrets in report JSON.

## 7. Styles

`styles[]` entries use `StyleDefinition`:

```json
{
  "id": "header-text",
  "name": "Header Text",
  "basedOn": "base-text",
  "typography": {
    "family": "Arial",
    "size": 14,
    "weight": "Bold",
    "style": "Normal",
    "lineHeight": 1.2,
    "letterSpacing": 0,
    "color": { "a": 255, "r": 17, "g": 24, "b": 39 },
    "alignment": "Left",
    "verticalAlignment": "Top",
    "decoration": "None"
  },
  "border": {
    "top": { "width": 1, "color": { "a": 255, "r": 17, "g": 24, "b": 39 }, "style": "Solid" },
    "right": { "width": 1, "color": { "a": 255, "r": 17, "g": 24, "b": 39 }, "style": "Solid" },
    "bottom": { "width": 1, "color": { "a": 255, "r": 17, "g": 24, "b": 39 }, "style": "Solid" },
    "left": { "width": 1, "color": { "a": 255, "r": 17, "g": 24, "b": 39 }, "style": "Solid" },
    "cornerRadius": 0
  },
  "background": { "a": 255, "r": 255, "g": 255, "b": 255 },
  "margin": { "left": 0, "top": 0, "right": 0, "bottom": 0 },
  "padding": { "left": 8, "top": 6, "right": 8, "bottom": 6 },
  "opacity": 1,
  "overflow": "Hidden"
}
```

Style property reference:

| Property | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Unique style key |
| `name` | string | No | Display label |
| `basedOn` | string | No | Parent style id |
| `typography` | object | No | Font and text settings |
| `border` | object | No | Four-side border definition |
| `background` | color | No | Fill color |
| `margin` | thickness | No | Outside spacing |
| `padding` | thickness | No | Inside spacing |
| `opacity` | float | No | 0..1 |
| `overflow` | enum string | No | `Hidden`, `Visible`, `Grow` |

Typography enum values:

- `weight`: `Thin`, `ExtraLight`, `Light`, `Normal`, `Medium`, `SemiBold`, `Bold`, `ExtraBold`, `Black`
- `style`: `Normal`, `Italic`, `Oblique`
- `alignment`: `Left`, `Center`, `Right`, `Justify`
- `verticalAlignment`: `Top`, `Middle`, `Bottom`
- `decoration`: `None`, `Underline`, `Strikethrough`, `Overline`

Border line style values:

- `None`, `Solid`, `Dashed`, `Dotted`, `DashDot`

Color format guidance:

- For canonical `ReportDefinition` JSON fields (for example style colors), use ARGB objects: `{ "a": 255, "r": 17, "g": 24, "b": 39 }`
- For `chartDataJson.options`, use either hex strings (for example `#1F2937`) or ARGB objects
- For `chartDataJson.points[].color`, use hex strings

Thickness object shape:

```json
{ "left": 8, "top": 6, "right": 8, "bottom": 6 }
```

## 8. Plugins

Optional per-report plugin toggles:

```json
{
  "pluginId": "my.plugin.id",
  "enabled": false
}
```

| Property | Type | Required | Notes |
|---|---|---|---|
| `pluginId` | string | Yes | Plugin id |
| `enabled` | bool | No | Default is plugin runtime default |

## 9. Layout model

`layout` has three sections:

- `pageHeader`
- `body`
- `pageFooter`

Each section is an array of `ReportLayoutItemDefinition`.

### 9.1 Item kinds

Supported `kind` values:

- `Text`
- `Image`
- `Barcode`
- `Chart`
- `Table`
- `PageBreak`

### 9.2 Item base properties

These can appear on most items:

| Property | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Stable item id |
| `kind` | enum string | Yes | Item kind |
| `blockStyleId` | string | No | Style id for containing region |

### 9.3 Text item properties

| Property | Type | Required | Notes |
|---|---|---|---|
| `text` | string | Recommended | Text content |
| `textStyleId` | string | No | Named style id |

### 9.4 Image item properties

| Property | Type | Required | Notes |
|---|---|---|---|
| `sourceKey` | string | Yes | URI/key |
| `stretch` | enum string | No | `None`, `Fill`, `Uniform`, `UniformToFill` |
| `imageStyleId` | string | No | Named style id |

### 9.5 Barcode item properties

| Property | Type | Required | Notes |
|---|---|---|---|
| `value` | string | Yes | Encoded value |
| `showText` | bool | No | Default `true` |
| `symbologyType` | enum string | Optional | Strongly typed route |
| `symbology` | string | Optional | Legacy string route |
| `barcodeStyleId` | string | No | Named style id |

Preferred `symbologyType` values:

- `QrCode`
- `MicroQr`
- `Code128`
- `Code39`
- `Ean13`
- `Ean8`
- `UpcA`
- `UpcE`
- `Itf`
- `Pdf417`
- `DataMatrix`
- `Codabar`

Canonical string `symbology` identifiers:

- `QR`, `MICROQR`, `CODE128`, `CODE39`, `EAN13`, `EAN8`, `UPCA`, `UPCE`, `ITF`, `PDF417`, `DATAMATRIX`, `CODABAR`

### 9.6 Chart item properties

| Property | Type | Required | Notes |
|---|---|---|---|
| `chartTypeValue` | enum string | Optional | Preferred typed route |
| `chartType` | string | Optional | Legacy string route |
| `chartDataJson` | string (JSON text) | Recommended | Chart payload |
| `chartStyleId` | string | No | Named style id |

Supported chart types:

- `chartTypeValue`: `BarVertical`, `BarHorizontal`, `Line`, `Pie`
- canonical `chartType`: `BAR_VERTICAL`, `BAR_HORIZONTAL`, `LINE`, `PIE`

`chartDataJson` schema:

```json
{
  "points": [
    { "label": "North", "value": 46, "color": "#2563EB" }
  ],
  "options": {
    "showAxes": true,
    "showGridLines": true,
    "showTicks": true,
    "showTickLabels": true,
    "xAxisLabel": "Region",
    "yAxisLabel": "Revenue (USD M)",
    "minValue": 0,
    "maxValue": 60,
    "yAxisTickCount": 6,
    "xAxisTickCount": 6,
    "axisColor": "#1F2937",
    "gridLineColor": "#D1D5DB",
    "labelColor": "#374151",
    "axisLineWidth": 1.2,
    "gridLineWidth": 0.8,
    "tickLength": 4,
    "labelFontSize": 9,
    "barGapRatio": 0.22,
    "lineColor": "#2563EB",
    "lineWidth": 2.4,
    "showMarkers": true,
    "showPieLabels": true,
    "pieLabelColor": "#1F2937",
    "pieLabelFontWeight": "SemiBold",
    "showLegend": true,
    "legendPosition": "Right",
    "legendMarkerSize": 10,
    "legendFontSize": 9,
    "legendTextColor": "#111827"
  }
}
```

Chart options reference:

| Property | Type | Default |
|---|---|---|
| `showAxes` | bool | `true` |
| `showGridLines` | bool | `true` |
| `showTicks` | bool | `true` |
| `showTickLabels` | bool | `true` |
| `xAxisLabel` | string | null |
| `yAxisLabel` | string | null |
| `minValue` | number | null |
| `maxValue` | number | null |
| `yAxisTickCount` | int | `5` |
| `xAxisTickCount` | int | `0` |
| `axisColor` | color | `#1F2937` |
| `gridLineColor` | color | `#D1D5DB` |
| `labelColor` | color | `#374151` |
| `axisLineWidth` | number | `1` |
| `gridLineWidth` | number | `1` |
| `tickLength` | number | `4` |
| `labelFontSize` | number | `9` |
| `barGapRatio` | number | `0.2` |
| `lineColor` | color | null |
| `lineWidth` | number | `2` |
| `showMarkers` | bool | `true` |
| `showPieLabels` | bool | `true` |
| `pieLabelColor` | color | null |
| `pieLabelFontWeight` | enum string/number | null |
| `showLegend` | bool | `false` |
| `legendPosition` | enum string | `Right` |
| `legendMarkerSize` | number | `10` |
| `legendFontSize` | number | `9` |
| `legendTextColor` | color | null |

Legend behavior:

- Works on pie, vertical bar, horizontal bar, and line charts
- Left/right legends use compact multi-column flow
- Top/bottom legends wrap rows
- Plot area reserves legend space automatically

### 9.7 Table item properties

| Property | Type | Required | Notes |
|---|---|---|---|
| `dataSourceId` | string | Yes | Source used for row generation |
| `columns` | array | Yes | Table columns |
| `includeHeader` | bool | No | Default `true` |
| `repeatHeaders` | bool | No | Default `true` |
| `groupByExpression` | string | No | Group boundary expression |
| `footerAggregates` | array | No | Aggregate definitions |
| `footerLabel` | string | No | Default `Total` |
| `footerLabelColumnIndex` | int | No | Default `0` |
| `regionStyleId` | string | No | Table region style |
| `tableStyleId` | string | No | Table style |
| `headerRowStyleId` | string | No | Header row style |
| `headerCellStyleId` | string | No | Header cell style |
| `dataRowStyleId` | string | No | Data row style |
| `dataCellStyleId` | string | No | Data cell style |
| `groupHeaderRowStyleId` | string | No | Group header row style |
| `groupHeaderCellStyleId` | string | No | Group header cell style |
| `footerRowStyleId` | string | No | Footer row style |
| `footerCellStyleId` | string | No | Footer cell style |

`columns[]` shape:

```json
{
  "header": "Amount",
  "valueExpression": "{Amount}",
  "minWidth": 80,
  "grow": 0
}
```

`footerAggregates[]` shape:

```json
{
  "columnIndex": 1,
  "valueExpression": "{Amount}",
  "kind": "Sum",
  "formatString": "0.00"
}
```

Aggregate `kind` values:

- `Sum`
- `Average`
- `Min`
- `Max`
- `Count`
- `CountNonNull`

### 9.8 Page break item

| Property | Type | Required | Notes |
|---|---|---|---|
| `id` | string | Yes | Marker id |
| `kind` | `PageBreak` | Yes | Forces page break before next block |
| `blockStyleId` | string | No | Optional style |

## 10. Full end-to-end example

This file includes text, image, barcode, chart, table, and page break:

```json
{
  "schemaVersion": "1.0",
  "id": "sales-q3-2026",
  "name": "Sales Q3 2026",
  "author": "Finance Team",
  "metadata": {
    "domain": "sales"
  },
  "parameters": [
    {
      "id": "region",
      "name": "Region",
      "type": "String",
      "isRequired": false,
      "defaultValue": "All"
    }
  ],
  "dataSources": [
    {
      "id": "orders",
      "name": "Orders",
      "providerType": "Poco",
      "sourceName": "DefaultInMemory",
      "properties": {
        "dataset": "orders"
      }
    }
  ],
  "styles": [
    {
      "id": "base",
      "typography": {
        "family": "Arial",
        "size": 11,
        "color": { "a": 255, "r": 17, "g": 24, "b": 39 }
      }
    },
    {
      "id": "title",
      "basedOn": "base",
      "typography": {
        "size": 16,
        "weight": "Bold"
      },
      "padding": { "left": 0, "top": 0, "right": 0, "bottom": 8 }
    }
  ],
  "layout": {
    "pageHeader": [
      {
        "id": "title",
        "kind": "Text",
        "text": "Quarterly Sales",
        "textStyleId": "title"
      }
    ],
    "body": [
      {
        "id": "logo",
        "kind": "Image",
        "sourceKey": "https://example.com/logo.png",
        "stretch": "Uniform"
      },
      {
        "id": "sales-chart",
        "kind": "Chart",
        "chartTypeValue": "Line",
        "chartDataJson": "{\"points\":[{\"label\":\"Jan\",\"value\":18},{\"label\":\"Feb\",\"value\":22},{\"label\":\"Mar\",\"value\":27}],\"options\":{\"showLegend\":true,\"legendPosition\":\"Right\",\"lineWidth\":2.4}}"
      },
      {
        "id": "invoice-qr",
        "kind": "Barcode",
        "symbologyType": "QrCode",
        "value": "https://example.com/invoice/123",
        "showText": false
      },
      {
        "id": "orders-table",
        "kind": "Table",
        "dataSourceId": "orders",
        "columns": [
          { "header": "Customer", "valueExpression": "{CustomerName}", "minWidth": 120, "grow": 1 },
          { "header": "Amount", "valueExpression": "{Amount}", "minWidth": 90, "grow": 0 }
        ],
        "includeHeader": true,
        "repeatHeaders": true,
        "footerAggregates": [
          { "columnIndex": 1, "valueExpression": "{Amount}", "kind": "Sum", "formatString": "0.00" }
        ],
        "footerLabel": "Grand Total",
        "footerLabelColumnIndex": 0
      },
      {
        "id": "break-1",
        "kind": "PageBreak"
      }
    ],
    "pageFooter": [
      {
        "id": "footer-note",
        "kind": "Text",
        "text": "Generated by KineticReports"
      }
    ]
  }
}
```

## 11. JSON authoring checklist

- Every style id referenced by `*StyleId` exists in `styles`
- Every table has at least one column
- `dataSourceId` values match declared data source ids
- Use canonical chart and barcode identifiers
- Keep `chartDataJson` valid JSON text
- Keep secrets out of `dataSources[].properties`

## 12. Troubleshooting

- Unknown style id: verify all referenced `*StyleId` values
- Empty chart: verify `chartDataJson` is valid and has non-empty `points`
- Missing table rows: verify `dataSourceId` and provider output
- Deserialize failure: check duplicate JSON keys and enum string spelling
