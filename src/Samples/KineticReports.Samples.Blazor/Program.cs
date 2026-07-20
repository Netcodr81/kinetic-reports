using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using KineticReports.Samples.Blazor.Services;

// ============================================================================
// KineticReports Blazor Sample Application (Interactive Server)
// ============================================================================
// This sample demonstrates:
// 1. Loading and managing plugins via IPluginService
// 2. Displaying sample reports with different components
// 3. Interactive Blazor components (Server-side rendering)
//
// To run: dotnet run
// To build: dotnet build
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register custom services
builder.Services.AddScoped<IPluginService, PluginService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<KineticReports.Samples.Blazor.App>()
    .AddInteractiveServerRenderMode();

app.Run();

// ============================================================================
// Application Structure
// ============================================================================
// 
// Components:
// - App.razor              - Root component
// - Routes.razor           - Router configuration
// - Layout.razor           - Main layout with header/footer
// - Pages/Index.razor      - Home page with plugin and report samples
//
// Services:
// - IPluginService         - Manages plugin lifecycle and discovery
// - PluginService          - Default implementation with assembly loading
//
// Reports:
// - SampleReports.cs       - Factory for creating sample report definitions
//   - CreateSimpleTextReport()      - Basic text layout
//   - CreateTableReport()            - Tables and data grids
//   - CreateBandedReport()           - Repeating sections
//   - CreateMultiSectionReport()     - Multiple sections
//   - CreateStyledReport()           - Styling demonstration
//
// ============================================================================

