using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DocumentationWorkbenchBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
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
        var theme = page.Locator("#documentation-theme");
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
            await page.Locator(".documentation-component-list a").EvaluateAllAsync<string[]>("links => links.map(link => new URL(link.href).pathname.split('/').pop())"));
        await Assertions.Expect(page.Locator("a[href='docs/components/kbd']")).ToHaveAttributeAsync("aria-current", "page");

        if (width <= 768)
        {
            await page.Keyboard.PressAsync("Escape");
            await Assertions.Expect(catalog).ToHaveAttributeAsync("data-open", "false");
            await Assertions.Expect(page.GetByTestId("catalog-trigger")).ToBeFocusedAsync();
        }

        if (width <= 1024)
        {
            await page.GetByTestId("theme-dock-trigger").ClickAsync();
            await Assertions.Expect(theme).ToHaveAttributeAsync("data-open", "true");
            await page.Keyboard.PressAsync("Escape");
            await Assertions.Expect(theme).ToHaveAttributeAsync("data-open", "false");
            await Assertions.Expect(page.GetByTestId("theme-dock-trigger")).ToBeFocusedAsync();
        }

        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
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

        await page.GetByTestId("theme-dock-trigger").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Close theme studio" }).ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-dock-trigger")).ToBeFocusedAsync();

        await page.GetByTestId("theme-dock-trigger").ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Close theme studio" }).FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(page.GetByTestId("theme-dock-trigger")).ToBeFocusedAsync();
    }
}
