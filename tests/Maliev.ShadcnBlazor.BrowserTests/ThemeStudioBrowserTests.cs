using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeStudioBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    public static TheoryData<int, int> ReleaseViewports => new()
    {
        { 1440, 900 }, { 1024, 768 }, { 768, 1024 }, { 390, 844 }, { 320, 568 }
    };

    [Fact]
    public async Task FixedCuratedDeckScrollsInOppositeDirectionsAndPausesForInteraction()
    {
        await using var context = await NewContextAsync(1440, 900);
        var page = await OpenAsync(context);
        await Assertions.Expect(page.GetByTestId("theme-runway-columns")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("theme-runway-mobile")).ToBeHiddenAsync();
        Assert.Equal(12, (await LogicalCardIdsAsync(page)).Count);

        var before = await TrackOffsetsAsync(page);
        await page.WaitForTimeoutAsync(1800);
        var moving = await TrackOffsetsAsync(page);
        Assert.True(moving.Left > before.Left + 8, $"Left track did not move down: {before.Left} -> {moving.Left}");
        Assert.True(moving.Right < before.Right - 8, $"Right track did not move up: {before.Right} -> {moving.Right}");

        await page.GetByTestId("theme-runway").HoverAsync();
        var paused = await TrackOffsetsAsync(page);
        await page.WaitForTimeoutAsync(700);
        var still = await TrackOffsetsAsync(page);
        Assert.InRange(Math.Abs(still.Left - paused.Left), 0, 1.5);
        Assert.InRange(Math.Abs(still.Right - paused.Right), 0, 1.5);

        await page.GetByTestId("runway-dock-pause").ClickAsync();
        await Assertions.Expect(page.GetByTestId("runway-dock-pause")).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(page.GetByTestId("theme-runway")).ToHaveAttributeAsync("data-runway-paused", "true");
    }

    [Fact]
    public async Task ShuffleChangesOnlyAReviewedPresetAndNeverTheCardDeck()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var cardIds = await LogicalCardIdsAsync(page);
        var originalPreset = await page.GetByTestId("theme-preset-dock").InnerTextAsync();
        await page.GetByTestId("runway-shuffle").ClickAsync();
        Assert.Equal(cardIds, await LogicalCardIdsAsync(page));
        Assert.NotEqual(originalPreset, await page.GetByTestId("theme-preset-dock").InnerTextAsync());
        await Assertions.Expect(page.Locator("[data-testid='theme-token-light-primary']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-testid='theme-palette-seed']")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task UniversalHeaderOwnsColorAndDirectionWhileSidebarOwnsPreviewSettings()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await Assertions.Expect(page.Locator(".documentation-header")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".theme-studio-appbar, .theme-preview-toolbar")).ToHaveCountAsync(0);
        await page.GetByTestId("documentation-theme-toggle").ClickAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("dir", "rtl");
        await page.GetByTestId("viewport-tablet").ClickAsync();
        await page.GetByTestId("locale-thai").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preview-stage")).ToHaveAttributeAsync("data-preview-width", "768");
        await Assertions.Expect(page.GetByTestId("theme-studio")).ToHaveAttributeAsync("lang", "th");
        await Assertions.Expect(page.Locator("[data-use-case-id='production-capacity'] h2").First).ToContainTextAsync("กำลังการผลิต");
    }

    [Fact]
    public async Task TypographyAndIconChoicesAreScopedToThePreview()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var headerFont = await ComputedFontAsync(page.Locator(".documentation-header"));
        await page.GetByTestId("theme-font-search").FillAsync("DM Sans");
        await page.GetByTestId("theme-font-result-dm-sans").ClickAsync();
        Assert.Contains("DM Sans", await ComputedFontAsync(page.Locator("[data-use-case-id='operator-profile']").First));
        Assert.Equal(headerFont, await ComputedFontAsync(page.Locator(".documentation-header")));
        await page.GetByTestId("theme-icon-library-select").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Tabler", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-theme-icon-library", "tabler");
        await Assertions.Expect(page.GetByTestId("theme-preset-dock").Locator("[data-icon='building-factory']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByText("Maliev.ShadcnBlazor.Icons.Tabler", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Theory]
    [MemberData(nameof(ReleaseViewports))]
    public async Task RunwayIsResponsiveWithoutDocumentOverflow(int width, int height)
    {
        var errors = new List<string>();
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, true);
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-studio").WaitForAsync();
        var overflow = await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth-document.documentElement.clientWidth,document.body.scrollWidth-document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
        if (width <= 640)
        {
            await Assertions.Expect(page.GetByTestId("theme-runway-mobile")).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByTestId("theme-runway-columns")).ToBeHiddenAsync();
            await OpenSettingsAsync(page);
            await Assertions.Expect(page.GetByTestId("theme-device-controls")).ToBeHiddenAsync();
        }
        else if (width <= 1024)
        {
            await OpenSettingsAsync(page);
            await Assertions.Expect(page.GetByTestId("viewport-desktop")).ToBeHiddenAsync();
            await Assertions.Expect(page.GetByTestId("viewport-tablet")).ToBeVisibleAsync();
            await Assertions.Expect(page.GetByTestId("viewport-mobile")).ToBeVisibleAsync();
        }
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task ReducedMotionStopsRunwayAndKeepsDeterministicDemonstrationState()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var before = await TrackOffsetsAsync(page);
        var capacity = await page.Locator("[data-use-case-id='production-capacity'] [role='progressbar']").First.GetAttributeAsync("aria-valuenow");
        await page.WaitForTimeoutAsync(1200);
        var after = await TrackOffsetsAsync(page);
        Assert.InRange(Math.Abs(after.Left - before.Left), 0, 1);
        Assert.InRange(Math.Abs(after.Right - before.Right), 0, 1);
        Assert.Equal(capacity, await page.Locator("[data-use-case-id='production-capacity'] [role='progressbar']").First.GetAttributeAsync("aria-valuenow"));
    }

    [Fact]
    public async Task MobileSettingsDrawerRestoresFocusAndRunwayHasNoSeriousAxeViolations()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await OpenAsync(context);
        var toggle = page.GetByTestId("theme-controls-toggle");
        await toggle.ClickAsync();
        Assert.Equal("theme-settings-toggle", await page.GetByTestId("theme-studio-sidebar").GetAttributeAsync("data-focus-return-id"));
        await page.GetByTestId("theme-settings-close").ClickAsync();
        await Assertions.Expect(toggle).ToBeFocusedAsync();
        await toggle.ClickAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(toggle).ToBeFocusedAsync();
        var axe = await page.GetByTestId("theme-studio").RunAxe();
        Assert.DoesNotContain(axe.Violations, violation => violation.Impact is "serious" or "critical");
    }

    private async Task<IBrowserContext> NewContextAsync(int width, int height, ReducedMotion motion = ReducedMotion.NoPreference, bool touch = false) => await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = width, Height = height }, ReducedMotion = motion, HasTouch = touch });
    private async Task<IPage> OpenAsync(IBrowserContext context) { var page = await context.NewPageAsync(); await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString()); await page.GetByTestId("theme-studio").WaitForAsync(); return page; }
    private static async Task OpenSettingsAsync(IPage page) { var toggle = page.GetByTestId("theme-controls-toggle"); if (string.Equals(await toggle.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal)) await toggle.ClickAsync(); }
    private static async Task<IReadOnlyList<string>> LogicalCardIdsAsync(IPage page) => await page.Locator(".theme-runway__viewport > .theme-runway__track > [data-use-case-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.useCaseId)");
    private static async Task<(double Left, double Right)> TrackOffsetsAsync(IPage page) { var values = await page.Locator(".theme-runway__track").EvaluateAllAsync<double[]>("nodes => nodes.map(node => new DOMMatrix(getComputedStyle(node).transform).m42)"); return (values[0], values[1]); }
    private static Task<string> ComputedFontAsync(ILocator locator) => locator.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");
}
