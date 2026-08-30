using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class VirtualizedOptionsBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task LargeOptionControlsKeepKeyboardActiveDescendantsMounted()
    {
        var page = await playwright.Browser.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/tests/virtualized-options").ToString());
        await page.GetByTestId("virtualized-options-fixture").WaitForAsync();

        var select = page.GetByRole(AriaRole.Combobox, new() { Name = "Virtualized select" });
        await select.FocusAsync();
        await select.PressAsync("End");
        await AssertMountedActiveOptionAsync(page, select, "Option 999");

        var combobox = page.GetByRole(AriaRole.Combobox, new() { Name = "Virtualized combobox" });
        await combobox.FocusAsync();
        await combobox.PressAsync("End");
        await AssertMountedActiveOptionAsync(page, combobox, "Option 999");

        Assert.InRange(await page.Locator("[data-slot='select-item']").CountAsync(), 1, 999);
        Assert.InRange(await page.Locator("[data-slot='combobox-item']").CountAsync(), 1, 999);
    }

    private static async Task AssertMountedActiveOptionAsync(IPage page, ILocator control, string expectedLabel)
    {
        var activeId = await control.GetAttributeAsync("aria-activedescendant");
        Assert.False(string.IsNullOrWhiteSpace(activeId));
        var active = page.Locator($"#{activeId}");
        await Assertions.Expect(active).ToHaveCountAsync(1);
        await Assertions.Expect(active).ToHaveAttributeAsync("aria-label", expectedLabel);
    }
}
