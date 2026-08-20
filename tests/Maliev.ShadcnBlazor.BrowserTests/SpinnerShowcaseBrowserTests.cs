using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class SpinnerShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task SpinnerDossierSupportsDirectInteractionResponsiveLayoutAndMotionPreferences()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.NoPreference
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/spinner").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var export = canvas.GetByTestId("spinner-export");
        var spinner = export.Locator("[data-slot='spinner']");
        await Assertions.Expect(export).ToHaveAttributeAsync("aria-busy", "true");
        await Assertions.Expect(spinner).ToHaveAttributeAsync("role", "status");
        await Assertions.Expect(spinner).ToHaveAttributeAsync("aria-label", "Generating production report");
        Assert.Equal("shadcn-spinner-rotate", await spinner.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        await export.GetByRole(AriaRole.Button, new() { Name = "Cancel export" }).ClickAsync();
        await Assertions.Expect(export).ToHaveAttributeAsync("aria-busy", "false");
        await Assertions.Expect(export).ToContainTextAsync("Export paused");
        await Assertions.Expect(spinner).ToHaveCountAsync(0);
        await export.GetByRole(AriaRole.Button, new() { Name = "Resume export" }).ClickAsync();
        await Assertions.Expect(export).ToHaveAttributeAsync("aria-busy", "true");

        await page.Locator("#spinner-decorative").CheckAsync();
        await Assertions.Expect(export.Locator("[data-slot='spinner']")).ToHaveAttributeAsync("aria-hidden", "true");
        await Assertions.Expect(export.Locator("[data-slot='spinner']")).Not.ToHaveAttributeAsync("role", "status");
        await page.Locator("#spinner-large").CheckAsync();
        Assert.Equal("24px", await export.Locator("[data-slot='spinner']").EvaluateAsync<string>("element => getComputedStyle(element).width"));
        await page.Locator("#spinner-reduced-motion").CheckAsync();
        await Assertions.Expect(export.Locator("[data-slot='spinner']")).ToHaveAttributeAsync("data-reduced-motion", "true");
        Assert.Equal("none", await export.Locator("[data-slot='spinner']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        await page.SetViewportSizeAsync(390, 844);
        Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));
        var canvasBox = await canvas.BoundingBoxAsync();
        var exportBox = await export.BoundingBoxAsync();
        Assert.NotNull(canvasBox);
        Assert.NotNull(exportBox);
        Assert.True(exportBox.X >= canvasBox.X && exportBox.X + exportBox.Width <= canvasBox.X + canvasBox.Width);
        var buttonBox = await export.GetByRole(AriaRole.Button).BoundingBoxAsync();
        Assert.NotNull(buttonBox);
        Assert.True(Math.Abs(buttonBox.Width - (exportBox.Width - 32)) <= 2);
    }

    [Fact]
    public async Task SpinnerRemainsVisibleInForcedColorsAndStopsForSystemReducedMotion()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 900, Height = 720 },
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/spinner").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var spinner = page.GetByTestId("component-preview-canvas").Locator("[data-slot='spinner']");
        await Assertions.Expect(spinner).ToBeVisibleAsync();
        Assert.Equal("none", await spinner.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        Assert.NotEqual("rgba(0, 0, 0, 0)", await spinner.EvaluateAsync<string>("element => getComputedStyle(element).color"));
    }
}
