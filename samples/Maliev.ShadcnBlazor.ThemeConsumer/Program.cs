using Maliev.ShadcnBlazor;
using Maliev.ShadcnBlazor.ThemeConsumer;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

using var bootstrapClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
await using var themeStream = await bootstrapClient.GetStreamAsync("theme.json");
var themeDocument = await ShadcnThemeDocumentLoader.LoadAsync(themeStream);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton(themeDocument);
builder.Services.AddMalievShadcn(options => options.Theme = themeDocument.Theme);

await builder.Build().RunAsync();
