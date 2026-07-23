using KineticReports.Samples.Blazor.Components;
using KineticReports.Samples.Blazor.DependencyInjection;

// ============================================================================
// KineticReports Blazor Sample Application (Interactive Server)
// ============================================================================
// This sample demonstrates:
// 1. Loading and managing plugins via IPluginService
// 2. Displaying sample reports with different components
// 3. Interactive Blazor components (Server-side rendering)
// 4. Report rendering with preview capability
//
// To run: dotnet run
// To build: dotnet build
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddKeneticReports()
    .AddBlazorViewer()
    .UseSampleDataResolver()
    .UseSampleAuthoringServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
