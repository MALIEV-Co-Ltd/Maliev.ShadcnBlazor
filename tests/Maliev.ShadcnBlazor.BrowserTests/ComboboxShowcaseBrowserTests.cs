using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ComboboxShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task ComboboxDossierWorksWithPointerKeyboardClearAndInvalidStates()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/combobox").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var input = page.GetByTestId("forms-dossier-combobox");
        var root = input.Locator("xpath=ancestor::*[@data-slot='combobox'][1]");
        var trigger = root.Locator("[data-slot='combobox-trigger']");

        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "false");
        await trigger.ClickAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "true");

        var content = root.Locator("[data-slot='combobox-content']");
        var contentBox = await content.BoundingBoxAsync();
        Assert.NotNull(contentBox);
        Assert.InRange(contentBox!.X, 0, 390 - contentBox.Width);
        Assert.InRange(contentBox.X + contentBox.Width, contentBox.Width, 390);

        await input.FillAsync("stain");
        await Assertions.Expect(root.Locator("[data-slot='combobox-item']")).ToHaveCountAsync(1);
        await input.PressAsync("ArrowDown");
        await input.PressAsync("Enter");
        await Assertions.Expect(input).ToHaveValueAsync("Stainless 316L");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "false");

        await trigger.ClickAsync();
        await page.Locator("#preview [data-slot='combobox-item'][data-value='peek']").ClickAsync();
        await Assertions.Expect(input).ToHaveValueAsync("PEEK");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "false");

        await input.ClickAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-expanded", "true");
        var clear = root.Locator("[data-slot='combobox-clear']");
        await Assertions.Expect(clear).ToBeVisibleAsync();
        await clear.ClickAsync();
        await Assertions.Expect(input).ToHaveValueAsync(string.Empty);

        await page.GetByTestId("control-combobox-invalid").CheckAsync();
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(root.Locator("[data-slot='input-group']")).Not.ToHaveCSSAsync("box-shadow", "none");
        await page.GetByTestId("control-combobox-multiple").CheckAsync();
        await Assertions.Expect(root.Locator("[data-slot='combobox-list']")).ToHaveAttributeAsync("aria-multiselectable", "true");
        await Assertions.Expect(root.Locator("[data-slot='combobox-chip']")).ToHaveCountAsync(2);

        var fieldBox = await root.Locator("[data-slot='input-group']").BoundingBoxAsync();
        var clearBox = await clear.BoundingBoxAsync();
        Assert.NotNull(fieldBox);
        Assert.NotNull(clearBox);
        Assert.InRange(clearBox!.X, fieldBox!.X, fieldBox.X + fieldBox.Width);
        Assert.InRange(clearBox.Y, fieldBox.Y, fieldBox.Y + fieldBox.Height);

        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
        var rtlContentBox = await root.Locator("[data-slot='combobox-content']").BoundingBoxAsync();
        Assert.NotNull(rtlContentBox);
        Assert.InRange(rtlContentBox!.X, 0, 390 - rtlContentBox.Width);
    }
}
