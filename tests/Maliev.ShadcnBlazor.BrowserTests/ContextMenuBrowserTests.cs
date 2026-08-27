using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ContextMenuBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task ContextMenuTriggerUsesTheContextMenuCursorAcrossNestedText()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/context-menu").ToString());

        var trigger = page.Locator("#preview [data-slot='context-menu-trigger']");
        await trigger.WaitForAsync();
        var cursors = await trigger.EvaluateAsync<string[]>("""
            element => [
                getComputedStyle(element).cursor,
                getComputedStyle(element.querySelector('p')).cursor
            ]
            """);

        Assert.Equal(["context-menu", "context-menu"], cursors);
    }

    [Fact]
    public async Task ContextMenuSupportsPointerKeyboardSelectionSubmenuDismissalAndFocusRestore()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/context-menu").ToString());

        var trigger = page.Locator("#preview [data-slot='context-menu-trigger']");
        var menu = page.Locator("#preview [data-slot='context-menu-content']");
        await trigger.WaitForAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("data-context-menu-ready", "true");
        await trigger.ClickAsync(new() { Button = MouseButton.Right, Position = new() { X = 120, Y = 90 } });
        await Assertions.Expect(menu).ToHaveAttributeAsync("data-positioned", "true");
        await Assertions.Expect(menu).ToBeVisibleAsync();
        var firstItem = menu.Locator("[role^='menuitem']").First;
        await Assertions.Expect(firstItem).ToBeFocusedAsync();
        var renameItem = menu.GetByRole(AriaRole.Menuitem, new() { Name = "Rename", Exact = true });
        await renameItem.HoverAsync();
        var hoverColor = await renameItem.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.NotEqual("rgba(0, 0, 0, 0)", hoverColor);
        Assert.NotEqual("transparent", hoverColor);

        var bounds = await menu.BoundingBoxAsync();
        Assert.NotNull(bounds);
        Assert.True(bounds!.X >= 8 && bounds.Y >= 8, $"Menu origin was ({bounds.X}, {bounds.Y}).");
        Assert.True(bounds.X + bounds.Width <= 1272, $"Menu right edge was {bounds.X + bounds.Width}.");
        Assert.True(bounds.Y + bounds.Height <= 892, $"Menu bottom edge was {bounds.Y + bounds.Height}.");

        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(menu.GetByRole(AriaRole.Menuitem, new() { Name = "Rename", Exact = true })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        var archivedItem = menu.GetByRole(AriaRole.Menuitemcheckbox, new() { Name = "Show archived files", Exact = true });
        await Assertions.Expect(archivedItem).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Space");
        await Assertions.Expect(archivedItem).ToHaveAttributeAsync("aria-checked", "false");
        await Assertions.Expect(menu).ToBeVisibleAsync();

        var subTrigger = menu.GetByRole(AriaRole.Menuitem, new() { Name = "Export as", Exact = true });
        await subTrigger.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        var submenu = page.Locator("#preview [data-slot='context-menu-sub-content']");
        await Assertions.Expect(submenu).ToBeVisibleAsync();
        await Assertions.Expect(submenu.GetByRole(AriaRole.Menuitem, new() { Name = "PDF package", Exact = true })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(submenu).ToHaveCountAsync(0);
        await Assertions.Expect(subTrigger).ToBeFocusedAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(menu).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();

        await Assertions.Expect(trigger).ToHaveAttributeAsync("data-context-menu-ready", "true");
        await trigger.PressAsync("Shift+F10");
        await Assertions.Expect(menu).ToHaveAttributeAsync("data-positioned", "true");
        await Assertions.Expect(menu).ToBeVisibleAsync();
        bounds = await menu.BoundingBoxAsync();
        var triggerBounds = await trigger.BoundingBoxAsync();
        Assert.NotNull(bounds);
        Assert.NotNull(triggerBounds);
        Assert.True(bounds!.X >= triggerBounds!.X - 1, "Keyboard invocation should anchor to the trigger, not the viewport origin.");

        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(menu.GetByRole(AriaRole.Menuitem, new() { Name = "Rename", Exact = true })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(menu).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }

    [Fact]
    public async Task ContextMenuSourceIsCompleteStateAwareAndForcedColorsRemainLegible()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/context-menu").ToString());

        var trigger = page.Locator("#preview [data-slot='context-menu-trigger']");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("data-context-menu-ready", "true");
        await trigger.PressAsync("Shift+F10");
        var menu = page.Locator("#preview [data-slot='context-menu-content']");
        await Assertions.Expect(menu).ToHaveAttributeAsync("data-positioned", "true");
        await Assertions.Expect(menu).ToBeVisibleAsync();
        var styles = await menu.EvaluateAsync<ComputedStyles>("element => { const value = getComputedStyle(element); return { borderStyle: value.borderStyle, animationName: value.animationName }; }");
        Assert.Equal("solid", styles.BorderStyle);
        Assert.Equal("none", styles.AnimationName);

        await page.Keyboard.PressAsync("Escape");
        var preview = page.GetByTestId("component-preview").First;
        var sourceDisclosure = preview.Locator("details[data-testid='example-source']");
        await Assertions.Expect(sourceDisclosure).Not.ToHaveAttributeAsync("open", "");
        await sourceDisclosure.Locator("summary").ClickAsync();
        await Assertions.Expect(sourceDisclosure).ToHaveAttributeAsync("open", "");
        var source = sourceDisclosure.Locator("[data-slot='code-block']");
        await Assertions.Expect(source).ToBeVisibleAsync();
        var sourceText = await source.InnerTextAsync();
        Assert.Contains("<ShadcnContextMenuCheckboxItem", sourceText, StringComparison.Ordinal);
        Assert.Contains("<ShadcnContextMenuRadioGroup", sourceText, StringComparison.Ordinal);
        Assert.Contains("<ShadcnContextMenuSub>", sourceText, StringComparison.Ordinal);
        Assert.Contains("<ShadcnContextMenuShortcut", sourceText, StringComparison.Ordinal);
        Assert.Contains("Shift+F10", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("...", sourceText, StringComparison.Ordinal);
    }

    private sealed class ComputedStyles
    {
        public string BorderStyle { get; init; } = string.Empty;
        public string AnimationName { get; init; } = string.Empty;
    }
}
