using Maliev.ShadcnBlazor;
using Maliev.ShadcnBlazor.Showcase;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Maliev.ShadcnBlazor.Showcase.MockSites;
using Maliev.ShadcnBlazor.Showcase.Theming;
using Maliev.ShadcnBlazor.Showcase.Theming.Fonts;
using Maliev.ShadcnBlazor.Showcase.ThemeScenarios;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMalievShadcn();
builder.Services.AddScoped<ShowcaseState>();
builder.Services.AddScoped<DocumentationPageState>();
builder.Services.AddSingleton<IComponentDocumentationCatalog, ComponentDocumentationCatalog>();
builder.Services.AddSingleton<IThemeScenarioRegistry>(services =>
{
    var catalog = services.GetRequiredService<IComponentDocumentationCatalog>();
    var scenarios = ThemeScenarioCatalog.Load(catalog);
    return ThemeScenarioRegistry.Create(scenarios, ThemeScenarioFactoryCatalog.Create(catalog, scenarios));
});
builder.Services.AddSingleton<ComponentApiCatalog>();
builder.Services.AddTransient<IComponentExampleRegistry, ComponentExampleRegistry>();
builder.Services.AddScoped<ThemeStudioStorage>();
builder.Services.AddScoped<IThemeStudioStorage>(services => services.GetRequiredService<ThemeStudioStorage>());
builder.Services.AddScoped<ThemeStudioWorkbenchState>();
builder.Services.AddScoped<ThemeStudioState>();
builder.Services.AddScoped<GoogleFontCatalogService>();
builder.Services.AddScoped<MockSiteState>();
await builder.Build().RunAsync();
