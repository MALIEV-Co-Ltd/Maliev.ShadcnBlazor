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

        Assert.Equal(65, slugs.Count);
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
            "mobile-forced-colors",
            new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 390, Height = 844 },
                DeviceScaleFactor = 1,
                ReducedMotion = ReducedMotion.Reduce,
                ForcedColors = ForcedColors.Active
            }, openSettings: true);
    }

    [Fact]
    public async Task ThemeScenarioMatrixMatchesReviewedCategoryAndHighRiskProofs()
    {
        var categoryRepresentatives = new[]
        {
            "typography-default", "button-default", "input-default", "alert-default",
            "accordion-default", "dialog-default", "chart-default", "message-default"
        };
        var highRiskScenarios = new[]
        {
            "input-stress", "input-accessible", "dialog-stress", "dialog-accessible",
            "chart-stress", "chart-accessible", "message-stress", "message-accessible"
        };

        await CaptureThemeScenariosAsync(
            categoryRepresentatives,
            "desktop-light",
            new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 1440, Height = 900 },
                DeviceScaleFactor = 1,
                ReducedMotion = ReducedMotion.Reduce,
                ColorScheme = ColorScheme.Light
            });
        await CaptureThemeScenariosAsync(
            highRiskScenarios,
            "mobile-dark-rtl",
            new BrowserNewContextOptions
            {
                ViewportSize = new() { Width = 390, Height = 844 },
                DeviceScaleFactor = 1,
                ReducedMotion = ReducedMotion.Reduce,
                ColorScheme = ColorScheme.Dark
            }, darkRtlThai: true);
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
        if (darkRtl)
        {
            await page.GetByTestId("mode-dark").ClickAsync();
            await page.GetByTestId("direction-rtl").ClickAsync();
        }
        if (openSettings)
        {
            var toggle = page.GetByTestId("theme-controls-toggle");
            if (string.Equals(await toggle.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal))
                await toggle.ClickAsync();
        }
        await page.EvaluateAsync("document.fonts.ready");
        var actual = await page.ScreenshotAsync(new()
        {
            Animations = ScreenshotAnimations.Disabled,
            FullPage = false
        });
        await VisualProof.CompareOrUpdateAsync(page, "theme-studio", mode, actual);
    }

    private async Task CaptureThemeScenariosAsync(
        IReadOnlyList<string> scenarioIds,
        string mode,
        BrowserNewContextOptions options,
        bool darkRtlThai = false)
    {
        await using var context = await playwright.Browser.NewContextAsync(options);
        var page = await context.NewPageAsync();

        foreach (var scenarioId in scenarioIds)
        {
            var separator = scenarioId.LastIndexOf('-');
            var slug = scenarioId[..separator];
            await page.GotoAsync(new Uri(server.BaseUri, $"/theme?component={slug}&scenario={scenarioId}").ToString());
            await page.GetByTestId("theme-scenario-browser").WaitForAsync();
            if (darkRtlThai)
            {
                await page.GetByTestId("mode-dark").ClickAsync();
                await page.GetByTestId("direction-rtl").ClickAsync();
                await page.GetByTestId("locale-thai").ClickAsync();
            }

            var host = page.GetByTestId("theme-scenario-host");
            await Assertions.Expect(host).ToHaveAttributeAsync("data-theme-scenario-host", scenarioId);
            await page.EvaluateAsync("document.fonts.ready");
            await host.ScrollIntoViewIfNeededAsync();
            byte[] actual;
            if (string.Equals(scenarioId, "dialog-stress", StringComparison.Ordinal))
            {
                await host.Locator("[data-slot='dialog-trigger']").ClickAsync();
                await Assertions.Expect(page.Locator("[data-slot='dialog-content']")).ToBeVisibleAsync();
                actual = await page.ScreenshotAsync(new() { Animations = ScreenshotAnimations.Disabled, FullPage = false });
                await page.Keyboard.PressAsync("Escape");
            }
            else
            {
                if (scenarioId.EndsWith("-accessible", StringComparison.Ordinal))
                {
                    var interactive = host.Locator("button:not([disabled]), input:not([disabled]), [href], [tabindex='0']").First;
                    if (await interactive.CountAsync() > 0)
                        await interactive.FocusAsync();
                }
                actual = await host.ScreenshotAsync(new() { Animations = ScreenshotAnimations.Disabled });
            }
            await VisualProof.CompareOrUpdateAsync(page, $"theme-scenario-{scenarioId}", mode, actual);
        }
    }

    private static string PrimaryPreviewSlot(string slug) => slug switch
    {
        "resizable" => "resizable-group",
        "toast" => "toast-viewport",
        _ => slug
    };
}
