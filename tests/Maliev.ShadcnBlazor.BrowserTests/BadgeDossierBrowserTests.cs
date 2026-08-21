using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class BadgeDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData(1280, 900, "light", "ltr")]
    [InlineData(390, 844, "dark", "rtl")]
    public async Task BadgeDossierIsResponsiveAccessibleAndStateAware(
        int width,
        int height,
        string theme,
        string direction)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/badge?theme={theme}&dir={direction}").ToString());
        await page.GetByTestId("badge-dossier-preview").WaitForAsync();

        var preview = page.GetByTestId("badge-dossier-preview");
        await Assertions.Expect(preview.GetByTestId("badge-variant-gallery").Locator("[data-slot='badge']")).ToHaveCountAsync(6);
        await Assertions.Expect(page.GetByTestId("component-preview-canvas")).ToBeVisibleAsync();
        Assert.InRange(
            await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"),
            0,
            1);

        await page.ChooseOptionAsync("control-badge-variant", "Outline");
        await page.GetByTestId("control-badge-link").CheckAsync();
        await page.GetByTestId("control-badge-invalid").CheckAsync();
        var selected = preview.GetByTestId("badge-current").Locator("[data-slot='badge']");
        await Assertions.Expect(selected).ToHaveAttributeAsync("data-variant", "outline");
        await Assertions.Expect(selected).ToHaveAttributeAsync("href", "docs/components/badge");
        await Assertions.Expect(selected).ToHaveAttributeAsync("aria-invalid", "true");
        await selected.FocusAsync();
        Assert.NotEqual("none", await selected.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));

        var source = page.Locator("#preview [data-slot='code-block'] pre");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnBadgeVariant.Outline");
        await Assertions.Expect(source).ToContainTextAsync("Href=\"docs/components/badge\"");
        await Assertions.Expect(source).ToContainTextAsync("aria-invalid=\"true\"");

        var axe = await preview.RunAxe();
        Assert.Empty(axe.Violations);
    }

    [Fact]
    public async Task BadgePreservesVisibleBoundariesInForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 900, Height = 720 },
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/badge").ToString());
        await page.GetByTestId("badge-dossier-preview").WaitForAsync();

        foreach (var badge in await page.GetByTestId("badge-dossier-preview").Locator("[data-slot='badge']").AllAsync())
            Assert.Equal("1px", await badge.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
    }
}
