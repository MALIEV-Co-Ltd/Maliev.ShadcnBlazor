using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DocumentationWorkbenchBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
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
        await Assertions.Expect(page.GetByText("Build accessible Blazor interfaces with shadcn primitives")).ToBeVisibleAsync();
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
        await Assertions.Expect(content.Locator("#usage")).ToContainTextAsync("@using Maliev.ShadcnBlazor");
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
        await Assertions.Expect(page.GetByTestId("documentation-result-count")).ToHaveTextAsync("11 components found");
        Assert.Equal(
            ["accordion", "calendar", "combobox", "command", "context-menu", "dropdown-menu", "kbd", "navigation-menu", "resizable", "select", "toast"],
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
        await page.GetByRole(AriaRole.Button, new() { Name = "Close page outline" }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("outline-trigger")).ToBeFocusedAsync();

        await page.GetByTestId("outline-trigger").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Close page outline" }).FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(page.GetByTestId("outline-trigger")).ToBeFocusedAsync();
    }
}
