using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ActionsAndSelectionBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    public static TheoryData<int, int, string, string, string> ReleaseMatrix => new()
    {
        { 1440, 900, "light", "ltr", "en" },
        { 768, 1024, "dark", "rtl", "en" },
        { 390, 844, "light", "ltr", "th" },
        { 320, 568, "dark", "rtl", "th" }
    };

    [Theory]
    [MemberData(nameof(ReleaseMatrix))]
    public async Task FamilyRouteIsResponsiveLocalizedAndThemeDirectionAware(
        int width,
        int height,
        string theme,
        string direction,
        string locale)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(FamilyUrl(theme, direction, locale));
        var fixture = page.GetByTestId("actions-selection-fixture");
        await fixture.WaitForAsync();

        var scope = page.Locator("[data-shadcn-scope]");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-shadcn-theme", theme);
        await Assertions.Expect(scope).ToHaveAttributeAsync("dir", direction);
        await Assertions.Expect(fixture).ToHaveAttributeAsync("data-locale", locale);
        var actions = page.Locator("section[aria-labelledby='actions-heading']");
        var selection = page.Locator("section[aria-labelledby='selection-heading']");
        await Assertions.Expect(actions.Locator("[data-slot='button']")).ToHaveCountAsync(27);
        await Assertions.Expect(selection.Locator("[data-slot='checkbox']")).ToHaveCountAsync(5);
        await Assertions.Expect(selection.Locator("[data-slot='radio-group']")).ToHaveCountAsync(4);
        await Assertions.Expect(selection.Locator("[data-slot='slider']")).ToHaveCountAsync(6);
        await Assertions.Expect(selection.Locator("[data-slot='switch']")).ToHaveCountAsync(5);
        await Assertions.Expect(actions.Locator("[data-slot='toggle-group']")).ToHaveCountAsync(4);

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));

        var screenshot = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-actions-{width}-{theme}-{direction}-{locale}.png");
        await page.ScreenshotAsync(new() { Path = screenshot, FullPage = true, Animations = ScreenshotAnimations.Disabled });
        Assert.True(File.Exists(screenshot));
    }

    [Fact]
    public async Task KeyboardAndPointerInteractionsPreserveControlledState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1000 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var toggle = page.GetByTestId("toggle-bold");
        await toggle.ClickAsync();
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-pressed", "false");

        var toggleItems = page.GetByTestId("toggle-group").Locator("[data-slot='toggle-group-item']");
        await Assertions.Expect(page.GetByTestId("toggle-group").Locator("[data-slot='toggle-group-item'][tabindex='0']")).ToHaveCountAsync(1);
        await toggleItems.Nth(0).FocusAsync();
        await toggleItems.Nth(0).PressAsync("ArrowRight");
        Assert.Equal("Italic", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
        await page.Keyboard.PressAsync("ArrowRight");
        Assert.Equal("Bold", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
        await page.Keyboard.PressAsync("End");
        Assert.Equal("Italic", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));

        var radioItems = page.GetByTestId("radio-group").Locator("[data-slot='radio-group-item']");
        await Assertions.Expect(page.GetByTestId("radio-group").Locator("[data-slot='radio-group-item'][tabindex='0']")).ToHaveCountAsync(1);
        await radioItems.Nth(1).FocusAsync();
        await radioItems.Nth(1).PressAsync("ArrowRight");
        await Assertions.Expect(radioItems.Nth(0)).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByTestId("selection-status")).ToContainTextAsync("default");

        var checkbox = page.GetByTestId("checkbox");
        await checkbox.CheckAsync();
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("aria-checked", "true");
        await Assertions.Expect(page.GetByTestId("checkbox-indeterminate")).ToHaveAttributeAsync("aria-checked", "mixed");

        var switchControl = page.GetByTestId("switch");
        await switchControl.UncheckAsync();
        await Assertions.Expect(switchControl).ToHaveAttributeAsync("aria-checked", "false");

        var firstThumb = page.GetByTestId("slider").Locator("input[type='range']").First;
        await firstThumb.FocusAsync();
        await firstThumb.PressAsync("ArrowRight");
        await Assertions.Expect(firstThumb).ToHaveAttributeAsync("aria-valuenow", "25");

        var slider = page.GetByTestId("slider");
        var box = await slider.BoundingBoxAsync();
        Assert.NotNull(box);
        await page.Mouse.ClickAsync((float)(box!.X + box.Width * 0.7), (float)(box.Y + box.Height / 2));
        await Assertions.Expect(slider.Locator("input[type='range']").Nth(1)).ToHaveAttributeAsync("aria-valuenow", "70");

        var stableIds = await page.GetByTestId("slider").Locator("input[type='range']").EvaluateAllAsync<string[]>(
            "elements => elements.map(element => element.id)");
        Assert.All(stableIds, id => Assert.False(string.IsNullOrWhiteSpace(id)));
        await page.GetByTestId("button-action").ClickAsync();
        Assert.Equal(stableIds, await page.GetByTestId("slider").Locator("input[type='range']").EvaluateAllAsync<string[]>(
            "elements => elements.map(element => element.id)"));

        await Assertions.Expect(firstThumb).ToHaveAttributeAsync("form", "slider-form");
        await Assertions.Expect(firstThumb).ToHaveAttributeAsync("required", string.Empty);
        var formPayload = await page.GetByTestId("slider-form").EvaluateAsync<string[][]>(
            "form => Array.from(new FormData(form).entries(), entry => [entry[0], String(entry[1])])");
        Assert.Equal([["budget", "25"], ["budget", "70"], ["level", "35"]], formPayload);

        var vertical = page.GetByTestId("slider-vertical");
        await vertical.ScrollIntoViewIfNeededAsync();
        var verticalBox = await vertical.BoundingBoxAsync();
        Assert.NotNull(verticalBox);
        await page.Mouse.ClickAsync((float)(verticalBox!.X + verticalBox.Width / 2), (float)(verticalBox.Y + verticalBox.Height * 0.2));
        await Assertions.Expect(vertical.Locator("input[type='range']")).ToHaveAttributeAsync("aria-valuenow", "80");

        var rtlItems = page.GetByTestId("rtl-toggle-group").Locator("[data-slot='toggle-group-item']");
        await rtlItems.Nth(0).FocusAsync();
        await rtlItems.Nth(0).PressAsync("ArrowLeft");
        Assert.Equal("شبكة", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
    }

    [Fact]
    public async Task CompleteRovingAndGroupedKeyboardContractsWorkInEveryOrientation()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var verticalGroup = page.GetByTestId("button-group-vertical");
        var verticalButtons = verticalGroup.Locator("[data-slot='button']");
        await verticalButtons.First.FocusAsync();
        await page.Keyboard.PressAsync("Tab");
        Assert.Equal("Vertical second", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
        await page.Keyboard.PressAsync("Tab");
        Assert.Equal("Nested first", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
        await Assertions.Expect(verticalButtons.Nth(2)).ToBeDisabledAsync();

        var radio = page.GetByTestId("radio-vertical").Locator("[data-slot='radio-group-item']");
        await radio.Nth(1).FocusAsync();
        await radio.Nth(1).PressAsync("Home");
        await Assertions.Expect(radio.Nth(0)).ToBeCheckedAsync();
        await page.Keyboard.PressAsync("ArrowUp");
        await Assertions.Expect(radio.Nth(3)).ToBeCheckedAsync();
        await page.Keyboard.PressAsync("End");
        await Assertions.Expect(radio.Nth(3)).ToBeCheckedAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(radio.Nth(0)).ToBeCheckedAsync();

        var verticalToggles = page.GetByTestId("toggle-group-vertical").Locator("[data-slot='toggle-group-item']");
        await verticalToggles.First.FocusAsync();
        await verticalToggles.First.PressAsync("ArrowDown");
        Assert.Equal("Vertical italic", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));
        await page.Keyboard.PressAsync("ArrowDown");
        Assert.Equal("Vertical bold", await page.EvaluateAsync<string>("document.activeElement.textContent.trim()"));

        var rtlSlider = page.GetByTestId("rtl-slider");
        var rtlThumb = rtlSlider.Locator("input[type='range']");
        await rtlThumb.FocusAsync();
        await rtlThumb.PressAsync("PageUp");
        await Assertions.Expect(rtlThumb).ToHaveAttributeAsync("aria-valuenow", "45");
        await rtlThumb.PressAsync("PageDown");
        await Assertions.Expect(rtlThumb).ToHaveAttributeAsync("aria-valuenow", "35");
        var box = await rtlSlider.BoundingBoxAsync();
        Assert.NotNull(box);
        await page.Mouse.ClickAsync((float)(box!.X + box.Width * 0.2), (float)(box.Y + box.Height / 2));
        await Assertions.Expect(rtlThumb).ToHaveAttributeAsync("aria-valuenow", "80");
    }

    [Fact]
    public async Task NestedVerticalButtonGroupMatchesPinnedVegaGeometry()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("dark", "rtl", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var defaultMetrics = await page.GetByTestId("button-group").EvaluateAsync<string[]>("""
            root => {
                const style = element => getComputedStyle(element);
                const first = root.children[0];
                const last = root.children[2];
                return [
                    style(first).borderTopLeftRadius,
                    style(first).borderTopRightRadius,
                    style(last).borderTopLeftRadius,
                    style(last).borderTopRightRadius,
                    style(last).borderRightWidth
                ];
            }
            """);
        Assert.Equal(["0px", "8px", "8px", "0px", "0px"], defaultMetrics);

        var metrics = await page.GetByTestId("button-group-vertical").EvaluateAsync<string[]>("""
            root => {
                const style = element => getComputedStyle(element);
                const text = root.children[0];
                const second = root.children[2];
                const nested = root.children[4];
                return [
                    style(root).gap,
                    style(text).paddingLeft,
                    style(text).paddingRight,
                    style(text).borderTopLeftRadius,
                    style(text).borderBottomLeftRadius,
                    style(second).borderTopWidth,
                    style(nested).borderBottomLeftRadius,
                    style(nested).borderBottomRightRadius,
                    String(style(text).boxShadow.includes('0.05'))
                ];
            }
            """);

        Assert.Equal(["normal", "10px", "10px", "8px", "0px", "0px", "8px", "8px", "true"], metrics);
    }

    [Fact]
    public async Task DarkInvalidSwitchMatchesPinnedVegaStateGeometry()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("dark", "rtl", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var metrics = await page.GetByTestId("switch-evidence").EvaluateAsync<string[]>(("""
            root => {
                const control = root.querySelector('[data-slot=switch][aria-invalid=true]');
                const wrapper = control.closest('[data-slot=switch-root]');
                const thumb = wrapper.querySelector('[data-slot=switch-thumb]');
                const style = element => getComputedStyle(element);
                const scope = root.closest('.shadcn-scope');
                const foreground = style(scope).getPropertyValue('--shadcn-foreground').trim();
                return [
                    style(control).borderTopWidth,
                    style(control).borderTopStyle,
                    style(thumb).width,
                    style(thumb).height,
                    String(style(thumb).backgroundColor === foreground),
                    String(style(control).boxShadow.includes('/ 0.4)'))
                ];
            }
            """));

        Assert.Equal(["1px", "solid", "16px", "16px", "true", "true"], metrics);
    }

    [Fact]
    public async Task BlazorIntentionalStateCorrectionsRemainVisibleAndSemantic()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("dark", "rtl", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var checkbox = page.GetByTestId("checkbox-indeterminate");
        await Assertions.Expect(checkbox).ToHaveAttributeAsync("aria-checked", "mixed");
        var mark = await checkbox.Locator("xpath=..").Locator("[data-slot=checkbox-indicator]").EvaluateAsync<string[]>("""
            indicator => {
                const mark = getComputedStyle(indicator, '::after');
                return [mark.width, mark.height, mark.backgroundColor];
            }
            """);
        Assert.Equal("8px", mark[0]);
        Assert.Equal("2px", mark[1]);
        Assert.NotEqual("rgba(0, 0, 0, 0)", mark[2]);

        var slider = page.GetByTestId("slider-vertical");
        var sliderBox = await slider.BoundingBoxAsync();
        var trackBox = await slider.Locator("[data-slot=slider-track]").BoundingBoxAsync();
        Assert.NotNull(sliderBox);
        Assert.NotNull(trackBox);
        Assert.Equal(160, sliderBox!.Height);
        Assert.Equal(6, trackBox!.Width);
        Assert.True(trackBox.Height > trackBox.Width);
        await slider.Locator("input[type=range]").FocusAsync();
        await slider.Locator("input[type=range]").PressAsync("ArrowUp");
        await Assertions.Expect(slider.Locator("input[type=range]")).ToHaveAttributeAsync("aria-valuenow", "46");
    }

    [Fact]
    public async Task DisabledAndReadOnlyControlsSuppressDomAndModelMutation()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1024, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("dark", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        await page.GetByTestId("button-action").ClickAsync();
        await Assertions.Expect(page.GetByTestId("button-action")).ToHaveTextAsync("Invoked 1");
        await page.GetByTestId("button-disabled").DispatchEventAsync("click");
        await Assertions.Expect(page.GetByTestId("button-action")).ToHaveTextAsync("Invoked 1");
        await Assertions.Expect(page.GetByTestId("toggle-disabled")).ToHaveAttributeAsync("aria-pressed", "true");

        var checkbox = page.GetByTestId("checkbox-readonly");
        await checkbox.ClickAsync();
        await Assertions.Expect(checkbox).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByTestId("checkbox-disabled")).ToBeDisabledAsync();

        var radio = page.GetByTestId("radio-readonly").Locator("input[type='radio']");
        await radio.Nth(1).ClickAsync();
        await Assertions.Expect(radio.Nth(0)).ToBeCheckedAsync();
        await Assertions.Expect(radio.Nth(1)).Not.ToBeCheckedAsync();

        var switchControl = page.GetByTestId("switch-readonly");
        await switchControl.ClickAsync();
        await Assertions.Expect(switchControl).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByTestId("switch-disabled")).ToBeDisabledAsync();

        var slider = page.GetByTestId("slider-readonly").Locator("input[type='range']");
        await slider.FocusAsync();
        await slider.PressAsync("ArrowRight");
        await Assertions.Expect(slider).ToHaveAttributeAsync("aria-valuenow", "60");
        var sliderBox = await slider.BoundingBoxAsync();
        Assert.NotNull(sliderBox);
        await page.Mouse.MoveAsync((float)(sliderBox!.X + sliderBox.Width * 0.8), (float)(sliderBox.Y + sliderBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync((float)(sliderBox.X + sliderBox.Width * 0.2), (float)(sliderBox.Y + sliderBox.Height / 2));
        await page.Mouse.UpAsync();
        await Assertions.Expect(slider).ToHaveValueAsync("60");
        await Assertions.Expect(slider).ToHaveAttributeAsync("aria-valuenow", "60");
    }

    [Fact]
    public async Task VegaGeometryFocusAndReducedMotionMatchTheActionContract()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("button-sizes").Locator("[data-size='default']")).ToHaveCSSAsync("height", "36px");
        await Assertions.Expect(page.GetByTestId("button-sizes").Locator("[data-size='xs']")).ToHaveCSSAsync("height", "24px");
        await Assertions.Expect(page.GetByTestId("button-sizes").Locator("[data-size='sm']")).ToHaveCSSAsync("height", "32px");
        await Assertions.Expect(page.GetByTestId("button-sizes").Locator("[data-size='lg']")).ToHaveCSSAsync("height", "40px");
        await Assertions.Expect(page.GetByTestId("slider").Locator("[data-slot='slider-track']")).ToHaveCSSAsync("height", "6px");
        await Assertions.Expect(page.GetByTestId("button-action")).ToHaveCSSAsync("cursor", "default");
        await Assertions.Expect(page.GetByTestId("button-with-icons")).ToHaveCSSAsync("cursor", "pointer");
        await Assertions.Expect(page.GetByTestId("button-with-icons")).ToHaveCSSAsync("padding-left", "8px");
        await Assertions.Expect(page.GetByTestId("button-with-icons")).ToHaveCSSAsync("padding-right", "8px");
        await Assertions.Expect(page.GetByTestId("button-expanded")).Not.ToHaveCSSAsync("background-color", "rgba(0, 0, 0, 0)");
        await Assertions.Expect(page.GetByTestId("button-invalid")).Not.ToHaveCSSAsync("border-color", "rgba(0, 0, 0, 0)");
        await Assertions.Expect(page.GetByTestId("toggle-invalid")).Not.ToHaveCSSAsync("border-color", "rgba(0, 0, 0, 0)");
        var connected = page.GetByTestId("toggle-group-connected");
        await Assertions.Expect(connected).ToHaveCSSAsync("gap", "0px");
        await Assertions.Expect(connected.Locator("[data-slot='toggle-group-item']").Nth(1)).ToHaveCSSAsync("border-left-width", "0px");
        var verticalThumb = page.GetByTestId("slider-vertical").Locator("input[type='range']");
        await Assertions.Expect(verticalThumb).ToHaveCSSAsync("writing-mode", "vertical-lr");
        await Assertions.Expect(verticalThumb).ToHaveCSSAsync("direction", "rtl");
        var verticalRootBox = await page.GetByTestId("slider-vertical").BoundingBoxAsync();
        var verticalThumbBox = await verticalThumb.BoundingBoxAsync();
        Assert.NotNull(verticalRootBox);
        Assert.NotNull(verticalThumbBox);
        Assert.True(verticalThumbBox!.Height > verticalThumbBox.Width * 4);
        Assert.InRange(verticalThumbBox.Y, verticalRootBox!.Y - 1, verticalRootBox.Y + 1);

        await verticalThumb.FocusAsync();
        await verticalThumb.PressAsync("ArrowUp");
        await Assertions.Expect(verticalThumb).ToHaveAttributeAsync("aria-valuenow", "46");
        await verticalThumb.PressAsync("End");
        await Assertions.Expect(verticalThumb).ToHaveAttributeAsync("aria-valuenow", "100");
        await verticalThumb.PressAsync("Home");
        await Assertions.Expect(verticalThumb).ToHaveAttributeAsync("aria-valuenow", "0");

        Assert.True(await page.EvaluateAsync<bool>(
            "() => Array.from(document.styleSheets).some(sheet => Array.from(sheet.cssRules || []).some(rule => rule.selectorText?.includes('.shadcn-button:active:not([aria-haspopup])') && rule.style.transform === 'translateY(1px)'))"));

        var focusTarget = page.GetByTestId("button-action");
        await focusTarget.FocusAsync();
        Assert.NotEqual("none", await focusTarget.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
        var transitionDuration = await focusTarget.EvaluateAsync<double>(
            "element => parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(transitionDuration, 0, 0.00001);

        var lightOutlineBackground = await page.GetByTestId("button-outline").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await page.GetByTestId("theme-toggle").ClickAsync();
        await Assertions.Expect(page.Locator("[data-shadcn-scope]")).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        Assert.NotEqual(lightOutlineBackground, await page.GetByTestId("button-outline").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));

        var darkOutline = page.GetByTestId("button-outline");
        await darkOutline.HoverAsync();
        await page.WaitForTimeoutAsync(100);
        var darkHoverColors = await darkOutline.EvaluateAsync<string[]>("""
            element => {
                const probe = document.createElement('span');
                probe.style.background = 'color-mix(in oklch, var(--shadcn-input) 50%, transparent)';
                element.parentElement.appendChild(probe);
                const expected = getComputedStyle(probe).backgroundColor;
                probe.remove();
                const matching = Array.from(document.styleSheets)
                    .flatMap(sheet => Array.from(sheet.cssRules || []))
                    .filter(rule => rule.selectorText?.includes('outline') && rule.selectorText?.includes(':hover') && element.matches(rule.selectorText))
                    .map(rule => `${rule.selectorText} => ${rule.style.background}`)
                    .join(' | ');
                return [getComputedStyle(element).backgroundColor, expected, String(element.matches(':hover')), matching];
            }
            """);
        Assert.True(darkHoverColors[0] == darkHoverColors[1], string.Join(Environment.NewLine, darkHoverColors));

        var noIcon = page.GetByTestId("toggle-no-icon");
        var leading = page.GetByTestId("toggle-leading-icon");
        var trailing = page.GetByTestId("toggle-trailing-icon");
        await Assertions.Expect(noIcon).ToHaveCSSAsync("padding-left", "10px");
        await Assertions.Expect(noIcon).ToHaveCSSAsync("padding-right", "10px");
        await Assertions.Expect(leading).ToHaveCSSAsync("padding-left", "6px");
        await Assertions.Expect(leading).ToHaveCSSAsync("padding-right", "10px");
        await Assertions.Expect(trailing).ToHaveCSSAsync("padding-left", "8px");
        await Assertions.Expect(trailing).ToHaveCSSAsync("padding-right", "10px");
        await Assertions.Expect(page.GetByTestId("toggle-group-icon-item")).ToHaveCSSAsync("padding-left", "6px");
    }

    [Fact]
    public async Task CoarsePointerAndForcedColorsRetainTargetsAndState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            HasTouch = true,
            IsMobile = true,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        foreach (var testId in new[] { "checkbox", "switch", "checkbox-readonly" })
        {
            var size = await page.GetByTestId(testId).EvaluateAsync<double>("element => element.parentElement.getBoundingClientRect().height");
            Assert.True(size >= 44, $"Expected a 44px coarse target for {testId}, got {size}px.");
        }

        Assert.True(await page.EvaluateAsync<bool>("matchMedia('(forced-colors: active)').matches"));
        await Assertions.Expect(page.GetByTestId("toggle-bold")).ToHaveCSSAsync("forced-color-adjust", "none");
    }

    [Fact]
    public async Task SemanticSnapshotsAndLocalAccessibilityAuditCoverEveryComponent()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1000 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var snapshot = await page.GetByTestId("actions-selection-fixture").AriaSnapshotAsync();
        foreach (var semantic in new[]
                 {
                     "heading \"Actions\"", "button \"Default\"", "group \"Drawing actions\"",
                     "button \"Bold\"", "group \"Text formatting\"", "checkbox \"Accept terms\"",
                     "radiogroup \"Density\"", "switch \"Notifications\"", "slider \"Budget range\""
                 })
            Assert.True(snapshot.Contains(semantic, StringComparison.OrdinalIgnoreCase), $"Missing {semantic}{Environment.NewLine}{snapshot}");

        var violations = await page.GetByTestId("actions-selection-fixture").EvaluateAsync<string[]>("""
            root => {
                const violations = [];
                const controls = root.querySelectorAll('button, a[href], input, [role="group"], [role="radiogroup"]');
                const ids = new Set();
                for (const element of controls) {
                    if (element.id && ids.has(element.id)) violations.push(`duplicate-id:${element.id}`);
                    if (element.id) ids.add(element.id);
                    const labelledBy = element.getAttribute('aria-labelledby');
                    const labelled = labelledBy && labelledBy.split(/\s+/).every(id => document.getElementById(id));
                    const name = element.getAttribute('aria-label')?.trim()
                        || (labelled ? labelledBy.split(/\s+/).map(id => document.getElementById(id).textContent).join(' ').trim() : '')
                        || Array.from(element.labels || []).map(label => label.textContent).join(' ').trim()
                        || element.textContent?.trim();
                    if (!name && !element.matches('[role="separator"]')) violations.push(`missing-name:${element.outerHTML.slice(0, 120)}`);
                    if (labelledBy && !labelled) violations.push(`broken-labelledby:${labelledBy}`);
                    const describedBy = element.getAttribute('aria-describedby');
                    if (describedBy && !describedBy.split(/\s+/).every(id => document.getElementById(id))) violations.push(`broken-describedby:${describedBy}`);
                    if (element.matches('button') && !['button', 'submit', 'reset'].includes(element.type)) violations.push(`invalid-button-type:${element.type}`);
                    if (element.matches('[role="radiogroup"]') && !element.querySelector('input[type="radio"]')) violations.push('empty-radiogroup');
                    if (element.matches('[role="group"]') && !element.querySelector('button, input')) violations.push('empty-group');
                }
                return violations;
            }
            """);
        Assert.Empty(violations);
    }

    [Theory]
    [InlineData("button", "button-evidence")]
    [InlineData("button-group", "button-group-evidence")]
    [InlineData("checkbox", "checkbox-evidence")]
    [InlineData("radio-group", "radio-group-evidence")]
    [InlineData("slider", "slider-evidence")]
    [InlineData("switch", "switch-evidence")]
    [InlineData("toggle", "toggle-evidence")]
    [InlineData("toggle-group", "toggle-group-evidence")]
    public async Task NamedAccessibilityRulesCoverEveryActionState(string slug, string testId)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var violations = await page.GetByTestId(testId).EvaluateAsync<string[]>("""
            (root, slug) => {
                const violations = [];
                const named = element => {
                    const labelledBy = element.getAttribute('aria-labelledby');
                    const referenced = labelledBy?.split(/\s+/).map(id => document.getElementById(id));
                    if (referenced?.some(node => !node)) violations.push(`broken-labelledby:${labelledBy}`);
                    const labels = Array.from(element.labels || []).map(label => label.textContent?.trim()).filter(Boolean);
                    return element.getAttribute('aria-label')?.trim()
                        || referenced?.map(node => node?.textContent?.trim()).filter(Boolean).join(' ')
                        || labels.join(' ')
                        || element.textContent?.trim();
                };
                const controls = root.matches('button,input,a[href],[role=group],[role=radiogroup]') ? [root] : [...root.querySelectorAll('button,input,a[href],[role=group],[role=radiogroup]')];
                for (const element of controls) {
                    if (!named(element)) violations.push(`missing-name:${element.getAttribute('data-slot') || element.tagName}`);
                    const describedBy = element.getAttribute('aria-describedby');
                    if (describedBy && describedBy.split(/\s+/).some(id => !document.getElementById(id))) violations.push(`broken-describedby:${describedBy}`);
                    if (element.matches('input[type=checkbox]')) {
                        const expected = element.indeterminate ? 'mixed' : String(element.checked);
                        if (element.getAttribute('aria-checked') !== expected) violations.push(`checked-state:${expected}`);
                    }
                    if (element.matches('input[type=radio]') && element.getAttribute('aria-checked') !== String(element.checked)) violations.push('radio-state');
                    if (element.matches('input[type=range]')) {
                        for (const attribute of ['aria-valuemin', 'aria-valuemax', 'aria-valuenow']) if (!element.hasAttribute(attribute)) violations.push(`slider-${attribute}`);
                        if (+element.getAttribute('aria-valuenow') < +element.getAttribute('aria-valuemin') || +element.getAttribute('aria-valuenow') > +element.getAttribute('aria-valuemax')) violations.push('slider-range');
                    }
                    if (element.disabled && element.hasAttribute('tabindex') && element.tabIndex >= 0) violations.push('disabled-focusable');
                    if (element.hasAttribute('aria-invalid') && !['true', 'false'].includes(element.getAttribute('aria-invalid'))) violations.push('invalid-state-value');
                }
                for (const group of root.querySelectorAll('[data-slot=radio-group],[data-slot=toggle-group]')) {
                    const items = [...group.querySelectorAll('input:not(:disabled),button:not(:disabled)')];
                    if (items.filter(item => item.tabIndex === 0).length !== 1) violations.push('roving-tabstop');
                }
                const require = (condition, code) => { if (!condition) violations.push(code); };
                if (slug === 'button') {
                    require([...root.querySelectorAll('[data-slot=button]')].every(item => item.matches('button,a[href]')), 'button-role');
                    require(root.querySelector('[data-slot=button]:disabled'), 'button-disabled-state');
                    require(root.querySelector('[data-slot=button][aria-invalid=true]'), 'button-invalid-state');
                } else if (slug === 'button-group') {
                    require(root.querySelector('[data-slot=button-group][role=group]'), 'button-group-role');
                    require(root.querySelector('[data-slot=button-group][data-orientation=vertical]'), 'button-group-orientation');
                    require(root.querySelector('[data-slot=button-group] [data-slot=button-group]'), 'button-group-nested');
                    require(root.querySelector('[role=separator][aria-orientation]'), 'button-group-separator');
                } else if (slug === 'checkbox') {
                    require(root.querySelector('[data-slot=checkbox][aria-checked=mixed]'), 'checkbox-mixed');
                    require(root.querySelector('[data-slot=checkbox][aria-readonly=true]'), 'checkbox-readonly');
                    require(root.querySelector('[data-slot=checkbox][aria-invalid=true]'), 'checkbox-invalid');
                    require(root.querySelector('[data-slot=checkbox]:disabled'), 'checkbox-disabled');
                } else if (slug === 'radio-group') {
                    require([...root.querySelectorAll('[data-slot=radio-group]')].every(group => group.getAttribute('role') === 'radiogroup'), 'radiogroup-role');
                    require(root.querySelector('[data-slot=radio-group][data-orientation=vertical]'), 'radiogroup-vertical');
                    require(root.querySelector('[data-slot=radio-group][aria-invalid=true]'), 'radiogroup-invalid');
                    require(root.querySelector('[data-slot=radio-group-item][aria-readonly=true]'), 'radio-readonly');
                    require(root.querySelector('[data-slot=radio-group-item]:disabled'), 'radio-disabled');
                } else if (slug === 'slider') {
                    require(root.querySelector('[data-slot=slider][data-orientation=vertical]'), 'slider-vertical');
                    require(root.querySelector('[data-slot=slider-thumb][aria-readonly=true]'), 'slider-readonly');
                    require(root.querySelector('[data-slot=slider-thumb][aria-invalid=true]'), 'slider-invalid');
                    require(root.querySelector('[data-slot=slider-thumb]:disabled'), 'slider-disabled');
                } else if (slug === 'switch') {
                    require([...root.querySelectorAll('[data-slot=switch]')].every(control => control.getAttribute('role') === 'switch'), 'switch-role');
                    require(root.querySelector('[data-slot=switch][aria-readonly=true]'), 'switch-readonly');
                    require(root.querySelector('[data-slot=switch][aria-invalid=true]'), 'switch-invalid');
                    require(root.querySelector('[data-slot=switch]:disabled'), 'switch-disabled');
                } else if (slug === 'toggle') {
                    require([...root.querySelectorAll('[data-slot=toggle]')].every(control => ['true', 'false'].includes(control.getAttribute('aria-pressed'))), 'toggle-pressed');
                    require(root.querySelector('[data-slot=toggle][aria-invalid=true]'), 'toggle-invalid');
                    require(root.querySelector('[data-slot=toggle]:disabled'), 'toggle-disabled');
                } else if (slug === 'toggle-group') {
                    require(root.querySelector('[data-slot=toggle-group][role=group]'), 'toggle-group-role');
                    require(root.querySelector('[data-slot=toggle-group][data-orientation=vertical]'), 'toggle-group-vertical');
                    require(root.querySelector('[data-slot=toggle-group-item]:disabled'), 'toggle-group-disabled');
                }
                const ids = [...document.querySelectorAll('[id]')].map(element => element.id);
                if (new Set(ids).size !== ids.length) violations.push('duplicate-id');
                if (!root.querySelector('[data-evidence-state]') && !root.hasAttribute('data-evidence-state')) violations.push(`missing-state-fixture:${slug}`);
                return violations;
            }
            """, slug);
        Assert.True(violations.Length == 0, $"{slug}: {string.Join(", ", violations)}");
    }

    [Theory]
    [InlineData("button")]
    [InlineData("button-group")]
    [InlineData("checkbox")]
    [InlineData("radio-group")]
    [InlineData("slider")]
    [InlineData("switch")]
    [InlineData("toggle")]
    [InlineData("toggle-group")]
    public async Task PinnedVegaComputedStylesCoverEveryActionComponent(string slug)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();
        await Assertions.Expect(page.GetByTestId("button-default")).ToHaveCSSAsync("height", "36px");

        var metrics = await page.GetByTestId($"{slug}-evidence").EvaluateAsync<string[]>("""
            (root, slug) => {
                const style = element => getComputedStyle(element);
                const px = (element, property) => style(element).getPropertyValue(property);
                if (slug === 'button') {
                    const control = root.querySelector('[data-size=default]');
                    return [px(control, 'height'), px(control, 'padding-left'), px(root.querySelector('[aria-invalid=true]'), 'border-top-width')];
                }
                if (slug === 'button-group') {
                    const group = root.querySelector('[data-slot=button-group]');
                    const separator = root.querySelector('[role=separator]');
                    return [px(group, 'display'), px(group, 'flex-direction'), px(separator, 'width')];
                }
                if (slug === 'checkbox') {
                    const control = root.querySelector('[data-slot=checkbox]');
                    const checked = root.querySelector('[data-slot=checkbox]:checked');
                    const unchecked = root.querySelector('[data-slot=checkbox]:not(:checked):not([data-state=indeterminate])');
                    return [px(control, 'width'), px(control, 'height'), px(control, 'border-radius'), px(root.querySelector('[aria-invalid=true]'), 'border-top-width'), String(px(checked, 'background-color') !== px(unchecked, 'background-color'))];
                }
                if (slug === 'radio-group') {
                    const checked = root.querySelector('[data-slot=radio-group-item]:checked');
                    const unchecked = root.querySelector('[data-slot=radio-group-item]:not(:checked)');
                    return [px(checked, 'width'), px(checked, 'height'), px(checked, 'border-radius'), px(root.querySelector('[data-slot=radio-group-item][aria-invalid=true]'), 'border-top-width'), String(px(checked, 'background-color') !== px(unchecked, 'background-color'))];
                }
                if (slug === 'slider') {
                    const horizontal = root.querySelector('[data-orientation=horizontal]');
                    const vertical = root.querySelector('[data-orientation=vertical]');
                    const disabled = root.querySelector('[data-disabled=true]');
                    const invalid = root.querySelector('[data-invalid-fixture=true] input');
                    return [px(horizontal.querySelector('[data-slot=slider-track]'), 'height'), px(vertical.querySelector('[data-slot=slider-track]'), 'width'), px(vertical, 'height'), px(disabled, 'opacity'), String(px(horizontal.querySelector('[data-slot=slider-range]'), 'background-color') !== px(horizontal.querySelector('[data-slot=slider-track]'), 'background-color')), invalid.getAttribute('aria-invalid')];
                }
                if (slug === 'switch') {
                    const normal = root.querySelector('[data-size=default]');
                    const small = root.querySelector('[data-size=sm]');
                    const checked = root.querySelector('[data-slot=switch]:checked');
                    const unchecked = root.querySelector('[data-slot=switch]:not(:checked)');
                    const invalid = root.querySelector('[data-slot=switch][aria-invalid=true]');
                    return [px(normal, 'width'), px(normal, 'height'), px(small, 'width'), px(small, 'height'), String(px(checked, 'background-color') !== px(unchecked, 'background-color')), String(px(invalid, 'box-shadow') !== 'none')];
                }
                if (slug === 'toggle') {
                    const normal = root.querySelector('[data-size=default]');
                    const small = root.querySelector('[data-size=sm]');
                    const large = root.querySelector('[data-size=lg]');
                    const pressed = root.querySelector('[data-state=on]');
                    const off = root.querySelector('[data-state=off]');
                    return [px(normal, 'min-width'), px(normal, 'height'), px(small, 'height'), px(large, 'height'), String(px(pressed, 'background-color') !== px(off, 'background-color')), String(px(root.querySelector('[aria-invalid=true]'), 'box-shadow') !== 'none')];
                }
                const spaced = root.querySelector('[data-spacing="2"]');
                const connected = root.querySelector('[data-spacing="0"]');
                return [px(spaced, 'gap'), px(connected, 'gap'), px(root.querySelector('[data-orientation=vertical]'), 'flex-direction')];
            }
            """, slug);

        var expected = slug switch
        {
            "button" => new[] { "36px", "10px", "1px" },
            "button-group" => new[] { "flex", "row", "1px" },
            "checkbox" => new[] { "16px", "16px", "4px", "1px", "true" },
            "radio-group" => new[] { "16px", "16px", "50%", "1px", "true" },
            "slider" => new[] { "6px", "6px", "160px", "0.5", "true", "true" },
            "switch" => new[] { "32px", "18.3906px", "24px", "14px", "true", "true" },
            "toggle" => new[] { "36px", "36px", "32px", "40px", "true", "true" },
            "toggle-group" => new[] { "8px", "0px", "column" },
            _ => throw new InvalidOperationException(slug)
        };
        Assert.Equal(expected, metrics);
    }

    [Theory]
    [InlineData("button-evidence")]
    [InlineData("button-group-evidence")]
    [InlineData("checkbox-evidence")]
    [InlineData("radio-group-evidence")]
    [InlineData("slider-evidence")]
    [InlineData("switch-evidence")]
    [InlineData("toggle-evidence")]
    [InlineData("toggle-group-evidence")]
    public async Task NamedActionTextContrastMeetsWcagThreshold(string testId)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();

        var failures = await page.GetByTestId(testId).EvaluateAsync<string[]>("""
            root => {
                const parse = color => {
                    const canvas = document.createElement('canvas');
                    canvas.width = canvas.height = 1;
                    const probe = canvas.getContext('2d', { willReadFrequently: true });
                    probe.clearRect(0, 0, 1, 1);
                    probe.fillStyle = color;
                    probe.fillRect(0, 0, 1, 1);
                    const pixel = probe.getImageData(0, 0, 1, 1).data;
                    return [pixel[0], pixel[1], pixel[2], pixel[3] / 255];
                };
                const opaqueBackground = element => {
                    for (let node = element; node; node = node.parentElement) {
                        const color = parse(getComputedStyle(node).backgroundColor);
                        if (color[3] > 0.99) return color;
                    }
                    return [255, 255, 255, 1];
                };
                const luminance = color => {
                    const channels = color.slice(0, 3).map(value => {
                        const normalized = value / 255;
                        return normalized <= 0.04045 ? normalized / 12.92 : Math.pow((normalized + 0.055) / 1.055, 2.4);
                    });
                    return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722;
                };
                const contrast = (left, right) => {
                    const first = luminance(left), second = luminance(right);
                    return (Math.max(first, second) + 0.05) / (Math.min(first, second) + 0.05);
                };
                const candidates = [...root.querySelectorAll('button:not(:disabled), a[href], label, [data-slot=button-group-text], span[id$=label]')]
                    .filter(element => element.offsetParent && element.textContent?.trim() && !element.closest('[aria-disabled=true]'));
                return candidates.flatMap(element => {
                    const ratio = contrast(parse(getComputedStyle(element).color), opaqueBackground(element));
                    return ratio >= 4.5 ? [] : [`${element.textContent.trim().slice(0, 40)}:${ratio.toFixed(2)}`];
                });
            }
            """);
        Assert.True(failures.Length == 0, $"{testId}: {string.Join(", ", failures)}");
    }

    [Theory]
    [InlineData("button-evidence")]
    [InlineData("button-group-evidence")]
    [InlineData("checkbox-evidence")]
    [InlineData("radio-group-evidence")]
    [InlineData("slider-evidence")]
    [InlineData("switch-evidence")]
    [InlineData("toggle-evidence")]
    [InlineData("toggle-group-evidence")]
    public async Task NamedActionForcedColorStatesRemainVisible(string testId)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1100 },
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();
        Assert.True(await page.EvaluateAsync<bool>("matchMedia('(forced-colors: active)').matches"));

        var invisible = await page.GetByTestId(testId).EvaluateAsync<string[]>("""
            root => [...root.querySelectorAll('button,input:not([type=range]),a[href]')]
                .filter(control => !control.disabled && control.offsetParent)
                .flatMap(control => {
                    const style = getComputedStyle(control);
                    const visible = style.borderTopWidth !== '0px'
                        || style.backgroundColor !== 'rgba(255, 255, 255, 0)'
                        || style.color !== 'rgb(0, 0, 0)';
                    return visible ? [] : [control.getAttribute('data-slot') || control.tagName];
                })
            """);
        Assert.Empty(invisible);
    }

    [Fact]
    public async Task VisualComparatorRejectsSyntheticDifferentSizePngWithoutDiffOutput()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 100, Height = 100 },
            DeviceScaleFactor = 1
        });
        var page = await context.NewPageAsync();
        await page.SetContentAsync("<div id='expected' style='width:10px;height:10px;background:red'></div><div id='actual' style='width:11px;height:10px;background:red'></div>");
        var expected = await page.Locator("#expected").ScreenshotAsync();
        var actual = await page.Locator("#actual").ScreenshotAsync();
        var comparison = await ComparePngsAsync(page, expected, actual);

        Assert.NotEqual(comparison.ExpectedWidth, comparison.ActualWidth);
        Assert.False(IsVisualMatch(new VisualComparison
        {
            ExpectedWidth = comparison.ExpectedWidth,
            ExpectedHeight = comparison.ExpectedHeight,
            ActualWidth = comparison.ActualWidth,
            ActualHeight = comparison.ActualHeight,
            DifferentPixels = comparison.DifferentPixels,
            Ratio = comparison.Ratio,
            Diff = null
        }, 0.001));
    }

    [Fact]
    public async Task DeterministicComponentCropsMatchReviewedVisualBaselines()
    {
        const double mismatchThreshold = 0.001;
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Light
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(FamilyUrl("light", "ltr", "en"));
        await page.GetByTestId("actions-selection-fixture").WaitForAsync();
        await page.EvaluateAsync("document.fonts.ready");

        var crops = new Dictionary<string, ILocator>(StringComparer.Ordinal)
        {
            ["button"] = page.GetByTestId("button-evidence"),
            ["button-group"] = page.GetByTestId("button-group-evidence"),
            ["checkbox"] = page.GetByTestId("checkbox-evidence"),
            ["radio-group"] = page.GetByTestId("radio-group-evidence"),
            ["slider"] = page.GetByTestId("slider-evidence"),
            ["switch"] = page.GetByTestId("switch-evidence"),
            ["toggle"] = page.GetByTestId("toggle-evidence"),
            ["toggle-group"] = page.GetByTestId("toggle-group-evidence")
        };
        var baselineDirectory = Path.Combine(FindRoot(), "docs", "evidence", "actions-selection-baselines");
        var update = string.Equals(Environment.GetEnvironmentVariable("SHADCN_UPDATE_VISUAL_BASELINES"), "1", StringComparison.Ordinal);

        foreach (var (slug, locator) in crops)
        {
            if (slug == "toggle")
                await locator.EvaluateAsync("element => element.scrollIntoView({ block: 'center', inline: 'center' })");
            var actual = await locator.ScreenshotAsync(new() { Animations = ScreenshotAnimations.Disabled });
            var baselinePath = Path.Combine(baselineDirectory, $"{slug}.png");
            if (update)
            {
                Directory.CreateDirectory(baselineDirectory);
                await File.WriteAllBytesAsync(baselinePath, actual);
            }

            Assert.True(File.Exists(baselinePath), $"Missing reviewed visual baseline: {baselinePath}");
            var expected = await File.ReadAllBytesAsync(baselinePath);
            var comparison = await ComparePngsAsync(page, expected, actual);
            if (!IsVisualMatch(comparison, mismatchThreshold))
            {
                string? diffPath = null;
                if (comparison.Diff is not null)
                {
                    diffPath = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-{slug}-visual-diff.png");
                    await File.WriteAllBytesAsync(diffPath, Convert.FromBase64String(comparison.Diff));
                }
                Assert.Fail($"{slug} visual mismatch {comparison.DifferentPixels}/{comparison.CanvasWidth * comparison.CanvasHeight} pixels ({comparison.Ratio:P4}) exceeds {mismatchThreshold:P2}. Diff: {diffPath ?? "unavailable"}");
            }
        }
    }

    [Theory]
    [InlineData("button")]
    [InlineData("button-group")]
    [InlineData("checkbox")]
    [InlineData("radio-group")]
    [InlineData("slider")]
    [InlineData("switch")]
    [InlineData("toggle")]
    [InlineData("toggle-group")]
    public async Task CertifiedActionDossierHasRealPreviewApiCopyAccessibilityTokensAndReferences(string slug)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
            Permissions = ["clipboard-read", "clipboard-write"]
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{slug}").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("component-preview")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("component-api")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("api-row").First).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("component-token-guidance")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("component-reference")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".component-accessibility li")).Not.ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByTestId("planned-component-notice")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='true']")).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='false']")).ToHaveCountAsync(0);

        await page.Locator("#preview").GetByTestId("copy-source").ClickAsync();
        await Assertions.Expect(page.Locator("#preview .component-code__announcement")).ToHaveTextAsync("Source copied to clipboard.");
        Assert.Contains("<Shadcn", await page.EvaluateAsync<string>("navigator.clipboard.readText()"), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("button")]
    [InlineData("button-group")]
    [InlineData("checkbox")]
    [InlineData("radio-group")]
    [InlineData("slider")]
    [InlineData("switch")]
    [InlineData("toggle")]
    [InlineData("toggle-group")]
    public async Task EveryActionDossierControlChangesRenderedStateAndStyle(string slug)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1000 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{slug}").ToString());
        await page.GetByTestId("component-preview").WaitForAsync();

        switch (slug)
        {
            case "button":
                var variants = page.GetByTestId("button-dossier-preview").Locator("[data-testid^='button-variant-']");
                await Assertions.Expect(variants).ToHaveCountAsync(6);
                await Assertions.Expect(page.GetByTestId("button-variant-destructive")).ToHaveAttributeAsync("data-variant", "destructive");
                var sizes = page.GetByTestId("button-dossier-preview").Locator(".showcase-button-dossier__sizes [data-slot='button']");
                await Assertions.Expect(sizes).ToHaveCountAsync(4);
                await Assertions.Expect(sizes.Last).ToHaveCSSAsync("height", "40px");
                var iconSizes = page.GetByTestId("button-dossier-preview").Locator(".showcase-button-dossier__icon-sizes [data-slot='button']");
                await Assertions.Expect(iconSizes).ToHaveCountAsync(4);
                await page.GetByTestId("control-button-disabled").CheckAsync();
                await Assertions.Expect(variants).ToHaveCountAsync(6);
                await Assertions.Expect(page.GetByTestId("button-dossier-preview").Locator("button[data-slot='button']:disabled")).ToHaveCountAsync(13);
                await Assertions.Expect(page.GetByTestId("button-variant-link")).ToHaveAttributeAsync("aria-disabled", "true");
                break;
            case "button-group":
                await Assertions.Expect(page.GetByTestId("action-button-group").Locator("[data-slot=button-group-text]")).ToHaveCountAsync(1);
                await Assertions.Expect(page.GetByTestId("action-button-group").Locator("[data-slot=button-group-separator]")).ToHaveCountAsync(1);
                await Assertions.Expect(page.GetByTestId("action-button-group").Locator("[data-slot=button]")).ToHaveCountAsync(3);
                await page.GetByTestId("button-group-archive").ClickAsync();
                await Assertions.Expect(page.GetByTestId("button-group-last-action")).ToContainTextAsync("Quotation archived");
                await page.GetByTestId("control-button-group-orientation").SelectOptionAsync("Vertical");
                await Assertions.Expect(page.GetByTestId("action-button-group")).ToHaveAttributeAsync("data-orientation", "vertical");
                await Assertions.Expect(page.GetByTestId("action-button-group")).ToHaveCSSAsync("flex-direction", "column");
                await Assertions.Expect(page.Locator("#preview .shadcn-code-block pre").First).ToContainTextAsync("ShadcnButtonGroupOrientation.Vertical");
                break;
            case "checkbox":
                await Assertions.Expect(page.GetByTestId("checkbox-dossier-preview").Locator("[data-slot=checkbox]")).ToHaveCountAsync(6);
                await Assertions.Expect(page.GetByTestId("action-checkbox")).ToHaveAttributeAsync("aria-checked", "false");
                await page.GetByTestId("action-checkbox").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-checkbox")).ToHaveAttributeAsync("aria-checked", "true");
                await Assertions.Expect(page.GetByTestId("checkbox-indeterminate")).ToHaveAttributeAsync("aria-checked", "mixed");
                await Assertions.Expect(page.GetByTestId("checkbox-disabled")).ToBeDisabledAsync();
                await Assertions.Expect(page.GetByTestId("checkbox-readonly")).ToHaveAttributeAsync("aria-readonly", "true");
                await Assertions.Expect(page.GetByTestId("checkbox-invalid")).ToHaveAttributeAsync("aria-invalid", "true");
                break;
            case "radio-group":
                await page.GetByTestId("control-radio-orientation").SelectOptionAsync("Horizontal");
                await Assertions.Expect(page.GetByTestId("action-radio-group")).ToHaveCSSAsync("display", "flex");
                var dossierRadios = page.GetByTestId("action-radio-group").Locator("input[type='radio']");
                await dossierRadios.First.CheckAsync();
                await Assertions.Expect(dossierRadios.First).ToBeCheckedAsync();
                await Assertions.Expect(page.GetByTestId("radio-group-dossier-preview").GetByRole(AriaRole.Status)).ToContainTextAsync("Standard review selected");
                var centers = await page.GetByTestId("action-radio-group").Locator("[data-slot='radio-group-control']").First.EvaluateAsync<double[]>("""
                    control => {
                        const input = control.querySelector('[data-slot=radio-group-item]').getBoundingClientRect();
                        const indicator = control.querySelector('[data-slot=radio-group-indicator]').getBoundingClientRect();
                        return [input.x + input.width / 2, input.y + input.height / 2, indicator.x + indicator.width / 2, indicator.y + indicator.height / 2];
                    }
                    """);
                Assert.InRange(Math.Abs(centers[0] - centers[2]), 0, .5);
                Assert.InRange(Math.Abs(centers[1] - centers[3]), 0, .5);
                await page.GetByTestId("radio-group-dossier-preview").EvaluateAsync("element => element.setAttribute('dir', 'rtl')");
                var rtlCenters = await page.GetByTestId("action-radio-group").Locator("[data-slot='radio-group-control']").First.EvaluateAsync<double[]>("""
                    control => {
                        const input = control.querySelector('[data-slot=radio-group-item]').getBoundingClientRect();
                        const indicator = control.querySelector('[data-slot=radio-group-indicator]').getBoundingClientRect();
                        return [input.x + input.width / 2, input.y + input.height / 2, indicator.x + indicator.width / 2, indicator.y + indicator.height / 2];
                    }
                    """);
                Assert.InRange(Math.Abs(rtlCenters[0] - rtlCenters[2]), 0, .5);
                Assert.InRange(Math.Abs(rtlCenters[1] - rtlCenters[3]), 0, .5);
                await page.GetByTestId("control-radio-disabled").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-radio-group").Locator("input").First).ToBeDisabledAsync();
                await page.GetByTestId("control-radio-disabled").UncheckAsync();
                await page.GetByTestId("control-radio-readonly").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-radio-group").Locator("input").First).ToHaveAttributeAsync("aria-readonly", "true");
                await page.GetByTestId("control-radio-invalid").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-radio-group")).ToHaveAttributeAsync("aria-invalid", "true");
                await Assertions.Expect(page.GetByTestId("action-radio-group").Locator("input:disabled")).ToHaveCountAsync(1);
                break;
            case "slider":
                await page.GetByTestId("control-slider-values").SelectOptionAsync("Single");
                var actionSlider = page.GetByTestId("action-slider");
                await Assertions.Expect(actionSlider.Locator("input[type=range]")).ToHaveCountAsync(1);
                var trackBox = await actionSlider.Locator("[data-slot='slider-track']").BoundingBoxAsync();
                Assert.NotNull(trackBox);
                await page.Mouse.MoveAsync((float)(trackBox!.X + trackBox.Width * .25), (float)(trackBox.Y + trackBox.Height / 2));
                await page.Mouse.DownAsync();
                await page.Mouse.MoveAsync((float)(trackBox.X + trackBox.Width * .8), (float)(trackBox.Y + trackBox.Height / 2));
                await page.Mouse.UpAsync();
                Assert.InRange(int.Parse(await actionSlider.Locator("input[type=range]").First.GetAttributeAsync("aria-valuenow") ?? "0"), 75, 85);
                await page.GetByTestId("control-slider-values").SelectOptionAsync("Multiple");
                await Assertions.Expect(actionSlider.Locator("input[type=range]")).ToHaveCountAsync(3);
                await page.GetByTestId("control-slider-orientation").SelectOptionAsync("Vertical");
                await Assertions.Expect(actionSlider).ToHaveCSSAsync("height", "160px");
                await page.GetByTestId("control-slider-disabled").CheckAsync();
                await Assertions.Expect(actionSlider.Locator("input").First).ToBeDisabledAsync();
                await page.GetByTestId("control-slider-disabled").UncheckAsync();
                await page.GetByTestId("control-slider-readonly").CheckAsync();
                await Assertions.Expect(actionSlider.Locator("input").First).ToHaveAttributeAsync("aria-readonly", "true");
                await page.GetByTestId("control-slider-invalid").CheckAsync();
                await Assertions.Expect(actionSlider.Locator("input").First).ToHaveAttributeAsync("aria-invalid", "true");
                await Assertions.Expect(actionSlider.Locator("input").First).ToHaveAttributeAsync("name", "budget");
                await Assertions.Expect(actionSlider.Locator("input").First).ToHaveAttributeAsync("form", "dossier-slider-form");
                break;
            case "switch":
                await page.GetByTestId("control-switch-value").UncheckAsync();
                await Assertions.Expect(page.GetByTestId("action-switch")).ToHaveAttributeAsync("aria-checked", "false");
                await page.GetByTestId("control-switch-size").SelectOptionAsync("Small");
                await Assertions.Expect(page.GetByTestId("action-switch").Locator("xpath=..")).ToHaveCSSAsync("width", "24px");
                await page.GetByTestId("control-switch-disabled").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-switch")).ToBeDisabledAsync();
                await page.GetByTestId("control-switch-disabled").UncheckAsync();
                await page.GetByTestId("control-switch-readonly").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-switch")).ToHaveAttributeAsync("aria-readonly", "true");
                await page.GetByTestId("control-switch-invalid").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-switch")).ToHaveAttributeAsync("aria-invalid", "true");
                await Assertions.Expect(page.GetByTestId("action-switch")).ToHaveAttributeAsync("name", "production-updates");
                break;
            case "toggle":
                await Assertions.Expect(page.GetByTestId("control-toggle-pressed")).ToHaveCountAsync(0);
                await Assertions.Expect(page.GetByTestId("toggle-dossier-preview")).ToBeVisibleAsync();
                await Assertions.Expect(page.GetByTestId("toggle-format-state")).ToHaveTextAsync("Bold enabled");
                await page.GetByTestId("action-toggle").ClickAsync();
                await Assertions.Expect(page.GetByTestId("action-toggle")).ToHaveAttributeAsync("aria-pressed", "false");
                await Assertions.Expect(page.GetByTestId("toggle-format-state")).ToHaveTextAsync("Bold disabled");
                await page.GetByTestId("action-toggle").ClickAsync();
                await Assertions.Expect(page.GetByTestId("action-toggle")).ToHaveAttributeAsync("aria-pressed", "true");
                await page.GetByTestId("control-toggle-variant").SelectOptionAsync("Default");
                await Assertions.Expect(page.GetByTestId("action-toggle")).ToHaveAttributeAsync("data-variant", "default");
                await page.GetByTestId("control-toggle-size").SelectOptionAsync("Large");
                await Assertions.Expect(page.GetByTestId("action-toggle")).ToHaveCSSAsync("height", "40px");
                await page.GetByTestId("control-toggle-disabled").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-toggle")).ToBeDisabledAsync();
                await page.GetByTestId("control-toggle-invalid").CheckAsync();
                await Assertions.Expect(page.GetByTestId("action-toggle")).ToHaveAttributeAsync("aria-invalid", "true");
                break;
            case "toggle-group":
                await page.GetByTestId("control-toggle-group-multiple").UncheckAsync();
                await Assertions.Expect(page.GetByTestId("action-toggle-group")).ToHaveAttributeAsync("data-fixture-multiple", "false");
                await page.GetByTestId("control-toggle-group-orientation").SelectOptionAsync("Vertical");
                await Assertions.Expect(page.GetByTestId("action-toggle-group")).ToHaveCSSAsync("flex-direction", "column");
                await page.GetByTestId("control-toggle-group-spacing").SelectOptionAsync("0");
                await Assertions.Expect(page.GetByTestId("action-toggle-group")).ToHaveCSSAsync("gap", "0px");
                await page.GetByTestId("control-toggle-group-variant").SelectOptionAsync("Default");
                await Assertions.Expect(page.GetByTestId("action-toggle-group")).ToHaveAttributeAsync("data-variant", "default");
                await page.GetByTestId("control-toggle-group-size").SelectOptionAsync("Large");
                await Assertions.Expect(page.GetByTestId("action-toggle-group").Locator("button").First).ToHaveCSSAsync("height", "40px");
                await Assertions.Expect(page.GetByTestId("action-toggle-group").Locator("button:disabled")).ToHaveCountAsync(1);
                break;
        }
    }

    [Fact]
    public async Task SliderPointerDragContinuesAfterTheControlledPreviewRerenders()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 1000 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/slider").ToString());
        await page.GetByTestId("component-preview").WaitForAsync();
        await page.GetByTestId("control-slider-values").SelectOptionAsync("Single");

        var slider = page.GetByTestId("action-slider");
        var thumb = slider.Locator("input[type=range]").First;
        var trackBox = await slider.Locator("[data-slot='slider-track']").BoundingBoxAsync();
        Assert.NotNull(trackBox);

        await page.Mouse.MoveAsync((float)(trackBox!.X + trackBox.Width * .2), (float)(trackBox.Y + trackBox.Height / 2));
        await page.Mouse.DownAsync();
        await Assertions.Expect(thumb).ToHaveAttributeAsync("aria-valuenow", "20");

        await page.Mouse.MoveAsync((float)(trackBox.X + trackBox.Width * .8), (float)(trackBox.Y + trackBox.Height / 2), new() { Steps = 4 });
        await page.Mouse.UpAsync();

        Assert.InRange(int.Parse(await thumb.GetAttributeAsync("aria-valuenow") ?? "0"), 75, 85);

        await thumb.FillAsync("65");
        await Assertions.Expect(thumb).ToHaveAttributeAsync("aria-valuenow", "65");
        await page.GetByTestId("control-slider-readonly").CheckAsync();
        await page.Mouse.MoveAsync((float)(trackBox.X + trackBox.Width * .1), (float)(trackBox.Y + trackBox.Height / 2));
        await page.Mouse.DownAsync();
        await page.Mouse.UpAsync();
        await Assertions.Expect(thumb).ToHaveValueAsync("65");

        await page.GetByTestId("control-slider-readonly").UncheckAsync();
        await page.GetByTestId("control-slider-orientation").SelectOptionAsync("Vertical");
        trackBox = await slider.Locator("[data-slot='slider-track']").BoundingBoxAsync();
        Assert.NotNull(trackBox);
        await page.Mouse.MoveAsync((float)(trackBox!.X + trackBox.Width / 2), (float)(trackBox.Y + trackBox.Height * .2));
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync((float)(trackBox.X + trackBox.Width / 2), (float)(trackBox.Y + trackBox.Height * .8), new() { Steps = 4 });
        await page.Mouse.UpAsync();
        Assert.InRange(int.Parse(await thumb.GetAttributeAsync("aria-valuenow") ?? "0"), 15, 25);

        await page.GetByTestId("control-slider-orientation").SelectOptionAsync("Horizontal");
        await slider.EvaluateAsync("element => element.setAttribute('dir', 'rtl')");
        trackBox = await slider.Locator("[data-slot='slider-track']").BoundingBoxAsync();
        Assert.NotNull(trackBox);
        await page.Mouse.ClickAsync((float)(trackBox!.X + trackBox.Width * .2), (float)(trackBox.Y + trackBox.Height / 2));
        Assert.InRange(int.Parse(await thumb.GetAttributeAsync("aria-valuenow") ?? "0"), 75, 85);
    }

    [Fact]
    public async Task SwitchDossierSupportsDirectInteractionAndKeepsSourceInSync()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/switch").ToString());
        await page.GetByTestId("component-preview").WaitForAsync();

        var control = page.GetByTestId("action-switch");
        await control.ClickAsync();
        await Assertions.Expect(control).ToHaveAttributeAsync("aria-checked", "false");
        await Assertions.Expect(page.GetByTestId("switch-dossier-preview").GetByRole(AriaRole.Status))
            .ToContainTextAsync("Production updates are paused.");

        await page.GetByTestId("control-switch-size").SelectOptionAsync("Small");
        await Assertions.Expect(control.Locator("xpath=..")).ToHaveCSSAsync("width", "24px");
        await page.GetByTestId("control-switch-invalid").CheckAsync();
        var source = page.Locator("#preview .component-code pre").First;
        await Assertions.Expect(source).ToContainTextAsync("Size=\"ShadcnSwitchSize.Small\"");
        await Assertions.Expect(source).ToContainTextAsync("Invalid=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("@bind-Value=\"ProductionUpdates\"");

        await control.ClickAsync();
        await page.GetByTestId("documentation-direction-toggle").EvaluateAsync("element => element.click()");
        await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("dir", "rtl");
        var geometry = await control.Locator("xpath=..").EvaluateAsync<double[]>("""
            root => {
                const track = root.getBoundingClientRect();
                const thumb = root.querySelector('[data-slot=switch-thumb]').getBoundingClientRect();
                return [track.left, track.right, thumb.left, thumb.right, thumb.left - track.left, track.right - thumb.right];
            }
            """);
        Assert.True(geometry[2] >= geometry[0], "RTL checked thumb must remain inside the track's left edge.");
        Assert.True(geometry[3] <= geometry[1], "RTL checked thumb must remain inside the track's right edge.");
        Assert.InRange(geometry[4], 0, 2);

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
    }

    private string FamilyUrl(string theme, string direction, string locale) =>
        new Uri(server.BaseUri, $"/components/actions-and-selection?theme={theme}&dir={direction}&locale={locale}").ToString();

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate solution root.");
    }

    private static bool IsVisualMatch(VisualComparison comparison, double threshold) =>
        comparison.ExpectedWidth == comparison.ActualWidth &&
        comparison.ExpectedHeight == comparison.ActualHeight &&
        comparison.Ratio <= threshold;

    private static Task<VisualComparison> ComparePngsAsync(IPage page, byte[] expected, byte[] actual) =>
        page.EvaluateAsync<VisualComparison>("""
            async ({ expected, actual }) => {
                const decode = async value => createImageBitmap(await (await fetch(`data:image/png;base64,${value}`)).blob());
                const expectedImage = await decode(expected);
                const actualImage = await decode(actual);
                const canvas = document.createElement('canvas');
                canvas.width = Math.max(expectedImage.width, actualImage.width);
                canvas.height = Math.max(expectedImage.height, actualImage.height);
                const context = canvas.getContext('2d', { willReadFrequently: true });
                context.clearRect(0, 0, canvas.width, canvas.height);
                context.drawImage(expectedImage, 0, 0);
                const expectedPixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                context.clearRect(0, 0, canvas.width, canvas.height);
                context.drawImage(actualImage, 0, 0);
                const actualPixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
                const diff = context.createImageData(canvas.width, canvas.height);
                let differentPixels = 0;
                for (let offset = 0; offset < actualPixels.length; offset += 4) {
                    const delta = Math.max(
                        Math.abs(expectedPixels[offset] - actualPixels[offset]),
                        Math.abs(expectedPixels[offset + 1] - actualPixels[offset + 1]),
                        Math.abs(expectedPixels[offset + 2] - actualPixels[offset + 2]),
                        Math.abs(expectedPixels[offset + 3] - actualPixels[offset + 3]));
                    if (delta > 8) {
                        differentPixels++;
                        diff.data[offset] = 255;
                        diff.data[offset + 3] = 255;
                    }
                }
                context.putImageData(diff, 0, 0);
                return {
                    expectedWidth: expectedImage.width,
                    expectedHeight: expectedImage.height,
                    actualWidth: actualImage.width,
                    actualHeight: actualImage.height,
                    canvasWidth: canvas.width,
                    canvasHeight: canvas.height,
                    differentPixels,
                    ratio: differentPixels / (canvas.width * canvas.height),
                    diff: canvas.toDataURL('image/png').split(',')[1]
                };
            }
            """, new
        {
            expected = Convert.ToBase64String(expected),
            actual = Convert.ToBase64String(actual)
        });

    private sealed class VisualComparison
    {
        public int ExpectedWidth { get; set; }
        public int ExpectedHeight { get; set; }
        public int ActualWidth { get; set; }
        public int ActualHeight { get; set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }
        public int DifferentPixels { get; set; }
        public double Ratio { get; set; }
        public string? Diff { get; set; }
    }
}
