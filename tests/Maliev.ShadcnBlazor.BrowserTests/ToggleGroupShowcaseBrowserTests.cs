using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ToggleGroupShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task DrawingLayerToolbarSupportsDirectAndConfiguredSelection()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/toggle-group").ToString());

        var dossier = page.GetByTestId("toggle-group-dossier");
        await dossier.WaitForAsync();
        var dimensions = page.GetByTestId("toggle-group-dimensions");
        var notes = page.GetByTestId("toggle-group-notes");

        await notes.ClickAsync();
        await Assertions.Expect(dimensions).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(notes).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(page.GetByTestId("toggle-group-selection")).ToContainTextAsync("Dimensions, Notes");

        await page.GetByTestId("control-toggle-group-multiple").UncheckAsync();
        await notes.ClickAsync();
        await Assertions.Expect(dimensions).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(notes).ToHaveAttributeAsync("aria-pressed", "true");

        await page.GetByTestId("control-toggle-group-orientation").SelectOptionAsync("Vertical");
        await page.GetByTestId("control-toggle-group-spacing").SelectOptionAsync("0");
        await page.GetByTestId("control-toggle-group-size").SelectOptionAsync("Large");
        await page.GetByTestId("control-toggle-group-invalid").CheckAsync();
        var group = page.GetByTestId("action-toggle-group");
        await Assertions.Expect(group).ToHaveAttributeAsync("aria-orientation", "vertical");
        await Assertions.Expect(group).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(group).ToHaveCSSAsync("gap", "0px");
        await Assertions.Expect(notes).ToHaveCSSAsync("height", "40px");

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("Multiple=\"false\"");
        await Assertions.Expect(source).ToContainTextAsync("Orientation=\"ShadcnToggleGroupOrientation.Vertical\"");
        await Assertions.Expect(source).ToContainTextAsync("aria-invalid=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("Final inspection note");
    }

    [Fact]
    public async Task ConnectedGroupKeepsLogicalRtlGeometryKeyboardOrderAndMobileContainment()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/toggle-group").ToString());
        await page.GetByTestId("documentation-theme-toggle").ClickAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var dimensions = page.GetByTestId("toggle-group-dimensions");
        var notes = page.GetByTestId("toggle-group-notes");
        await dimensions.FocusAsync();
        await page.Keyboard.PressAsync("ArrowLeft");
        await Assertions.Expect(notes).ToBeFocusedAsync();

        await page.GetByTestId("control-toggle-group-orientation").SelectOptionAsync("Vertical");
        await dimensions.FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Assertions.Expect(notes).ToBeFocusedAsync();

        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        await Assertions.Expect(page.GetByTestId("action-toggle-group")).ToHaveCSSAsync("flex-direction", "column");
        await Assertions.Expect(page.GetByTestId("toggle-group-dossier")).ToBeVisibleAsync();
    }
}
