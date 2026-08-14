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
