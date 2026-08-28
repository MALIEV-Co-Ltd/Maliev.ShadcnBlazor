using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DocumentationWorkbenchBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData(1440, 900, false, false)]
    [InlineData(390, 844, true, false)]
    [InlineData(800, 900, true, true)]
    public async Task SharedDossierSelectUsesPackageKeyboardInteractionAndSynchronizesSource(int width, int height, bool darkRtl, bool forcedColors)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = darkRtl ? ColorScheme.Dark : ColorScheme.Light,
            ForcedColors = forcedColors ? ForcedColors.Active : ForcedColors.None
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/card").ToString());
        var controls = page.Locator(".component-preview__controls");
        await controls.WaitForAsync();

        if (darkRtl)
        {
            await page.GetByTestId("documentation-theme-toggle").ClickAsync();
            await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        }

        await Assertions.Expect(controls.Locator("select")).ToHaveCountAsync(0);
        await Assertions.Expect(controls.Locator("[data-slot='select']")).ToHaveCountAsync(1);
        var trigger = page.GetByTestId("control-card-size");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-label", "Size");
        await trigger.FocusAsync();
        await trigger.PressAsync("ArrowDown");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        await trigger.PressAsync("End");
        await trigger.PressAsync("Enter");

        await Assertions.Expect(page.Locator("[data-slot='card']")).ToHaveAttributeAsync("data-size", "sm");
        await Assertions.Expect(page.GetByTestId("component-preview").First.Locator("details[data-testid='example-source']"))
            .ToContainTextAsync("Size=\"ShadcnCardSize.Small\"");
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);
        Assert.Equal("solid", await trigger.EvaluateAsync<string>("element => getComputedStyle(element).borderTopStyle"));
    }

    [Theory]
    [InlineData(1440, 900, false)]
    [InlineData(390, 844, true)]
    public async Task SelectDossierOpensFromTriggerAndKeepsClearActionInsideCompactField(int width, int height, bool darkRtl)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = darkRtl ? ColorScheme.Dark : ColorScheme.Light
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/select").ToString());
        await page.GetByTestId("component-preview-canvas").WaitForAsync();

        if (darkRtl)
        {
            await page.GetByTestId("documentation-theme-toggle").ClickAsync();
            await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        }

        await Assertions.Expect(page.GetByTestId("control-select-open")).ToHaveCountAsync(0);
        var trigger = page.GetByTestId("forms-dossier-select");
        var root = trigger.Locator("xpath=ancestor-or-self::*[@data-slot='select'][1]");
        var clear = root.GetByRole(AriaRole.Button, new() { Name = "Clear selection" });
        var chevron = root.Locator("[data-slot='select-trigger-icon']");
        var rootBox = await root.BoundingBoxAsync();
        var clearBox = await clear.BoundingBoxAsync();
        var chevronBox = await chevron.BoundingBoxAsync();
        Assert.NotNull(rootBox);
        Assert.NotNull(clearBox);
        Assert.NotNull(chevronBox);
        Assert.InRange(clearBox.X, rootBox.X, rootBox.X + rootBox.Width - clearBox.Width);
        Assert.True(Math.Abs(clearBox.X - chevronBox.X) >= 12, "Clear action and chevron must not overlap.");

        await clear.ClickAsync();
        await Assertions.Expect(trigger).ToContainTextAsync("Select a process");
        await trigger.ClickAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        await Assertions.Expect(root.Locator("[role='group']")).ToHaveCountAsync(3);
        await root.Locator("[role='option'][data-value='slm']").ClickAsync();
        await Assertions.Expect(trigger).ToContainTextAsync("Metal 3D printing");

        await page.GetByTestId("control-select-invalid").CheckAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-invalid", "true");
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);
    }

    [Fact]
    public async Task RepositoryRootOpensTheDocumentationCatalog()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(server.BaseUri.ToString());

        await page.GetByTestId("documentation-workbench").WaitForAsync();
        Assert.EndsWith("/docs/components", new Uri(page.Url).AbsolutePath, StringComparison.Ordinal);
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Component catalog" })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".documentation-icon-action svg[aria-hidden='true']")).ToHaveCountAsync(2);
        await Assertions.Expect(page.GetByTestId("documentation-direction-toggle").Locator("[data-slot='icon']")).ToHaveAttributeAsync("data-library", "tabler");
        await Assertions.Expect(page.GetByTestId("documentation-direction-toggle").Locator("[data-slot='icon']")).ToHaveAttributeAsync("data-icon", "text-direction-rtl");
        await Assertions.Expect(page.GetByText("Build accessible Blazor interfaces with shadcn primitives")).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData(1440, 900)]
    [InlineData(390, 844)]
    public async Task DocumentationLandingRoutesSeparateLearningFromComponentDiscovery(int width, int height)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs").ToString());
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Get a themed Blazor interface running in five minutes" })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".documentation-topnav a[href='docs']")).ToHaveAttributeAsync("aria-current", "page");
        await Assertions.Expect(page.GetByTestId("outline-trigger")).ToHaveCountAsync(0);
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components").ToString());
        await Assertions.Expect(page.Locator(".documentation-topnav a[href='docs/components']")).ToHaveAttributeAsync("aria-current", "page");
        await Assertions.Expect(page.GetByTestId("outline-trigger")).ToHaveCountAsync(0);
        var search = page.GetByTestId("component-directory-search");
        await search.FillAsync("tooltip");
        await Assertions.Expect(page.Locator(".documentation-directory-link").Filter(new() { HasText = "Tooltip" })).ToHaveCountAsync(1);
        Assert.InRange(await page.Locator(".documentation-directory-link").CountAsync(), 1, 4);
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);
    }

    [Fact]
    public async Task CalendarPreviewKeepsAnIntrinsicSquareSurface()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
            Locale = "th-TH",
            TimezoneId = "Asia/Bangkok"
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/calendar").ToString());
        await page.GetByTestId("component-preview-canvas").WaitForAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var calendar = canvas.Locator("[data-slot='calendar']");
        var canvasBox = await canvas.BoundingBoxAsync();
        var calendarBox = await calendar.BoundingBoxAsync();
        Assert.NotNull(canvasBox);
        Assert.NotNull(calendarBox);
        Assert.True(calendarBox.Width < canvasBox.Width * 0.5, $"Calendar should remain intrinsic, got {calendarBox.Width}px inside {canvasBox.Width}px.");
        Assert.True(calendarBox.Height >= calendarBox.Width, $"Calendar surface should remain square-oriented, got {calendarBox.Width}x{calendarBox.Height}px.");
        var today = calendar.Locator("[data-day='2026-08-13']");
        Assert.Equal("true", await today.GetAttributeAsync("data-selected-single"));
        Assert.NotEqual("true", await today.GetAttributeAsync("aria-disabled"));
        var todayPaint = await today.EvaluateAsync<string>("element => { const style = getComputedStyle(element); return `${style.backgroundColor}|${style.color}|${style.opacity}|${style.getPropertyValue('--shadcn-primary')}|${style.getPropertyValue('--shadcn-muted')}`; }");
        Assert.Equal("oklch(0.205 0 0)|oklch(0.985 0 0)|1|oklch(0.205 0 0)|oklch(0.97 0 0)", todayPaint);
    }

    [Fact]
    public async Task ComponentDossierProgressivelyDisclosesSourceAndKeepsReferenceTextReadable()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Light
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/button").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var preview = page.GetByTestId("component-preview").First;
        var source = preview.Locator("details[data-testid='example-source']");
        await Assertions.Expect(source).Not.ToHaveAttributeAsync("open", "");
        await source.Locator("summary").ClickAsync();
        await Assertions.Expect(source).ToHaveAttributeAsync("open", "");
        Assert.Equal(1, await page.Locator("text=Example source").CountAsync());
        await Assertions.Expect(page.Locator("#usage")).ToContainTextAsync("Use when");
        await Assertions.Expect(page.Locator("#usage")).ToContainTextAsync("Avoid when");
        await Assertions.Expect(page.Locator("#usage a[href='#preview']")).ToBeVisibleAsync();

        var dossierProse = page.Locator("""
            .component-dossier__hero > p:not(.documentation-eyebrow),
            .component-dossier__heading p,
            .component-dossier__planned p,
            .component-guide > p,
            .component-accessibility > .documentation-prose-list > li,
            .component-token-guidance > p,
            .component-token-guidance > .documentation-prose-list > li,
            .component-reference > p,
            .component-reference > .documentation-prose-list > li
            """);
        await Assertions.Expect(page.Locator(".component-token-guidance > p")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".component-token-guidance > .documentation-prose-list > li")).Not.ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator(".component-reference > p")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator(".component-reference > .documentation-prose-list > li")).Not.ToHaveCountAsync(0);

        var overMeasure = await dossierProse
            .EvaluateAllAsync<string[]>("""
                elements => elements.flatMap(element => {
                    const probe = document.createElement('span');
                    const style = getComputedStyle(element);
                    probe.style.cssText = `position:absolute;visibility:hidden;inline-size:75ch;font:${style.font};`;
                    document.body.append(probe);
                    const limit = probe.getBoundingClientRect().width;
                    probe.remove();
                    const width = element.getBoundingClientRect().width;
                    return width <= limit + 1 ? [] : [`${element.textContent.trim().slice(0, 32)}:${width.toFixed(1)}>${limit.toFixed(1)}`];
                })
                """);
        Assert.True(overMeasure.Length == 0, $"Prose exceeded 75ch: {string.Join(", ", overMeasure)}");

        var proseMaxInlineSize = await page.Locator(".component-token-guidance > p")
            .EvaluateAsync<string>("element => getComputedStyle(element).maxInlineSize");
        var nonProseMaxInlineSizes = await page.Locator(".component-code__surface pre, .component-api__table th, .component-api__table td")
            .EvaluateAllAsync<string[]>("elements => elements.map(element => getComputedStyle(element).maxInlineSize)");
        Assert.DoesNotContain(proseMaxInlineSize, nonProseMaxInlineSizes);

        var apiHeaderContrast = await page.Locator(".component-api__table thead th").First.EvaluateAsync<double>("""
            element => {
                const parse = color => {
                    const canvas = document.createElement('canvas');
                    canvas.width = canvas.height = 1;
                    const context = canvas.getContext('2d', { willReadFrequently: true });
                    context.fillStyle = color;
                    context.fillRect(0, 0, 1, 1);
                    return [...context.getImageData(0, 0, 1, 1).data].slice(0, 3);
                };
                const luminance = color => {
                    const channels = color.map(value => {
                        const normalized = value / 255;
                        return normalized <= 0.04045 ? normalized / 12.92 : Math.pow((normalized + 0.055) / 1.055, 2.4);
                    });
                    return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722;
                };
                const style = getComputedStyle(element);
                const foreground = luminance(parse(style.color));
                const background = luminance(parse(style.backgroundColor));
                return (Math.max(foreground, background) + 0.05) / (Math.min(foreground, background) + 0.05);
            }
            """);
        Assert.True(apiHeaderContrast >= 4.5, $"API header contrast was {apiHeaderContrast:F2}:1.");
    }

    public static TheoryData<int, int> Viewports => new()
    {
        { 1440, 900 },
        { 1024, 768 },
        { 768, 1024 },
        { 390, 844 },
        { 320, 568 }
    };

    public static TheoryData<int, int> MobileViewports => new()
    {
        { 768, 1024 },
        { 390, 844 },
        { 320, 568 }
    };

    public static TheoryData<int, int, bool, bool> HeaderViewports => new()
    {
        { 1440, 900, false, false },
        { 1440, 900, true, false },
        { 1024, 768, false, true },
        { 1024, 768, true, true },
        { 390, 844, false, false },
        { 390, 844, true, false },
        { 320, 568, false, false }
    };

    [Theory]
    [MemberData(nameof(HeaderViewports))]
    public async Task HeaderSpansViewportAndKeepsShellControlsAtLogicalEdges(
        int width,
        int height,
        bool rtl,
        bool forcedColors)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = forcedColors ? ForcedColors.Active : ForcedColors.None
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/kbd").ToString());
        await page.GetByTestId("documentation-workbench").WaitForAsync();

        if (rtl)
        {
            await page.GetByTestId("documentation-direction-toggle").ClickAsync();
            await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
        }

        var header = page.Locator(".documentation-header");
        var leading = page.Locator(".documentation-header__leading");
        var actions = page.Locator(".documentation-header__actions");
        var headerBox = await header.BoundingBoxAsync();
        var leadingBox = await leading.BoundingBoxAsync();
        var actionsBox = await actions.BoundingBoxAsync();
        Assert.NotNull(headerBox);
        Assert.NotNull(leadingBox);
        Assert.NotNull(actionsBox);
        Assert.InRange(headerBox.X, -0.5, 0.5);
        Assert.InRange(headerBox.Width, width - 1, width + 1);

        var edgeMetrics = await header.EvaluateAsync<double[]>("""
            element => {
                const style = getComputedStyle(element);
                return [parseFloat(style.paddingLeft), parseFloat(style.paddingRight)];
            }
            """);
        Assert.All(edgeMetrics, gutter => Assert.InRange(gutter, 12, 32));
        var logicalStartBox = rtl ? actionsBox : leadingBox;
        var logicalEndBox = rtl ? leadingBox : actionsBox;
        Assert.InRange(logicalStartBox.X - edgeMetrics[0], -1, 1);
        Assert.InRange(width - edgeMetrics[1] - (logicalEndBox.X + logicalEndBox.Width), -1, 1);

        var topnav = page.Locator(".documentation-topnav");
        if (width > 1216)
        {
            var navBox = await topnav.BoundingBoxAsync();
            Assert.NotNull(navBox);
            Assert.InRange((navBox.X + navBox.Width / 2) - width / 2d, -1, 1);
        }
        else
        {
            await Assertions.Expect(topnav).ToBeHiddenAsync();
        }

        if (width <= 640)
        {
            var catalogTrigger = page.GetByTestId("catalog-trigger");
            var outlineTrigger = page.GetByTestId("outline-trigger");
            var brand = page.Locator(".documentation-brand");
            await Assertions.Expect(brand).ToBeVisibleAsync();
            await Assertions.Expect(page.Locator(".documentation-brand > span:last-child")).ToBeHiddenAsync();
            await Assertions.Expect(catalogTrigger).ToHaveAccessibleNameAsync("Open component catalog");
            await Assertions.Expect(outlineTrigger).ToHaveAccessibleNameAsync("On This Page");
            Assert.InRange((await catalogTrigger.BoundingBoxAsync())!.Width, 44, 48);
            Assert.InRange((await outlineTrigger.BoundingBoxAsync())!.Width, 44, 48);
            Assert.InRange((await brand.BoundingBoxAsync())!.Height, 44, 48);
            Assert.InRange((await page.GetByTestId("documentation-kofi-link").BoundingBoxAsync())!.Width, 40, 48);
            Assert.InRange((await page.GetByTestId("documentation-theme-toggle").BoundingBoxAsync())!.Width, 44, 48);
            Assert.InRange((await page.GetByTestId("documentation-direction-toggle").BoundingBoxAsync())!.Width, 44, 48);
            var order = await header.Locator("a.documentation-brand, a.documentation-kofi, button").EvaluateAllAsync<string[]>("elements => elements.map(element => element.matches('.documentation-brand') ? 'brand' : element.dataset.testid || '')");
            Assert.Equal(["brand", "catalog-trigger", "outline-trigger", "documentation-kofi-link", "documentation-theme-toggle", "documentation-direction-toggle"], order);
        }

        var article = page.Locator(".component-dossier");
        var articleBox = await article.BoundingBoxAsync();
        Assert.NotNull(articleBox);
        Assert.True(articleBox.Width <= 928.5, $"The readable article measure expanded to {articleBox.Width}px.");

        var brandLink = page.Locator(".documentation-brand");
        await brandLink.FocusAsync();
        await Assertions.Expect(brandLink).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Shift+Tab");
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(brandLink).ToBeFocusedAsync();
        Assert.NotEqual("none", await brandLink.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));

        var themeToggle = page.GetByTestId("documentation-theme-toggle");
        await themeToggle.FocusAsync();
        await Assertions.Expect(themeToggle).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Tab");
        var directionToggle = page.GetByTestId("documentation-direction-toggle");
        await Assertions.Expect(directionToggle).ToBeFocusedAsync();
        Assert.NotEqual("none", await directionToggle.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
        var reducedTransitionSeconds = await page.Locator("#documentation-catalog").EvaluateAsync<double>(
            "element => parseFloat(getComputedStyle(element).transitionDuration)");
        Assert.InRange(reducedTransitionSeconds, 0, 0.001);

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
    }

    [Fact]
    public async Task DocumentationShellMatchesReviewedVisualProof()
    {
        foreach (var mode in new[] { VisualProofMode.DesktopLight, VisualProofMode.MobileDarkRtl })
        {
            await using var context = await playwright.Browser.NewContextAsync(new()
            {
                ViewportSize = mode.Viewport,
                DeviceScaleFactor = 1,
                Locale = "th-TH",
                TimezoneId = "Asia/Bangkok",
                ReducedMotion = ReducedMotion.Reduce,
                ColorScheme = mode.Dark ? ColorScheme.Dark : ColorScheme.Light
            });
            var page = await context.NewPageAsync();
            await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/kbd").ToString());
            await page.GetByTestId("documentation-workbench").WaitForAsync();

            if (mode.Dark)
            {
                await page.GetByTestId("documentation-theme-toggle").ClickAsync();
                await page.GetByTestId("documentation-direction-toggle").ClickAsync();
                await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
            }

            await page.EvaluateAsync("document.fonts.ready");
            var actual = await page.ScreenshotAsync(new() { Animations = ScreenshotAnimations.Disabled });
            await VisualProof.CompareOrUpdateAsync(page, "documentation-shell", mode.Name, actual);
        }
    }

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task WorkbenchSearchNavigationAndResponsiveDrawersStayHealthy(int width, int height)
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

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/kbd").ToString());
        await page.GetByTestId("documentation-workbench").WaitForAsync();

        var catalog = page.Locator("#documentation-catalog");
        var outline = page.Locator("#documentation-outline");
        var content = page.Locator("#documentation-content");
        await Assertions.Expect(outline.Locator("a[href='#installation']")).ToHaveTextAsync("Installation");
        await Assertions.Expect(outline.Locator("a[href='#api-reference']")).ToHaveTextAsync("API Reference");
        var activeOutlineItem = outline.Locator("a[href='#overview']");
        await Assertions.Expect(activeOutlineItem).ToHaveAttributeAsync("data-active", "true");
        var activeOutlineStyle = await activeOutlineItem.EvaluateAsync<string>("element => { const style = getComputedStyle(element); return `${style.fontWeight}|${style.backgroundColor}|${style.borderInlineStartColor}`; }");
        var activeStyleParts = activeOutlineStyle.Split('|');
        Assert.True(int.Parse(activeStyleParts[0], System.Globalization.CultureInfo.InvariantCulture) >= 700);
        Assert.Contains("0, 0, 0, 0", activeStyleParts[1], StringComparison.Ordinal);
        Assert.Contains("0, 0, 0, 0", activeStyleParts[2], StringComparison.Ordinal);
        await Assertions.Expect(content.Locator("#usage")).ToContainTextAsync("Use when");
        await Assertions.Expect(content.Locator("#usage")).ToContainTextAsync("Avoid when");
        await Assertions.Expect(content.Locator("#usage a[href='#preview']")).ToBeVisibleAsync();
        if (width > 1280)
        {
            var catalogBox = await catalog.BoundingBoxAsync();
            var contentBox = await content.BoundingBoxAsync();
            var outlineBox = await outline.BoundingBoxAsync();
            Assert.NotNull(catalogBox);
            Assert.NotNull(contentBox);
            Assert.NotNull(outlineBox);
            Assert.True(catalogBox.X < contentBox.X);
            Assert.True(contentBox.X < outlineBox.X);
            Assert.True(contentBox.Width > 800, $"Expected a full-width content column, got {contentBox.Width}px.");
        }

        if (width <= 768)
        {
            await page.GetByTestId("catalog-trigger").ClickAsync();
            await Assertions.Expect(catalog).ToHaveAttributeAsync("data-open", "true");
        }

        var search = page.GetByLabel("Search components");
        await search.FillAsync("keyboard");
        await Assertions.Expect(page.GetByTestId("documentation-result-count")).ToHaveTextAsync("13 components found");
        Assert.Equal(
            ["accordion", "calendar", "code-block", "combobox", "command", "context-menu", "dropdown-menu", "dropzone", "kbd", "navigation-menu", "resizable", "select", "toast"],
            (await page.Locator(".documentation-component-list a").EvaluateAllAsync<string[]>("links => links.map(link => new URL(link.href).pathname.split('/').pop())")).Order());
        await Assertions.Expect(page.Locator("a[href='docs/components/kbd']")).ToHaveAttributeAsync("aria-current", "page");

        if (width <= 768)
        {
            await page.Keyboard.PressAsync("Escape");
            await Assertions.Expect(catalog).ToHaveAttributeAsync("data-open", "false");
            await Assertions.Expect(page.GetByTestId("catalog-trigger")).ToBeFocusedAsync();
        }

        if (width <= 1280)
        {
            await page.GetByTestId("outline-trigger").ClickAsync();
            await Assertions.Expect(outline).ToHaveAttributeAsync("data-open", "true");
            await page.Keyboard.PressAsync("Escape");
            await Assertions.Expect(outline).ToHaveAttributeAsync("data-open", "false");
            await Assertions.Expect(page.GetByTestId("outline-trigger")).ToBeFocusedAsync();
        }

        await page.GetByTestId("documentation-theme-toggle").ClickAsync();
        await Assertions.Expect(page.GetByTestId("documentation-theme-toggle")).ToHaveAccessibleNameAsync("Use light theme");
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await Assertions.Expect(page.GetByTestId("documentation-direction-toggle")).ToHaveAccessibleNameAsync("Use left-to-right direction");

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        var overflowSources = await page.EvaluateAsync<string[]>(
            "Array.from(document.querySelectorAll('*')).map(element => ({ element, rect: element.getBoundingClientRect() })).filter(item => item.rect.right > document.documentElement.clientWidth + 1 || item.rect.left < -1).map(item => `${item.element.tagName.toLowerCase()}.${item.element.className || ''} [${item.rect.left}, ${item.rect.right}]`)");
        Assert.True(overflow is >= 0 and <= 1, $"Horizontal overflow was {overflow}px. Sources: {string.Join("; ", overflowSources)}");
        Assert.Empty(errors);
    }

    [Theory]
    [MemberData(nameof(MobileViewports))]
    public async Task CatalogSkipLinkOpensTheMobileDrawerAndFocusesCatalogNavigation(int width, int height)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/kbd").ToString());
        await page.GetByTestId("documentation-workbench").WaitForAsync();

        var skipLink = page.GetByRole(AriaRole.Link, new() { Name = "Skip to component navigation" });
        await skipLink.FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Assertions.Expect(page.Locator("#documentation-catalog")).ToHaveAttributeAsync("data-open", "true");
        await Assertions.Expect(page.Locator("#documentation-catalog")).ToBeFocusedAsync();
    }

    [Theory]
    [MemberData(nameof(MobileViewports))]
    public async Task DrawerCloseButtonsRestoreFocusForClickAndKeyboardActivation(int width, int height)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/kbd").ToString());
        await page.GetByTestId("documentation-workbench").WaitForAsync();

        await page.GetByTestId("catalog-trigger").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Close component catalog" }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("catalog-trigger")).ToBeFocusedAsync();

        await page.GetByTestId("catalog-trigger").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Close component catalog" }).FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(page.GetByTestId("catalog-trigger")).ToBeFocusedAsync();

        await page.GetByTestId("outline-trigger").ClickAsync();
        var outline = page.Locator("#documentation-outline");
        await Assertions.Expect(outline).ToHaveAttributeAsync("role", "dialog");
        await Assertions.Expect(outline).ToHaveAttributeAsync("aria-modal", "true");
        await Assertions.Expect(outline).ToHaveAttributeAsync("data-drawer-ready", "true");
        await Assertions.Expect(outline).ToHaveAttributeAsync("data-drawer-focus-ready", "true");
        var outlineClose = page.GetByTestId("outline-close");
        var outlineCloseBox = await outlineClose.BoundingBoxAsync();
        Assert.NotNull(outlineCloseBox);
        Assert.InRange(outlineCloseBox.Width, 44, 48);
        Assert.InRange(outlineCloseBox.Height, 44, 48);
        await Assertions.Expect(outlineClose).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Shift+Tab");
        await Assertions.Expect(outline.Locator("a").Last).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(outlineClose).ToBeFocusedAsync();
        await outlineClose.ClickAsync();
        await Assertions.Expect(page.GetByTestId("outline-trigger")).ToBeFocusedAsync();

        await page.GetByTestId("outline-trigger").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Close page outline" }).FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(page.GetByTestId("outline-trigger")).ToBeFocusedAsync();
    }
}
