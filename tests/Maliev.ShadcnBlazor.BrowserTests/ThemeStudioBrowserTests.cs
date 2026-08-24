using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeStudioBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    public static TheoryData<int, int> ReleaseViewports => new() { { 1440, 900 }, { 1024, 768 }, { 768, 1024 }, { 390, 844 }, { 320, 568 } };

    [Fact]
    public async Task BentoUsesOneOrderedScrollableCanvasWithoutMirrorsOrClippedBorders()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var bento = page.GetByTestId("theme-bento");
        var cards = bento.Locator("[data-use-case-id]");
        await Assertions.Expect(bento).ToBeVisibleAsync();
        Assert.Equal(19, await cards.CountAsync());
        Assert.Equal(19, (await CardIdsAsync(page)).Distinct(StringComparer.Ordinal).Count());
        await Assertions.Expect(page.Locator("[data-mirror], [data-runway-track]")).ToHaveCountAsync(0);
        var first = cards.First;
        Assert.Equal("1px", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderLeftWidth"));
        Assert.Equal("1px", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderRightWidth"));
        Assert.NotEqual("rgba(0, 0, 0, 0)", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderLeftColor"));
        var preview = page.Locator(".theme-preview-region");
        Assert.True(await preview.EvaluateAsync<bool>("element => element.scrollHeight > element.clientHeight"));
        await preview.EvaluateAsync("element => element.scrollTop = 700");
        Assert.True(await preview.EvaluateAsync<double>("element => element.scrollTop") > 100);
    }

    [Fact]
    public async Task ShuffleChangesOnlyTheCuratedThemeAndPreservesCanvasPositionAndCards()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var preview = page.Locator(".theme-preview-region");
        await preview.EvaluateAsync("element => element.scrollTop = 420");
        var beforeScroll = await preview.EvaluateAsync<double>("element => element.scrollTop");
        var beforeCards = await CardIdsAsync(page);
        var beforePreset = await page.GetByTestId("theme-preset").InnerTextAsync();
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        Assert.Equal(beforeCards, await CardIdsAsync(page));
        Assert.NotEqual(beforePreset, await page.GetByTestId("theme-preset").InnerTextAsync());
        Assert.InRange(Math.Abs(await preview.EvaluateAsync<double>("element => element.scrollTop") - beforeScroll), 0, 2);
    }

    [Fact]
    public async Task ThemeAndTypographySettingsRemainScopedToThePreview()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var shellFont = await ComputedFontAsync(page.Locator(".documentation-header"));
        var shellBackground = await page.Locator(".theme-studio-shell").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        await OpenAdvancedAsync(page, "theme-advanced-typography");
        await page.GetByTestId("theme-font-search").FillAsync("DM Sans");
        await page.GetByTestId("theme-font-result-dm-sans").ClickAsync();
        Assert.Contains("DM Sans", await ComputedFontAsync(page.Locator("[data-use-case-id='operator-profile']")));
        Assert.Equal(shellFont, await ComputedFontAsync(page.Locator(".documentation-header")));
        Assert.Equal(shellBackground, await page.Locator(".theme-studio-shell").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }

    [Fact]
    public async Task RadiusHighContrastAndAnimationControlsAffectThePreviewOnly()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='production-capacity']");
        await page.GetByTestId("theme-radius-select").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Sharp · 0", Exact = true }).ClickAsync();
        Assert.Equal(0, await card.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).borderRadius)"));
        var normalBorder = await page.GetByTestId("theme-bento").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-border').trim()");
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("preview-high-contrast").ClickAsync();
        Assert.NotEqual(normalBorder, await page.GetByTestId("theme-bento").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-border').trim()"));
        await page.GetByTestId("preview-animation-pause").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-bento")).ToHaveAttributeAsync("data-animation-paused", "true");
    }

    [Fact]
    public async Task OverlayExamplesAreFullyInteractive()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await page.Locator("[data-use-case-id='quotation-actions']").GetByText("Actions", new() { Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByText("Open details", new() { Exact = true })).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
        await page.Locator("[data-use-case-id='contact-dialog']").GetByText("Edit contact", new() { Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog).GetByText("Production contact", new() { Exact = true })).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
        await page.Locator("[data-use-case-id='file-context'] .theme-bento-context-target").ClickAsync(new() { Button = MouseButton.Right });
        await Assertions.Expect(page.GetByText("Open drawing", new() { Exact = true })).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(page.Locator("[data-use-case-id='reviewer-details']").GetByText("Kanda T.", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='tooltip-guidance']").GetByText("Surface finish", new() { Exact = true })).ToBeEnabledAsync();
    }

    [Fact]
    public async Task CardsUsePackageComponentsAndCorrectPercentages()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile'].shadcn-card")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile'] .shadcn-avatar-image")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='assistant-conversation'] .shadcn-message-group")).ToBeVisibleAsync();
        var machinePercent = await page.Locator("[data-use-case-id='machine-cell'] [data-testid='machine-load-percent']").InnerTextAsync();
        Assert.Matches("^([0-9]|[1-9][0-9]|100)%$", machinePercent);
        Assert.Equal("0", await page.Locator("[data-use-case-id='quotation-files'] .shadcn-dropzone-input").EvaluateAsync<string>("element => getComputedStyle(element).opacity"));
    }

    [Theory]
    [MemberData(nameof(ReleaseViewports))]
    public async Task BentoIsResponsiveWithoutDocumentOverflow(int width, int height)
    {
        var errors = new List<string>();
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, true);
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-bento").WaitForAsync();
        var overflow = await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth-document.documentElement.clientWidth,document.body.scrollWidth-document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
        Assert.Equal(19, await page.Locator(".theme-bento__grid > [data-use-case-id]").CountAsync());
        if (width <= 640) { await OpenSettingsAsync(page); await Assertions.Expect(page.Locator(".theme-device-options")).ToBeHiddenAsync(); }
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task MobileSettingsRestoresFocusAndPreviewHasNoSeriousAxeViolations()
    {
        await using var context = await NewContextAsync(390, 844, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var toggle = page.GetByTestId("theme-controls-toggle");
        await toggle.ClickAsync();
        await page.GetByTestId("theme-sidebar-collapse").ClickAsync();
        await Assertions.Expect(toggle).ToBeFocusedAsync();
        var axe = await page.GetByTestId("theme-studio").RunAxe();
        Assert.DoesNotContain(axe.Violations, violation => violation.Impact is "serious" or "critical");
    }

    private async Task<IBrowserContext> NewContextAsync(int width, int height, ReducedMotion motion = ReducedMotion.NoPreference, bool touch = false) => await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = width, Height = height }, ReducedMotion = motion, HasTouch = touch });
    private async Task<IPage> OpenAsync(IBrowserContext context) { var page = await context.NewPageAsync(); await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString()); await page.GetByTestId("theme-studio").WaitForAsync(); return page; }
    private static async Task OpenSettingsAsync(IPage page) { var toggle = page.GetByTestId("theme-controls-toggle"); if (string.Equals(await toggle.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal)) await toggle.ClickAsync(); }
    private static async Task OpenAdvancedAsync(IPage page, string testId) { var trigger = page.GetByTestId(testId).Locator("[data-slot='collapsible-trigger']"); if (string.Equals(await trigger.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal)) await trigger.ClickAsync(); }
    private static Task<string[]> CardIdsAsync(IPage page) => page.Locator(".theme-bento__grid > [data-use-case-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.useCaseId)");
    private static Task<string> ComputedFontAsync(ILocator locator) => locator.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");
}
