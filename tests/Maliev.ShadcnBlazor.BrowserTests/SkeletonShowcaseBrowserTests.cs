using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class SkeletonShowcaseBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task SkeletonDossierLoadsRealContentAndKeepsPreviewSourceAndMotionInSync()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            Locale = "th-TH"
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/skeleton").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var preview = page.GetByTestId("skeleton-dossier-preview");
        await Assertions.Expect(preview).ToHaveAttributeAsync("aria-busy", "true");
        Assert.Equal(12, await preview.Locator("[data-testid='skeleton-loading-list'] [data-slot='skeleton']").CountAsync());
        Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));

        await page.GetByTestId("control-skeleton-circle").CheckAsync();
        await page.GetByTestId("control-skeleton-motion").UncheckAsync();
        await Assertions.Expect(preview.Locator("[data-testid='skeleton-media']").First).ToHaveAttributeAsync("data-shape", "circle");
        await Assertions.Expect(preview.Locator("[data-slot='skeleton']").First).ToHaveAttributeAsync("data-animation", "none");
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("private bool RoundMedia = true;");
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("ShadcnSkeletonAnimation.None");

        await page.GetByTestId("skeleton-state-toggle").ClickAsync();
        await Assertions.Expect(preview).ToHaveAttributeAsync("aria-busy", "false");
        await Assertions.Expect(preview.GetByTestId("skeleton-loading-list")).ToHaveCountAsync(0);
        await Assertions.Expect(preview.GetByTestId("skeleton-loaded-list").Locator(":scope > li")).ToHaveCountAsync(3);
        await Assertions.Expect(preview).ToContainTextAsync("WO-2486");
        await Assertions.Expect(page.GetByTestId("skeleton-state-toggle")).ToHaveTextAsync("Reset loading preview");

        await page.GetByTestId("skeleton-state-toggle").ClickAsync();
        await Assertions.Expect(preview).ToHaveAttributeAsync("aria-busy", "true");
        Assert.True(await preview.Locator("[data-slot='skeleton']").First.EvaluateAsync<bool>(
            "element => parseFloat(getComputedStyle(element).animationDuration) <= .001"));
    }
}
