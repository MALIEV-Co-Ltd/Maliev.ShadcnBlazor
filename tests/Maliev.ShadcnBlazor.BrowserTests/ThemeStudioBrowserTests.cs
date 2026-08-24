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

        await page.WaitForTimeoutAsync(1800);
        var before = await TrackScrollPositionsAsync(page);
        await page.WaitForTimeoutAsync(1800);
        var moving = await TrackScrollPositionsAsync(page);
        Assert.True(moving.Left < before.Left - 8, $"Left track did not move down: {before.Left} -> {moving.Left}");
        Assert.True(moving.Right > before.Right + 8, $"Right track did not move up: {before.Right} -> {moving.Right}");

        await page.GetByTestId("theme-runway").HoverAsync();
        var paused = await TrackScrollPositionsAsync(page);
        await page.WaitForTimeoutAsync(700);
        var still = await TrackScrollPositionsAsync(page);
        Assert.InRange(Math.Abs(still.Left - paused.Left), 0, 1.5);
        Assert.InRange(Math.Abs(still.Right - paused.Right), 0, 1.5);

        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("runway-pause").ClickAsync();
        await Assertions.Expect(page.GetByTestId("runway-pause")).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByTestId("theme-runway")).ToHaveAttributeAsync("data-runway-paused", "true");
    }

    [Fact]
    public async Task ShuffleChangesOnlyAReviewedPresetAndNeverTheCardDeck()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var cardIds = await LogicalCardIdsAsync(page);
        var originalPreset = await page.GetByTestId("theme-preset").InnerTextAsync();
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        Assert.Equal(cardIds, await LogicalCardIdsAsync(page));
        Assert.NotEqual(originalPreset, await page.GetByTestId("theme-preset").InnerTextAsync());
        await Assertions.Expect(page.Locator("[data-testid='theme-token-light-primary']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-testid='theme-palette-seed']")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ShufflePreservesTheRunwayPositionAndDemonstrationFrame()
    {
        await using var context = await NewContextAsync(1440, 900);
        var page = await OpenAsync(context);
        await page.WaitForTimeoutAsync(2400);
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("runway-pause").ClickAsync();
        var before = await TrackScrollPositionsAsync(page);
        var capacityBefore = await page.Locator("[data-use-case-id='production-capacity'] [role='progressbar']").First.GetAttributeAsync("aria-valuenow");

        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        var after = await TrackScrollPositionsAsync(page);
        var capacityAfter = await page.Locator("[data-use-case-id='production-capacity'] [role='progressbar']").First.GetAttributeAsync("aria-valuenow");

        Assert.InRange(Math.Abs(after.Left - before.Left), 0, 24);
        Assert.InRange(Math.Abs(after.Right - before.Right), 0, 24);
        Assert.Equal(capacityBefore, capacityAfter);
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
        await Assertions.Expect(page.Locator("[data-use-case-id='production-capacity'] .shadcn-card-title").First).ToContainTextAsync("กำลังการผลิต");
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
        await OpenAdvancedAsync(page, "theme-advanced-icons");
        await page.GetByTestId("theme-icon-library-tabler").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-theme-icon-library", "tabler");
        await Assertions.Expect(page.GetByText("Maliev.ShadcnBlazor.Icons.Tabler", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CompanyShellStaysStableAndPreviewAccessibilityStartsOff()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var shellFont = await ComputedFontAsync(page.Locator(".documentation-header"));
        var shellBackground = await page.Locator(".theme-studio-shell").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");

        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await Assertions.Expect(page.GetByTestId("preview-high-contrast")).Not.ToBeCheckedAsync();
        await page.GetByTestId("theme-font-search").FillAsync("DM Sans");
        await page.GetByTestId("theme-font-result-dm-sans").ClickAsync();
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();

        Assert.Equal(shellFont, await ComputedFontAsync(page.Locator(".documentation-header")));
        Assert.Equal(shellBackground, await page.Locator(".theme-studio-shell").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        Assert.Equal("start", await page.Locator(".theme-font-results button").First.EvaluateAsync<string>("element => getComputedStyle(element).textAlign"));
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("data-preview-high-contrast", "false");
    }

    [Fact]
    public async Task PreviewControlsChangeRadiusTypographyAndHighContrastInsideTheRunwayOnly()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='production-capacity']").First;
        var title = card.Locator(".shadcn-card-title");
        var shellBorder = await page.Locator(".documentation-header").EvaluateAsync<string>("element => getComputedStyle(element).borderBottomColor");
        var initialRadius = await card.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).borderRadius)");

        await page.GetByTestId("theme-radius-select").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Sharp · 0", Exact = true }).ClickAsync();
        Assert.Equal(0, await card.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).borderRadius)"));
        Assert.True(initialRadius > 0);

        await OpenAdvancedAsync(page, "theme-advanced-typography");
        await page.GetByTestId("theme-role-heading-4-to-6-weight").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "900", Exact = true }).ClickAsync();
        Assert.Equal("900", await title.EvaluateAsync<string>("element => getComputedStyle(element).fontWeight"));

        var normalBorder = await page.GetByTestId("theme-runway").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-border').trim()");
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("preview-high-contrast").ClickAsync();
        var contrastBorder = await page.GetByTestId("theme-runway").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-border').trim()");
        Assert.NotEqual(normalBorder, contrastBorder);
        Assert.Equal(shellBorder, await page.Locator(".documentation-header").EvaluateAsync<string>("element => getComputedStyle(element).borderBottomColor"));
    }

    [Fact]
    public async Task RunwayAllowsManualScrollingAndKeepsNativeDropzoneInputVisuallyHidden()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var viewport = page.Locator("[data-runway-track='right']");
        var before = await viewport.EvaluateAsync<double>("element => element.scrollTop");
        await viewport.HoverAsync();
        await page.Mouse.WheelAsync(0, 320);
        var after = await viewport.EvaluateAsync<double>("element => element.scrollTop");
        Assert.True(after > before + 100, $"Manual runway scroll did not move: {before} -> {after}");
        Assert.Equal("0", await page.Locator("[data-use-case-id='quotation-files'] .shadcn-dropzone-input").First.EvaluateAsync<string>("element => getComputedStyle(element).opacity"));
    }

    [Fact]
    public async Task ComponentCoverageOffersThreeInteractiveExamplesForEveryComponentWithoutInflatingTheRunway()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        Assert.Equal(36, await page.Locator(".theme-use-case-card").CountAsync());
        await page.GetByTestId("preview-surface-coverage").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-runway")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByTestId("theme-scenario-browser")).ToBeVisibleAsync();
        Assert.Equal(3, await page.Locator("[data-testid^='theme-scenario-kind-']").CountAsync());

        await page.GetByTestId("theme-scenario-search").FillAsync("Dropzone");
        await page.Locator("[data-theme-scenario-component='dropzone']").ClickAsync();
        await page.GetByTestId("theme-scenario-kind-default").ClickAsync();
        var scenarioDropzone = page.Locator("[data-theme-scenario-host='dropzone-default']");
        var scenarioInput = scenarioDropzone.Locator(".shadcn-dropzone-input");
        await Assertions.Expect(scenarioInput).ToBeEnabledAsync();
        await scenarioInput.SetInputFilesAsync(new FilePayload
        {
            Name = "bracket.step",
            MimeType = "application/octet-stream",
            Buffer = "solid-model"u8.ToArray()
        });
        await Assertions.Expect(scenarioDropzone.Locator(".shadcn-dropzone-status")).ToHaveTextAsync("1 file selected");
    }

    [Fact]
    public async Task RunwayCommunicatesAutomaticAndInteractionPauseStates()
    {
        await using var context = await NewContextAsync(1280, 900);
        var page = await OpenAsync(context);
        var status = page.GetByTestId("runway-motion-status");

        await Assertions.Expect(status).ToContainTextAsync("running");
        await page.GetByTestId("theme-runway").HoverAsync();
        await Assertions.Expect(status).ToContainTextAsync("interact");
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("runway-pause").ClickAsync();
        await Assertions.Expect(status).ToContainTextAsync("paused");
    }

    [Fact]
    public async Task SemanticSuccessStylingSurvivesAccentChangesAndHistoryControlsMeetTouchSize()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce, true);
        var page = await OpenAsync(context);
        var success = page.Locator("[data-use-case-id='production-capacity'] .theme-status-success.shadcn-badge").First;
        var before = await success.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");

        await page.GetByTestId("theme-preset").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Ruby Alert", Exact = true }).ClickAsync();
        Assert.Equal(before, await success.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));

        foreach (var control in await page.Locator(".theme-history-actions .shadcn-button").AllAsync())
        {
            var box = await control.BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Width >= 44 && box.Height >= 44, $"History control measured {box.Width}x{box.Height}.");
        }
    }

    [Fact]
    public async Task DesktopRunwayFillsTheAvailableHeightAndHasNoWrapperSurface()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var geometry = await page.GetByTestId("theme-runway").EvaluateAsync<RunwayGeometry>("element => { const style=getComputedStyle(element); const columns=element.querySelector('[data-testid=theme-runway-columns]').getBoundingClientRect(); return { borderWidth: style.borderTopWidth, radius: style.borderRadius, background: style.backgroundColor, bottom: columns.bottom, gap: parseFloat(getComputedStyle(element.querySelector('.theme-runway__track')).gap), beforeContent: getComputedStyle(element, '::before').content }; }");

        Assert.Equal("0px", geometry.BorderWidth);
        Assert.Equal("0px", geometry.Radius);
        Assert.Equal("rgba(0, 0, 0, 0)", geometry.Background);
        Assert.True(geometry.Bottom >= 868, $"Runway stopped at {geometry.Bottom}px in a 900px viewport.");
        Assert.True(geometry.Gap >= 24, $"Curated cards are packed too tightly: {geometry.Gap}px.");
        Assert.Equal("none", geometry.BeforeContent);
    }

    [Fact]
    public async Task RunwayUsesPackageCardsMessagesAvatarsAndCorrectPercentages()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile'].shadcn-card").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile'] .shadcn-avatar-image").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='assistant-conversation'] .shadcn-message-group").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='assistant-conversation'] .shadcn-bubble").First).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='machine-cell'] [data-testid='machine-load-percent']").First).ToHaveTextAsync("75%");
        await Assertions.Expect(page.Locator(".theme-use-case-card__eyebrow, [data-use-case-id] > .theme-use-case-card__header .shadcn-badge")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task AssistantMessageUsesASmoothCssRevealAndReducedMotionShowsTheFullText()
    {
        const string message = "กำหนดส่งยังเป็นวันศุกร์ เวลา 16:00 น. งานกัดเสร็จแล้วและกำลังรอรายงานตรวจสอบขั้นสุดท้าย";
        await using var animatedContext = await NewContextAsync(1280, 900);
        var animatedPage = await OpenAsync(animatedContext);
        var animated = animatedPage.Locator("[data-use-case-id='assistant-conversation'] .theme-runway-typing").First;
        await Assertions.Expect(animated).ToHaveTextAsync(message);
        Assert.Equal("theme-runway-typing-reveal", await animated.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        await using var reducedContext = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var reducedPage = await OpenAsync(reducedContext);
        var reduced = reducedPage.Locator("[data-use-case-id='assistant-conversation'] .theme-runway-typing").First;
        await Assertions.Expect(reduced).ToHaveTextAsync(message);
        Assert.Equal("none", await reduced.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        Assert.Equal("none", await reduced.EvaluateAsync<string>("element => getComputedStyle(element).clipPath"));
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
            Assert.Equal(12, await page.Locator(".theme-runway__mobile > [data-use-case-id]").CountAsync());
            await OpenSettingsAsync(page);
            await Assertions.Expect(page.GetByTestId("theme-device-controls")).ToBeVisibleAsync();
            await Assertions.Expect(page.Locator(".theme-device-options")).ToBeHiddenAsync();
            await Assertions.Expect(page.GetByTestId("preview-surface-coverage")).ToBeVisibleAsync();
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
        var before = await TrackScrollPositionsAsync(page);
        var capacity = await page.Locator("[data-use-case-id='production-capacity'] [role='progressbar']").First.GetAttributeAsync("aria-valuenow");
        await page.WaitForTimeoutAsync(1200);
        var after = await TrackScrollPositionsAsync(page);
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
        await page.GetByTestId("theme-sidebar-collapse").ClickAsync();
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
    private static async Task OpenAdvancedAsync(IPage page, string testId)
    {
        var disclosure = page.GetByTestId(testId);
        var trigger = disclosure.Locator("[data-slot='collapsible-trigger']");
        if (string.Equals(await trigger.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal))
            await trigger.ClickAsync();
    }
    private static async Task<IReadOnlyList<string>> LogicalCardIdsAsync(IPage page) => await page.Locator(".theme-runway__viewport > .theme-runway__track > [data-use-case-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.useCaseId)");
    private static async Task<(double Left, double Right)> TrackScrollPositionsAsync(IPage page) { var values = await page.Locator(".theme-runway__viewport").EvaluateAllAsync<double[]>("nodes => nodes.map(node => node.scrollTop)"); return (values[0], values[1]); }
    private static Task<string> ComputedFontAsync(ILocator locator) => locator.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");
    private sealed class RunwayGeometry
    {
        public string BorderWidth { get; set; } = string.Empty;
        public string Radius { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public double Bottom { get; set; }
        public double Gap { get; set; }
        public string BeforeContent { get; set; } = string.Empty;
    }
}
