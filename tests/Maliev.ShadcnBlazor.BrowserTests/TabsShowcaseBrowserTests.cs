using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class TabsShowcaseBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task DossierTabsSupportDirectSelectionManualRovingAndExactDynamicSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/tabs").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var preview = page.GetByTestId("component-preview-canvas");
        var tabs = preview.GetByRole(AriaRole.Tab);
        await Assertions.Expect(tabs).ToHaveCountAsync(4);
        await tabs.Nth(1).ClickAsync();
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
        await Assertions.Expect(preview).ToContainTextAsync("inspection-plan.pdf");

        var valueControl = page.GetByTestId("control-tabs-value");
        await valueControl.SelectOptionAsync("activity");
        await valueControl.SelectOptionAsync("overview");
        await page.GetByTestId("control-tabs-orientation").SelectOptionAsync("Vertical");
        await page.GetByTestId("control-tabs-activation").SelectOptionAsync("Manual");
        await page.GetByTestId("control-tabs-variant").SelectOptionAsync("Line");
        await page.GetByTestId("control-tabs-loop").UncheckAsync();
        await page.GetByTestId("control-tabs-force").UncheckAsync();

        var root = preview.Locator("[data-slot='tabs']");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-orientation", "vertical");
        await Assertions.Expect(root.Locator("[data-slot='tabs-list']")).ToHaveAttributeAsync("data-variant", "line");
        var source = page.Locator("#preview .component-code").First;
        await Assertions.Expect(source).ToContainTextAsync("ShadcnTabsOrientation.Vertical");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnTabsActivationMode.Manual");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnTabsListVariant.Line");
        await Assertions.Expect(source).ToContainTextAsync("Loop=\"false\"");
        await Assertions.Expect(source).ToContainTextAsync("ForceMount=\"false\"");

        tabs = preview.GetByRole(AriaRole.Tab);
        await tabs.First.FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(tabs.Nth(1)).ToBeFocusedAsync();
        await Assertions.Expect(tabs.First).ToHaveAttributeAsync("aria-selected", "true");
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(tabs.Nth(1)).ToHaveAttributeAsync("aria-selected", "true");
    }

    [Fact]
    public async Task TabsRemainContainedInDarkRtlMobileAndExposeForcedColorFocus()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 320, Height = 720 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active,
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/tabs?theme=dark&dir=rtl&locale=th").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        await page.GetByTestId("control-tabs-orientation").SelectOptionAsync("Vertical");

        var canvas = page.GetByTestId("component-preview-canvas");
        var overflow = await canvas.EvaluateAsync<double>("element => element.scrollWidth - element.clientWidth");
        Assert.InRange(overflow, 0, 1);
        var tab = canvas.GetByRole(AriaRole.Tab).First;
        await tab.FocusAsync();
        var focus = await tab.EvaluateAsync<string[]>("element => [getComputedStyle(element).outlineStyle, getComputedStyle(element).borderColor]");
        Assert.True(focus[0] != "none" || focus[1] != "rgba(0, 0, 0, 0)");
    }
}
