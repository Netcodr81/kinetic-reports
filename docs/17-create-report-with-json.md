# 17 - Create a Report with JSON (Step-by-Step)

This guide is focused only on creating reports from JSON, including all available
runtime `ReportDefinition` properties, metadata, and plugin usage.

## Prerequisites

1. Target framework: `net10.0`
2. Reference `KineticReports.Core`
3. Add these namespaces:

```csharp
using KineticReports.Core.Authoring.Serialization;
using KineticReports.Core.Definition;
```

## Step 1: Create a complete runtime JSON report definition

Create a JSON file named `sales-summary.runtime.json` with every top-level
property supported by `ReportDefinition`.

```json
{
  "schemaVersion": "1.0",
  "id": "sales-summary",
  "name": "Sales Summary",
  "author": "Kinetic Team",
  "description": "Quarterly sales summary report",
  "metadata": {
    "report.category": "finance",
    "report.owner": "ops",
    "plugins.kinetic.watermark.enabled": true,
    "plugins.kinetic.watermark.message": "Internal Use Only",
    "plugins.kinetic.watermark.opacity": 0.12,
    "plugins.kinetic.watermark.rotationDegrees": -30,
    "plugins.kinetic.watermark.fontSizePx": 72,
    "plugins.kinetic.watermark.textColorHex": "#000000"
  },
  "parameters": [
    {
      "id": "region",
      "name": "Region",
      "type": "String",
      "isRequired": false,
      "defaultValue": "ALL",
      "description": "Region filter"
    },
    {
      "id": "asOfDate",
      "name": "As Of Date",
      "type": "DateTime",
      "isRequired": true,
      "defaultValue": "2026-01-01T00:00:00Z",
      "description": "Report date"
    }
  ],
  "dataSources": [
    {
      "id": "sales-orders",
      "name": "Sales Orders",
      "providerType": "SampleInMemory",
      "properties": {
        "seed": "sales-2026"
      }
    }
  ],
  "styles": [
    {
      "id": "text-body",
      "name": "Body Text",
      "basedOn": null,
      "typography": {
        "family": "Segoe UI",
        "size": 11,
        "weight": "Regular",
        "style": "Normal",
        "lineHeight": 1.3,
        "letterSpacing": 0,
        "color": {
          "a": 255,
          "r": 34,
          "g": 34,
          "b": 34
        },
        "alignment": "Left",
        "verticalAlignment": "Middle",
        "decoration": "None"
      },
      "border": {
        "top": {
          "width": 0,
          "color": {
            "a": 255,
            "r": 0,
            "g": 0,
            "b": 0
          },
          "style": "Solid"
        },
        "right": {
          "width": 0,
          "color": {
            "a": 255,
            "r": 0,
            "g": 0,
            "b": 0
          },
          "style": "Solid"
        },
        "bottom": {
          "width": 0,
          "color": {
            "a": 255,
            "r": 0,
            "g": 0,
            "b": 0
          },
          "style": "Solid"
        },
        "left": {
          "width": 0,
          "color": {
            "a": 255,
            "r": 0,
            "g": 0,
            "b": 0
          },
          "style": "Solid"
        },
        "cornerRadius": 0
      },
      "background": {
        "a": 0,
        "r": 255,
        "g": 255,
        "b": 255
      },
      "margin": {
        "left": 0,
        "top": 0,
        "right": 0,
        "bottom": 0
      },
      "padding": {
        "left": 0,
        "top": 0,
        "right": 0,
        "bottom": 0
      },
      "opacity": 1,
      "overflow": "Visible"
    },
    {
      "id": "table-header-cell",
      "name": "Table Header Cell",
      "basedOn": "text-body",
      "typography": {
        "weight": "SemiBold"
      }
    }
  ],
  "plugins": [
    {
      "pluginId": "kinetic.watermark",
      "enabled": true
    }
  ],
  "layout": {
    "pageHeader": [
      {
        "id": "header-title",
        "kind": "Text",
        "text": "Sales Summary",
        "blockStyleId": "text-body",
        "textStyleId": "text-body"
      }
    ],
    "body": [
      {
        "id": "hero-image",
        "kind": "Image",
        "sourceKey": "images/company-logo.png",
        "stretch": "Uniform",
        "imageStyleId": "text-body"
      },
      {
        "id": "orders-table",
        "kind": "Table",
        "dataSourceId": "sales-orders",
        "regionStyleId": "text-body",
        "tableStyleId": "text-body",
        "headerRowStyleId": "text-body",
        "headerCellStyleId": "table-header-cell",
        "dataRowStyleId": "text-body",
        "dataCellStyleId": "text-body",
        "groupHeaderRowStyleId": "text-body",
        "groupHeaderCellStyleId": "table-header-cell",
        "footerRowStyleId": "text-body",
        "footerCellStyleId": "text-body",
        "columns": [
          {
            "header": "Customer",
            "valueExpression": "{CustomerName}",
            "minWidth": 120,
            "grow": 2
          },
          {
            "header": "Amount",
            "valueExpression": "{Amount}",
            "minWidth": 90,
            "grow": 1
          }
        ],
        "footerAggregates": [
          {
            "columnIndex": 1,
            "valueExpression": "Sum({Amount})",
            "kind": "Sum",
            "formatString": "C2"
          }
        ],
        "groupByExpression": "{Region}",
        "includeHeader": true,
        "repeatHeaders": true,
        "footerLabel": "Total",
        "footerLabelColumnIndex": 0
      },
      {
        "id": "manual-page-break",
        "kind": "PageBreak",
        "blockStyleId": "text-body"
      }
    ],
    "pageFooter": [
      {
        "id": "footer-text",
        "kind": "Text",
        "text": "Confidential",
        "textStyleId": "text-body"
      }
    ]
  }
}
```

## Step 2: Understand plugin usage in JSON

Use both mechanisms together:

1. `plugins[]` controls whether a plugin runs for this report.
2. `metadata` provides plugin-specific settings.

Watermark example behavior:

1. If `plugins` contains `kinetic.watermark` with `enabled: false`, watermark is skipped.
2. If plugin is enabled, metadata keys under `plugins.kinetic.watermark.*` are used.
3. If keys are missing, runtime defaults are applied.

## Step 3: Deserialize JSON into `ReportDefinition`

```csharp
var json = File.ReadAllText("sales-summary.runtime.json");

var serializer = new SystemTextJsonReportDefinitionSerializer();
ReportDefinition definition = serializer.Deserialize(json);
```

## Step 4: Validate plugin toggles and metadata values in code (optional)

```csharp
var watermarkToggle = definition.Plugins
    .FirstOrDefault(p => string.Equals(p.PluginId, "kinetic.watermark", StringComparison.OrdinalIgnoreCase));

var watermarkEnabled = watermarkToggle?.Enabled ?? true;
var watermarkMessage = definition.Metadata.TryGetValue("plugins.kinetic.watermark.message", out var value)
    ? value?.ToString()
    : "KineticReports";
```

## Step 5: Re-serialize after edits (optional)

```csharp
var serializer = new SystemTextJsonReportDefinitionSerializer();
var updatedJson = serializer.Serialize(definition);
File.WriteAllText("sales-summary.runtime.updated.json", updatedJson);
```

## What you now have

1. A complete JSON `ReportDefinition` with all supported top-level properties
2. Metadata and plugin toggles configured in the same report JSON
3. A repeatable JSON-only path to load, edit, and persist reports
