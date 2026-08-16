using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ComponentCatalogVisualProofTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public void EveryCompletedCatalogEntryHasTwoReviewedBaselines()
    {
        var root = VisualProof.FindRoot();
        var slugs = ComponentCatalogProof.LoadCompleted(root);
        var baselineDirectory = VisualProof.BaselineDirectory(root);

        Assert.Equal(64, slugs.Count);
        if (VisualProof.UpdateEnabled)
            return;

        foreach (var slug in slugs)
        {
            Assert.True(File.Exists(Path.Combine(baselineDirectory, $"{slug}--desktop-light.png")), $"Missing desktop proof for {slug}.");
            Assert.True(File.Exists(Path.Combine(baselineDirectory, $"{slug}--mobile-dark-rtl.png")), $"Missing mobile proof for {slug}.");
        }
    }

    [Fact]
    public async Task EveryCompletedCatalogDossierMatchesReviewedVisualProof()
    {
        var root = VisualProof.FindRoot();
        var slugs = ComponentCatalogProof.LoadCompleted(root);
        var errors = new List<string>();

        await CaptureModeAsync(slugs, VisualProofMode.DesktopLight, errors);
        await CaptureModeAsync(slugs, VisualProofMode.MobileDarkRtl, errors);

        Assert.Empty(errors);
    }

    private async Task CaptureModeAsync(
        IReadOnlyList<string> slugs,
        VisualProofMode mode,
        List<string> errors)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = mode.Viewport,
            DeviceScaleFactor = 1,
            Locale = "th-TH",
            TimezoneId = "Asia/Bangkok",
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = mode.Dark ? ColorScheme.Dark : ColorScheme.Light,
        });
        var page = await context.NewPageAsync();
        page.PageError += (_, error) => errors.Add($"{mode.Name}: {error}");
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
                errors.Add($"{mode.Name}: {message.Text}");
        };

        foreach (var slug in slugs)
        {
            await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{slug}").ToString());
            await page.GetByTestId("component-dossier").WaitForAsync();
            await Assertions.Expect(page.GetByTestId("planned-component-notice")).ToHaveCountAsync(0);
            await Assertions.Expect(page.GetByTestId("component-preview-canvas")).ToHaveCountAsync(1);

            if (mode.Dark)
            {
                await page.GetByTestId("documentation-theme-toggle").EvaluateAsync("element => element.click()");
                await page.GetByTestId("documentation-direction-toggle").EvaluateAsync("element => element.click()");
                await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("data-shadcn-theme", "dark");
                await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("dir", "rtl");
            }

            await page.EvaluateAsync("document.fonts.ready");
            await page.EvaluateAsync("""
                async () => {
                    const images = Array.from(document.images);
                    await Promise.all(images.map(image => image.complete
                        ? Promise.resolve()
                        : new Promise(resolve => {
                            image.addEventListener('load', resolve, { once: true });
                            image.addEventListener('error', resolve, { once: true });
                        })));
                }
                """);
            var canvas = page.GetByTestId("component-preview-canvas");
            await canvas.ScrollIntoViewIfNeededAsync();
            var actual = await canvas.ScreenshotAsync(new() { Animations = ScreenshotAnimations.Disabled });
            await VisualProof.CompareOrUpdateAsync(page, slug, mode.Name, actual);
        }
    }
}
