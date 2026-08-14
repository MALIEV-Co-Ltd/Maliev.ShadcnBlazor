using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class FeedbackContentBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task EveryFeedbackContentComponentPassesNamedAccessibilityRulesInLocalizedRtlState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content?theme=dark&dir=rtl").ToString());
        await page.GetByTestId("feedback-content-fixture").WaitForAsync();

        Assert.Equal(9, await page.Locator("[data-component]").CountAsync());
        await Assertions.Expect(page.Locator("[data-component='alert'] [data-slot='alert']")).ToHaveAttributeAsync("role", "alert");
        await Assertions.Expect(page.Locator("[data-component='avatar'] img")).ToHaveAttributeAsync("alt", "Thai operator");
        Assert.Equal("span", await page.Locator("[data-component='badge'] [data-slot='badge']").EvaluateAsync<string>("element => element.localName"));
        await Assertions.Expect(page.Locator("[data-component='carousel'] [data-slot='carousel']")).ToHaveAttributeAsync("aria-label", "Production jobs");
        Assert.Equal(3, await page.Locator("[data-component='carousel'] [role='group'][aria-roledescription='slide']").CountAsync());
        await Assertions.Expect(page.Locator("[data-component='progress'] [role='progressbar']")).ToHaveAttributeAsync("aria-valuenow", "64");
        await Assertions.Expect(page.Locator("[data-component='skeleton'] [data-slot='skeleton']")).ToHaveAttributeAsync("aria-hidden", "true");
        await Assertions.Expect(page.Locator("[data-component='spinner'] [role='status']")).ToHaveAttributeAsync("aria-label", "กำลังโหลด");
        await page.GetByRole(AriaRole.Button, new() { Name = "Show toast" }).DispatchEventAsync("click");
        await Assertions.Expect(page.Locator("[data-component='toast'] [role='status']")).ToContainTextAsync("บันทึกแล้ว");

        var unnamed = await page.Locator("[data-testid='feedback-content-fixture'] button:not([aria-label])").EvaluateAllAsync<string[]>("elements => elements.filter(element => !(element.textContent || '').trim()).map(element => element.outerHTML)");
        Assert.Empty(unnamed);
    }

    [Fact]
    public async Task EveryFeedbackContentStateMatchesPinnedCorrespondingSlotMetrics()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content").ToString());
        await page.GetByTestId("feedback-content-fixture").WaitForAsync();

        static async Task<string> Css(ILocator locator, string property) => await locator.EvaluateAsync<string>("(element, property) => getComputedStyle(element).getPropertyValue(property)", property);
        Assert.Equal("grid", await Css(page.Locator("[data-component='alert'] [data-slot='alert']"), "display"));
        Assert.Equal("16px", await Css(page.Locator("[data-component='alert'] [data-slot='alert']"), "padding-left"));
        Assert.Equal("32px", await Css(page.Locator("[data-component='avatar'] [data-slot='avatar']"), "width"));
        Assert.Equal("20px", await Css(page.Locator("[data-component='badge'] [data-slot='badge']"), "height"));
        Assert.Equal("14px", await Css(page.Locator("[data-component='card'] [data-slot='card']"), "border-radius"));
        Assert.Equal("6px", await Css(page.Locator("[data-component='progress'] [data-slot='progress-track']"), "height"));
        Assert.Equal("8px", await Css(page.Locator("[data-component='skeleton'] [data-slot='skeleton']"), "border-radius"));
        Assert.Equal("16px", await Css(page.Locator("[data-component='spinner'] [data-slot='spinner']"), "width"));
        await page.GetByRole(AriaRole.Button, new() { Name = "Show toast" }).ClickAsync();
        Assert.Equal("18px", await Css(page.Locator("[data-component='toast'] [data-slot='toast']"), "border-radius"));
        await Assertions.Expect(page.Locator("[data-component='carousel'] [data-slot='carousel-track']")).ToHaveAttributeAsync("data-measured", "true");
        Assert.Equal("flex", await Css(page.Locator("[data-component='carousel'] [data-slot='carousel-track']"), "display"));
        Assert.Equal("16px", await Css(page.Locator("[data-component='carousel'] [data-slot='carousel-item']").First, "padding-left"));
        Assert.Equal("0 0 100%", await Css(page.Locator("[data-component='carousel'] [data-slot='carousel-item']").First, "flex"));
        Assert.Equal("32px", await Css(page.Locator("[data-component='carousel'] [data-slot='carousel-next']"), "width"));
        Assert.Equal("pan-y pinch-zoom", await Css(page.Locator("[data-component='carousel'] [data-slot='carousel-content']"), "touch-action"));
        var announcement = page.Locator("[data-component='carousel'] [data-slot='carousel-announcement']");
        await Assertions.Expect(announcement).ToHaveTextAsync("Slide 1 of 3");
        Assert.Equal("1px", await Css(announcement, "width"));
        Assert.Equal("1px", await Css(announcement, "height"));
        Assert.Equal("rect(0px, 0px, 0px, 0px)", await Css(announcement, "clip"));
        await page.Locator("[data-testid='feedback-content-pairs']").EvaluateAsync("element => element.style.display = 'grid'");
        var carouselBox = await page.Locator("[data-pair-id='carousel']").BoundingBoxAsync();
        var previousBox = await page.Locator("[data-pair-id='carousel'] [data-slot='carousel-previous']").BoundingBoxAsync();
        var nextBox = await page.Locator("[data-pair-id='carousel'] [data-slot='carousel-next']").BoundingBoxAsync();
        Assert.NotNull(carouselBox); Assert.NotNull(previousBox); Assert.NotNull(nextBox);
        Assert.True(previousBox.X >= carouselBox.X && nextBox.X + nextBox.Width <= carouselBox.X + carouselBox.Width);
    }

    [Fact]
    public async Task FeedbackContentKeyboardOrderAndContrastRemainUsableAtTwoHundredPercentZoom()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content?theme=dark&dir=rtl").ToString());
        await page.GetByTestId("feedback-content-fixture").WaitForAsync();
        await page.EvaluateAsync("document.documentElement.style.zoom = '2'");
        Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= document.documentElement.clientWidth + 1"));

        var carousel = page.Locator("[data-component='carousel'] [data-slot='carousel']");
        await carousel.Locator("[data-slot='carousel-next']").FocusAsync();
        await Assertions.Expect(carousel.Locator("[data-slot='carousel-next']")).ToBeFocusedAsync();
        Assert.True(await page.EvaluateAsync<bool>("""() => { const next=document.querySelector('[data-component="carousel"] [data-slot="carousel-next"]'), toast=document.querySelector('[data-component="toast"] button'); return Boolean(next.compareDocumentPosition(toast) & Node.DOCUMENT_POSITION_FOLLOWING); }"""));

        await page.GetByRole(AriaRole.Button, new() { Name = "Show toast" }).ClickAsync();
        await page.Keyboard.PressAsync("F6");
        await Assertions.Expect(page.Locator("[data-slot='toast-viewport']")).ToBeFocusedAsync();

        foreach (var selector in new[] { "[data-component='alert'] [data-slot='alert']", "[data-component='badge'] [data-slot='badge']", "[data-component='card'] [data-slot='card']", "[data-slot='toast']" })
        {
            var ratio = await page.Locator(selector).First.EvaluateAsync<double>("""
                element => {
                  const parse = value => { const canvas=document.createElement('canvas'), probe=canvas.getContext('2d',{willReadFrequently:true}); canvas.width=canvas.height=1; probe.fillStyle=value; probe.fillRect(0,0,1,1); return [...probe.getImageData(0,0,1,1).data].slice(0,3).map(channel => { channel /= 255; return channel <= .04045 ? channel / 12.92 : Math.pow((channel + .055) / 1.055, 2.4); }); };
                  const style = getComputedStyle(element), foreground = parse(style.color); let node=element, background; while(node && !background){const value=getComputedStyle(node).backgroundColor;if(value !== 'rgba(0, 0, 0, 0)') background=parse(value);node=node.parentElement;} background ??= parse('white');
                  const luminance = rgb => .2126 * rgb[0] + .7152 * rgb[1] + .0722 * rgb[2];
                  const a = luminance(foreground), b = luminance(background); return (Math.max(a,b)+.05)/(Math.min(a,b)+.05);
                }
                """);
            Assert.True(ratio >= 3, $"{selector} contrast was {ratio:F2}:1.");
        }
    }

    [Fact]
    public async Task FeedbackContentForcedColorsAndReducedMotionPreserveVisibleBoundariesAndSuppressAnimation()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 900, Height = 720 },
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content").ToString());
        await page.GetByTestId("feedback-content-fixture").WaitForAsync();

        foreach (var selector in new[] { "[data-component='alert'] [data-slot='alert']", "[data-component='card'] [data-slot='card']", "[data-component='progress'] [data-slot='progress-track']", "[data-component='skeleton'] [data-slot='skeleton']" })
        {
            Assert.Equal("1px", await page.Locator(selector).EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        }

        Assert.True(await page.Locator("[data-component='skeleton'] [data-slot='skeleton']").EvaluateAsync<bool>("element => parseFloat(getComputedStyle(element).animationDuration) <= .001"));
        Assert.True(await page.Locator("[data-component='spinner'] [data-slot='spinner']").EvaluateAsync<bool>("element => parseFloat(getComputedStyle(element).animationDuration) <= .001"));
        Assert.Equal("none", await page.Locator("[data-component='carousel'] [data-slot='carousel-track']").EvaluateAsync<string>("element => getComputedStyle(element).transitionProperty"));
    }

    [Fact]
    public async Task CarouselMeasuresAndMovesRealTrackAfterButtonAndPointerInput()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 900, Height = 720 },
            ReducedMotion = ReducedMotion.Reduce,
            HasTouch = true
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content?theme=dark&dir=ltr").ToString());
        await page.GetByTestId("feedback-content-fixture").WaitForAsync();

        var carousel = page.Locator("[data-component='carousel'] [data-slot='carousel']");
        var track = carousel.Locator("[data-slot='carousel-track']");
        await Assertions.Expect(track).ToHaveAttributeAsync("data-measured", "true");
        await carousel.Locator("[data-slot='carousel-next']").ClickAsync();
        await Assertions.Expect(carousel.Locator("[data-slot='carousel-item']").Nth(1)).ToHaveAttributeAsync("data-selected", "true");
        var afterButton = await track.EvaluateAsync<string>("element => getComputedStyle(element).translate");
        Assert.StartsWith("-", afterButton, StringComparison.Ordinal);

        var viewport = carousel.Locator("[data-slot='carousel-content']");
        await viewport.EvaluateAsync("element => { const box = element.getBoundingClientRect(); element.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true, pointerId: 7, clientX: box.left + box.width * .75, clientY: box.top + 1 })); element.dispatchEvent(new PointerEvent('pointerup', { bubbles: true, pointerId: 7, clientX: box.left + box.width * .25, clientY: box.top + 1 })); }");
        await Assertions.Expect(carousel.Locator("[data-slot='carousel-item']").Nth(2)).ToHaveAttributeAsync("data-selected", "true");

        await page.SetViewportSizeAsync(640, 720);
        await Assertions.Expect(track).ToHaveAttributeAsync("data-measured", "true");
    }

    [Fact]
    public async Task ToastSupportsGlobalF6HoverExpansionAndPhysicalSwipe()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, HasTouch = true });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content?theme=dark&dir=rtl").ToString());
        await page.GetByRole(AriaRole.Button, new() { Name = "Show toast" }).ClickAsync();
        var viewport = page.Locator("[data-slot='toast-viewport']");
        var toast = viewport.Locator("[data-slot='toast']");
        await Assertions.Expect(toast).ToHaveCountAsync(1);

        await page.Keyboard.PressAsync("F6");
        await Assertions.Expect(viewport).ToBeFocusedAsync();
        await toast.HoverAsync();
        await Assertions.Expect(viewport).ToHaveAttributeAsync("data-expanded", "true");

        await toast.EvaluateAsync("element => { const box = element.getBoundingClientRect(); element.dispatchEvent(new PointerEvent('pointerdown', { bubbles: true, pointerId: 12, clientX: box.left + 10, clientY: box.top + 10 })); element.dispatchEvent(new PointerEvent('pointermove', { bubbles: true, pointerId: 12, clientX: box.left + 100, clientY: box.top + 10 })); }");
        await Assertions.Expect(toast).ToHaveAttributeAsync("data-swipe", "move");
        await toast.EvaluateAsync("element => { const box = element.getBoundingClientRect(); element.dispatchEvent(new PointerEvent('pointerup', { bubbles: true, pointerId: 12, clientX: box.left + 100, clientY: box.top + 10 })); }");
        await Assertions.Expect(toast).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task BadgeLinkFocusStateHasNativeSemanticsAndPinnedVisibleRing()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 900, Height = 720 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content").ToString());
        await page.Locator("[data-testid='feedback-content-pairs']").EvaluateAsync("element => element.style.display = 'grid'");
        var badge = page.Locator("[data-pair-id='badge-outline'] [data-slot='badge']");
        Assert.Equal("a", await badge.EvaluateAsync<string>("element => element.localName"));
        await Assertions.Expect(badge).ToHaveAttributeAsync("href", "#badge");
        await page.EvaluateAsync("document.body.focus()");
        for (var attempt = 0; attempt < 30 && !await badge.EvaluateAsync<bool>("element => document.activeElement === element"); attempt++)
            await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(badge).ToBeFocusedAsync();
        Assert.NotEqual("none", await badge.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
        Assert.NotEqual("0px", await badge.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.NotEqual("rgba(0, 0, 0, 0)", await badge.EvaluateAsync<string>("element => getComputedStyle(element).borderColor"));
    }

    [Fact]
    public async Task EveryFeedbackComponentHasExactVariantThemeDirectionAndStateMetrics()
    {
        var themeColors = new Dictionary<string, (string Alert, string Skeleton, string Toast)>();
        foreach (var (theme, direction) in new[] { ("light", "ltr"), ("dark", "rtl") })
        {
            await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 } });
            var page = await context.NewPageAsync();
            await page.GotoAsync(new Uri(server.BaseUri, $"/components/feedback-and-content?theme={theme}&dir={direction}").ToString());
            await page.GetByTestId("feedback-content-fixture").WaitForAsync();
            await page.EvaluateAsync("({theme,direction}) => { const scope=document.querySelector('.shadcn-scope'); scope?.setAttribute('data-shadcn-theme', theme); scope?.setAttribute('dir', direction); document.documentElement.dir=direction; }", new { theme, direction });
            await page.Locator("[data-testid='feedback-content-pairs']").EvaluateAsync("element => element.style.display = 'grid'");

            static Task<string> Css(ILocator locator, string property) => locator.EvaluateAsync<string>("(element, property) => getComputedStyle(element).getPropertyValue(property)", property);
            var alert = page.Locator("[data-pair-id='alert-destructive'] [data-slot='alert']");
            await Assertions.Expect(alert).ToHaveAttributeAsync("data-variant", "destructive");
            var alertColors = await alert.EvaluateAsync<string[]>("""element => { const probe=document.createElement('span'); probe.style.color='var(--shadcn-border)'; element.append(probe); const values=[getComputedStyle(element).borderTopColor,getComputedStyle(probe).color]; probe.remove(); return values; }""");
            Assert.Equal(alertColors[1], alertColors[0]);

            var loadedImage = page.Locator("[data-pair-id='avatar-loaded'] [data-slot='avatar-image']");
            await Assertions.Expect(loadedImage).ToHaveAttributeAsync("data-state", "loaded");
            Assert.Equal("visible", await Css(loadedImage, "visibility"));
            var failedImage = page.Locator("[data-pair-id='avatar-error'] [data-slot='avatar-image']");
            await Assertions.Expect(page.Locator("[data-pair-id='avatar-error'] [data-slot='avatar-fallback']")).ToHaveAttributeAsync("data-state", "visible");
            Assert.Equal("hidden", await Css(failedImage, "visibility"));

            var badge = page.Locator("[data-pair-id='badge-outline'] [data-slot='badge']");
            await badge.FocusAsync();
            Assert.NotEqual("none", await Css(badge, "box-shadow"));
            Assert.Equal("16px", await Css(page.Locator("[data-pair-id='card-small'] [data-slot='card-content']"), "padding-left"));

            var vertical = page.Locator("[data-pair-id='carousel-vertical'] [data-slot='carousel']");
            await Assertions.Expect(vertical).ToHaveAttributeAsync("data-orientation", "vertical");
            Assert.Equal("pan-x pinch-zoom", await Css(vertical.Locator("[data-slot='carousel-content']"), "touch-action"));
            Assert.Equal("column", await Css(vertical.Locator("[data-slot='carousel-track']"), "flex-direction"));
            foreach (var button in new[] { "carousel-previous", "carousel-next" })
            {
                Assert.Equal("32px", await Css(vertical.Locator($"[data-slot='{button}']"), "width"));
                Assert.Equal("32px", await Css(vertical.Locator($"[data-slot='{button}']"), "height"));
                Assert.Equal("9999px", await Css(vertical.Locator($"[data-slot='{button}']"), "border-radius"));
            }

            var progress = page.Locator("[data-pair-id='progress-indeterminate'] [data-slot='progress']");
            await Assertions.Expect(progress).ToHaveAttributeAsync("data-state", "indeterminate");
            await Assertions.Expect(progress).Not.ToHaveAttributeAsync("aria-valuenow", "0");
            var indicator = progress.Locator("[data-slot='progress-indicator']");
            Assert.Equal("shadcn-progress-indeterminate", await Css(indicator, "animation-name"));
            Assert.Equal(direction == "rtl" ? "reverse" : "normal", await Css(indicator, "animation-direction"));

            var skeleton = page.Locator("[data-pair-id='skeleton'] [data-slot='skeleton']");
            Assert.Equal("shadcn-skeleton-pulse", await Css(skeleton, "animation-name"));
            Assert.Equal("8px", await Css(skeleton, "border-radius"));
            var spinner = page.Locator("[data-pair-id='spinner'] [data-slot='spinner']");
            Assert.Equal("shadcn-spinner-rotate", await Css(spinner, "animation-name"));
            Assert.Equal("16px", await Css(spinner, "width"));

            await page.GetByRole(AriaRole.Button, new() { Name = "Show loading toast", Exact = true }).ClickAsync();
            var toast = page.Locator("[data-component='toast'] [data-slot='toast'][data-type='loading']");
            await Assertions.Expect(toast).ToHaveAttributeAsync("data-state", "open");
            Assert.Equal("shadcn-spinner-rotate", await Css(toast.Locator("[data-slot='toast-icon']"), "animation-name"));
            await toast.HoverAsync();
            await Assertions.Expect(page.Locator("[data-slot='toast-viewport']")).ToHaveAttributeAsync("data-expanded", "true");
            themeColors[theme] = (alertColors[0], await Css(skeleton, "background-color"), await Css(toast, "background-color"));
        }

        Assert.NotEqual(themeColors["light"].Alert, themeColors["dark"].Alert);
        Assert.NotEqual(themeColors["light"].Skeleton, themeColors["dark"].Skeleton);
        Assert.NotEqual(themeColors["light"].Toast, themeColors["dark"].Toast);
    }

    [Fact]
    public async Task VerticalAndRtlCarouselsUseMeasuredPhysicalAxesAndLoop()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 800, Height = 900 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/feedback-and-content?dir=rtl").ToString());
        var vertical = page.GetByTestId("carousel-vertical").Locator("[data-slot='carousel']");
        await vertical.Locator("[data-slot='carousel-previous']").ClickAsync();
        await Assertions.Expect(vertical.Locator("[data-slot='carousel-item']").Nth(2)).ToHaveAttributeAsync("data-selected", "true");
        Assert.Contains("0px", await vertical.Locator("[data-slot='carousel-track']").EvaluateAsync<string>("element => getComputedStyle(element).translate"), StringComparison.Ordinal);
        var rtl = page.GetByTestId("carousel-rtl").Locator("[data-slot='carousel']");
        await rtl.DispatchEventAsync("keydown", new { key = "ArrowLeft" });
        await Assertions.Expect(rtl.Locator("[data-slot='carousel-item']").Nth(1)).ToHaveAttributeAsync("data-selected", "true");
        var translate = await rtl.Locator("[data-slot='carousel-track']").EvaluateAsync<string>("element => getComputedStyle(element).translate");
        Assert.NotEqual("none", translate);
    }
}
