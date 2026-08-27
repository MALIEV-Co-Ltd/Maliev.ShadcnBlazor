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

        Assert.Equal(69, slugs.Count);
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
        var slugs = ComponentCatalogProof.SelectRequested(ComponentCatalogProof.LoadCompleted(root));
        var errors = new List<string>();

        await CaptureModeAsync(slugs, VisualProofMode.DesktopLight, errors);
        await CaptureModeAsync(slugs, VisualProofMode.MobileDarkRtl, errors);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task EveryCompletedCatalogDossierRendersItsPrimaryRclComponent()
    {
        var root = VisualProof.FindRoot();
        var slugs = ComponentCatalogProof.SelectRequested(ComponentCatalogProof.LoadCompleted(root));
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        foreach (var slug in slugs)
        {
            await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{slug}").ToString());
            await page.GetByTestId("component-dossier").WaitForAsync();
            await Assertions.Expect(page.GetByTestId("planned-component-notice")).ToHaveCountAsync(0);
            var expectedSlot = PrimaryPreviewSlot(slug);
            Assert.True(
                await page.GetByTestId("component-preview-canvas").Locator($"[data-slot='{expectedSlot}']").CountAsync() > 0,
                $"The {slug} documentation preview did not render a '{expectedSlot}' RCL component slot.");
        }
    }

    [Fact]
    public async Task ThemeStudioWorkbenchMatchesReviewedVisualProof()
    {
        await CaptureThemeStudioAsync(
            "desktop-light",
            new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 1440, Height = 900 },
                DeviceScaleFactor = 1,
                ReducedMotion = ReducedMotion.Reduce,
                ColorScheme = ColorScheme.Light
            });
        await CaptureThemeStudioAsync(
            "tablet-dark-rtl",
            new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 768, Height = 1024 },
                DeviceScaleFactor = 1,
                ReducedMotion = ReducedMotion.Reduce,
                ColorScheme = ColorScheme.Dark
            }, darkRtl: true, openSettings: true);
        await CaptureThemeStudioAsync(
            "mobile-light",
            new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 390, Height = 844 },
                DeviceScaleFactor = 1,
                ReducedMotion = ReducedMotion.Reduce,
                ColorScheme = ColorScheme.Light
            });
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
            var canvas = page.GetByTestId("component-preview-canvas").First;
            await Assertions.Expect(canvas).ToBeVisibleAsync();

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
            await canvas.ScrollIntoViewIfNeededAsync();
            var actual = await canvas.ScreenshotAsync(new() { Animations = ScreenshotAnimations.Disabled });
            await VisualProof.CompareOrUpdateAsync(page, slug, mode.Name, actual);
        }
    }

    private async Task CaptureThemeStudioAsync(
        string mode,
        BrowserNewContextOptions options,
        bool darkRtl = false,
        bool openSettings = false)
    {
        await using var context = await playwright.Browser.NewContextAsync(options);
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        await Assertions.Expect(page.GetByTestId("theme-bento")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-testid='theme-bento'] .theme-use-case-card")).ToHaveCountAsync(45);
        var settingsToggle = page.GetByTestId("theme-controls-toggle");
        var settingsWereOpen = string.Equals(
            await settingsToggle.GetAttributeAsync("aria-expanded"),
            "true",
            StringComparison.Ordinal);
        if (!settingsWereOpen)
            await settingsToggle.ClickAsync();

        var accessibilitySection = page.GetByTestId("theme-advanced-accessibility");
        var accessibilityTrigger = accessibilitySection.Locator("button").First;
        await accessibilityTrigger.ClickAsync();
        var reducedMotion = accessibilitySection.GetByTestId("preview-reduced-motion");
        await Assertions.Expect(reducedMotion).ToBeVisibleAsync();
        if (string.Equals(await reducedMotion.GetAttributeAsync("aria-checked"), "false", StringComparison.Ordinal))
            await reducedMotion.ClickAsync();
        await Assertions.Expect(reducedMotion).ToHaveAttributeAsync("aria-checked", "true");
        await accessibilityTrigger.ClickAsync();
        await page.GetByTestId("theme-studio-sidebar").Locator(".shadcn-sidebar-content").EvaluateAsync("element => element.scrollTop = 0");
        if (!settingsWereOpen)
        {
            await page.GetByTestId("theme-sidebar-collapse").ClickAsync();
            await Assertions.Expect(settingsToggle).ToHaveAttributeAsync("aria-expanded", "false");
        }
        if (mode.StartsWith("mobile", StringComparison.Ordinal))
        {
            var firstCard = page.Locator("[data-testid='theme-bento'] .theme-use-case-card").First;
            await Assertions.Expect(firstCard).ToBeVisibleAsync();
            await Assertions.Expect(firstCard).ToBeInViewportAsync();
        }
        if (darkRtl)
        {
            await page.GetByTestId("documentation-theme-toggle").ClickAsync();
            await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        }
        if (openSettings)
        {
            if (string.Equals(await settingsToggle.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal))
                await settingsToggle.ClickAsync();
        }
        var catalogStatus = page.GetByTestId("theme-font-catalog-status");
        if (await catalogStatus.CountAsync() > 0)
            await Assertions.Expect(catalogStatus).Not.ToContainTextAsync("Loading local font catalog");
        await page.EvaluateAsync("document.fonts.ready");
        await page.EvaluateAsync("() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
        var actual = await page.ScreenshotAsync(new()
        {
            Animations = ScreenshotAnimations.Disabled,
            FullPage = false
        });
        await VisualProof.CompareOrUpdateAsync(page, "theme-studio", mode, actual);
    }

    private static string PrimaryPreviewSlot(string slug) => slug switch
    {
        "resizable" => "resizable-group",
        "toast" => "toast-viewport",
        _ => slug
    };
}
