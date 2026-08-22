using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeStudioBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    public static TheoryData<int, int> ReleaseViewports => new()
    {
        { 1440, 900 },
        { 1024, 768 },
        { 768, 1024 },
        { 390, 844 },
        { 320, 568 }
    };

    [Fact]
    public async Task ThemeEditsApplyLiveUndoPersistAcrossReloadAndKeepInvalidDraftIsolated()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.NoPreference
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        var primary = page.Locator("input[data-testid='theme-token-light-primary']");
        await primary.FillAsync("#123456");
        await primary.PressAsync("Tab");
        await Assertions.Expect(page.GetByTestId("operations-dashboard-mock").Locator(".mock-brand-mark").First).ToHaveCSSAsync("background-color", "rgb(18, 52, 86)");
        await page.GetByTestId("theme-undo").ClickAsync();
        await Assertions.Expect(primary).Not.ToHaveValueAsync("#123456");
        await page.GetByTestId("theme-redo").ClickAsync();
        await Assertions.Expect(primary).ToHaveValueAsync("#123456");

        await page.ReloadAsync();
        await page.GetByTestId("theme-studio").WaitForAsync();
        await Assertions.Expect(page.Locator("input[data-testid='theme-token-light-primary']")).ToHaveValueAsync("#123456");

        await page.Locator("input[data-testid='theme-token-light-primary']").FillAsync("red; background:url(https://bad.example)");
        await page.Locator("input[data-testid='theme-token-light-primary']").PressAsync("Tab");
        await Assertions.Expect(page.GetByTestId("theme-validation-summary")).ToContainTextAsync("invalid");
        await Assertions.Expect(page.GetByTestId("operations-dashboard-mock").Locator(".mock-brand-mark").First).ToHaveCSSAsync("background-color", "rgb(18, 52, 86)");
    }

    [Fact]
    public async Task ToolbarSupportsDarkRtlThaiAndDeterministicPreviewWidthsWithoutHistoryPollution()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByTestId("mode-dark").ClickAsync();
        await page.GetByTestId("direction-rtl").ClickAsync();
        await page.GetByTestId("locale-thai").ClickAsync();
        await page.GetByTestId("viewport-tablet").ClickAsync();

        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        await Assertions.Expect(page.GetByTestId("theme-preview-stage")).ToHaveAttributeAsync("data-preview-width", "768");
        await Assertions.Expect(page.GetByTestId("operations-title")).ToContainTextAsync("ภาพรวมการผลิต");
        await Assertions.Expect(page.GetByTestId("theme-undo")).ToBeDisabledAsync();
    }

    [Fact]
    public async Task ThemeStudioPreloadsAndAppliesASelectableGoogleFontPreset()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        var studioBox = await page.GetByTestId("theme-studio").BoundingBoxAsync();
        Assert.NotNull(studioBox);
        Assert.True(studioBox.Width > 1200, $"Expected full-width Theme Studio, got {studioBox.Width}px.");
        // Bundled defaults must work offline; remote Google CSS is only added when
        // a user chooses a font that is not shipped with the package.
        await Assertions.Expect(page.Locator("link[href*='fonts.googleapis.com']")).ToHaveCountAsync(0);
        var defaultFontVariable = await page.GetByTestId("theme-preview-scope").EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--shadcn-font-sans')");
        Assert.Contains("Geist", defaultFontVariable, StringComparison.Ordinal);
        await page.GetByTestId("font-family-select").ClickAsync();
        await page.GetByText("DM Sans", new() { Exact = true }).ClickAsync();

        await Assertions.Expect(page.Locator("link[rel='stylesheet'][href*='DM+Sans']")).ToHaveCountAsync(1);

        var fontVariable = await page.GetByTestId("theme-preview-scope").EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--shadcn-font-sans')");
        Assert.Contains("DM Sans", fontVariable, StringComparison.Ordinal);

        await page.GetByTestId("monospace-font-family-select").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "JetBrains Mono", Exact = true }).ClickAsync();
        var monoVariable = await page.GetByTestId("theme-preview-scope").EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--shadcn-font-mono')");
        Assert.Contains("JetBrains Mono", monoVariable, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThemeStudioCanCollapseControlsForFullWidthPreview()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        var toggle = page.GetByTestId("theme-controls-toggle");
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");
        await toggle.ClickAsync();
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(page.GetByTestId("theme-inspector")).ToBeHiddenAsync();

        var previewBox = await page.Locator(".theme-preview-region").BoundingBoxAsync();
        Assert.NotNull(previewBox);
        Assert.InRange(previewBox.X, 0, 20);
        Assert.True(previewBox.Width > 1200, $"Expected a full-width preview, got {previewBox.Width}px.");

        await toggle.ClickAsync();
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");
        await Assertions.Expect(page.GetByTestId("theme-inspector")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MobileSettingsDrawerClosesByButtonBackdropAndEscapeThenRestoresFocus()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        var sidebar = page.GetByTestId("theme-studio-sidebar");
        var toggle = page.GetByTestId("theme-controls-toggle");
        await Assertions.Expect(sidebar).ToBeHiddenAsync();
        await toggle.ClickAsync();
        await Assertions.Expect(sidebar).ToBeVisibleAsync();
        await page.GetByTestId("theme-settings-close").ClickAsync();
        await Assertions.Expect(sidebar).ToBeHiddenAsync();
        await Assertions.Expect(toggle).ToBeFocusedAsync();

        await toggle.ClickAsync();
        await page.GetByTestId("theme-settings-backdrop").ClickAsync(new() { Position = new() { X = 380, Y = 420 } });
        await Assertions.Expect(sidebar).ToBeHiddenAsync();
        await Assertions.Expect(toggle).ToBeFocusedAsync();

        await toggle.ClickAsync();
        await page.GetByTestId("theme-settings-close").FocusAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(sidebar).ToBeHiddenAsync();
        await Assertions.Expect(toggle).ToBeFocusedAsync();

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
        var axe = await page.GetByTestId("theme-studio-appbar").RunAxe();
        Assert.DoesNotContain(axe.Violations, violation => violation.Impact is "serious" or "critical");
    }

    [Fact]
    public async Task AccessibilityPreviewControlsApplyWithoutCreatingThemeHistory()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1024, Height = 768 },
            ReducedMotion = ReducedMotion.NoPreference
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByTestId("preview-reduced-motion").ClickAsync();
        await page.GetByTestId("preview-high-contrast").ClickAsync();

        var scope = page.GetByTestId("theme-preview-scope");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-preview-reduced-motion", "true");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-preview-high-contrast", "true");
        await Assertions.Expect(page.GetByTestId("theme-undo")).ToBeDisabledAsync();
        var duration = await page.Locator(".mock-progress-track span").First.EvaluateAsync<double>(
            "element => parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(duration, 0, 0.00001);
    }

    [Fact]
    public async Task ThemeGeneratorExportsPortableJsonAndReadyToPasteCSharp()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
            Permissions = ["clipboard-read", "clipboard-write"]
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByRole(AriaRole.Combobox, new() { Name = "Generated icon library" }).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Tabler", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-theme-icon-library", "tabler");

        await page.GetByRole(AriaRole.Combobox, new() { Name = "Generated menu accent" }).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Bold", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-theme-menu-accent", "bold");
        await Assertions.Expect(page.GetByTestId("theme-generator-summary")).ToContainTextAsync("Icons Tabler");
        await Assertions.Expect(page.GetByTestId("theme-generator-summary")).ToContainTextAsync("Menu Bold / Default");

        await page.GetByTestId("theme-code-open").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-code-dialog")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("theme-code-content")).ToContainTextAsync("Generated by Maliev.ShadcnBlazor Theme Studio");

        await page.GetByTestId("theme-code-tab-json").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-code-content")).ToContainTextAsync("\"iconLibrary\": \"tabler\"");
        await Assertions.Expect(page.GetByTestId("theme-json-download")).ToBeVisibleAsync();
        var canonicalJson = await page.GetByTestId("theme-code-content").TextContentAsync();
        await page.GetByTestId("theme-code-copy").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-code-status")).ToContainTextAsync("Copied JSON output");
        var clipboardJson = await page.EvaluateAsync<string>("navigator.clipboard.readText()");
        Assert.Equal(
            canonicalJson?.Replace("\r\n", "\n", StringComparison.Ordinal),
            clipboardJson.Replace("\r\n", "\n", StringComparison.Ordinal));

        await page.EvaluateAsync("Object.defineProperty(navigator, 'clipboard', { value: undefined, configurable: true })");
        await page.GetByTestId("theme-code-copy").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-code-status")).ToContainTextAsync("Copied JSON output");
    }

    [Theory]
    [MemberData(nameof(ReleaseViewports))]
    public async Task ThemeStudioHasNoHorizontalDocumentOverflowAndKeepsCoarseTargets(int width, int height)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            HasTouch = true,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        var targetHeight = await page.GetByTestId("viewport-mobile").EvaluateAsync<double>("element => element.getBoundingClientRect().height");
        Assert.InRange(overflow, 0, 1);
        Assert.True(targetHeight >= 44, $"Expected a 44px target, got {targetHeight}px.");
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task KeyboardAndReducedMotionKeepTheInspectorOperable()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByTestId("theme-controls-toggle").ClickAsync();
        await page.Locator("input[data-testid='theme-token-light-primary']").FocusAsync();
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync("#654321");
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(page.GetByTestId("operations-dashboard-mock").Locator(".mock-brand-mark").First).ToHaveCSSAsync("background-color", "rgb(101, 67, 33)");
        var duration = await page.Locator(".mock-progress-track span").First.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(duration, 0, 0.00001);
    }

    [Theory]
    [InlineData(ColorScheme.Dark, "dark")]
    [InlineData(ColorScheme.Light, "light")]
    public async Task SystemModeFollowsTheBrowserColorScheme(ColorScheme colorScheme, string expectedTheme)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 768, Height = 1024 },
            ColorScheme = colorScheme,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();

        await page.GetByTestId("mode-system").ClickAsync();

        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-shadcn-theme", expectedTheme);
    }
}
