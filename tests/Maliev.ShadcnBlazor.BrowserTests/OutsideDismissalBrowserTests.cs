using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class OutsideDismissalBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData("dropdown-menu", "dropdown-menu-trigger", "dropdown-menu-content")]
    [InlineData("menubar", "menubar-trigger", "menubar-content")]
    public async Task MenuSurfaceClosesWhenTheUserPressesOutside(
        string component,
        string triggerSlot,
        string contentSlot)
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();
        await OpenComponentAsync(page, component);

        var trigger = page.Locator($"#preview [data-slot='{triggerSlot}']").First;
        await trigger.ClickAsync();
        var content = page.Locator($"#preview [data-slot='{contentSlot}']");
        await Assertions.Expect(content).ToBeVisibleAsync();

        await page.Locator("#overview h1").ClickAsync();

        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
    }

    [Fact]
    public async Task ContextMenuClosesWhenTheUserPressesOutside()
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();
        await OpenComponentAsync(page, "context-menu");

        var trigger = page.Locator("#preview [data-slot='context-menu-trigger']");
        await trigger.ClickAsync(new() { Button = MouseButton.Right });
        var content = page.Locator("#preview [data-slot='context-menu-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();

        await page.Locator("#overview h1").ClickAsync();

        await Assertions.Expect(content).ToHaveCountAsync(0);
    }

    [Theory]
    [InlineData("dialog", "dialog-trigger", "dialog-overlay", "dialog-content")]
    [InlineData("drawer", "drawer-trigger", "drawer-overlay", "drawer-content")]
    [InlineData("sheet", "sheet-trigger", "sheet-overlay", "sheet-content")]
    public async Task ModalSurfaceClosesWhenTheUserPressesItsBackdrop(
        string component,
        string triggerSlot,
        string backdropSlot,
        string contentSlot)
    {
        await using var context = await CreateContextAsync();
        var page = await context.NewPageAsync();
        await OpenComponentAsync(page, component);

        await page.Locator($"#preview [data-slot='{triggerSlot}']").First.ClickAsync();
        var content = page.Locator($"[data-slot='{contentSlot}']");
        await Assertions.Expect(content).ToBeVisibleAsync();

        await page.Locator($"[data-slot='{backdropSlot}']").ClickAsync(new() { Position = new() { X = 4, Y = 4 } });

        await Assertions.Expect(content).ToHaveCountAsync(0);
    }

    private async Task<IBrowserContext> CreateContextAsync() =>
        await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });

    private async Task OpenComponentAsync(IPage page, string component)
    {
        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{component}").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
    }
}
