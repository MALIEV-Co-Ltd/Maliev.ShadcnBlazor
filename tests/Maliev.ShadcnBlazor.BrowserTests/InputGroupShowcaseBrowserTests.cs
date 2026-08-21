using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class InputGroupShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task InputGroupDossierIsCompactInteractiveAndKeepsSourceInSync()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/input-group").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var demo = page.GetByTestId("forms-dossier-input-group-demo");
        var group = page.GetByTestId("forms-dossier-input-group");
        var input = group.Locator("[data-slot='input-group-control']");
        var addon = group.Locator("[data-slot='input-group-addon']");
        var reset = page.GetByTestId("input-group-reset");
        var subtotal = page.GetByTestId("input-group-subtotal");

        var demoBox = await demo.BoundingBoxAsync();
        var canvasBox = await page.Locator("#preview .component-preview__canvas").BoundingBoxAsync();
        Assert.NotNull(demoBox);
        Assert.NotNull(canvasBox);
        Assert.InRange(demoBox!.Width, 300, 368);
        Assert.InRange(Math.Abs((demoBox.X + demoBox.Width / 2) - (canvasBox!.X + canvasBox.Width / 2)), 0, 2);

        await addon.Locator("[data-slot='input-group-text']").ClickAsync();
        await Assertions.Expect(input).ToBeFocusedAsync();
        await input.FillAsync("1000");
        await Assertions.Expect(subtotal).ToContainTextAsync("12,000");
        await reset.ClickAsync();
        await Assertions.Expect(input).ToHaveValueAsync("1250");
        await Assertions.Expect(subtotal).ToContainTextAsync("15,000");

        await page.GetByTestId("control-input-group-invalid").CheckAsync();
        await Assertions.Expect(group).ToHaveAttributeAsync("aria-invalid", "true");
        await page.ChooseOptionAsync("control-input-group-alignment", "BlockEnd");
        await Assertions.Expect(addon).ToHaveAttributeAsync("data-align", "block-end");
        var source = page.Locator("#preview .component-code pre").First;
        await Assertions.Expect(source).ToContainTextAsync("ShadcnInputGroupAlignment.BlockEnd");
        await Assertions.Expect(source).ToContainTextAsync("Invalid=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("ResetUnitPrice");
    }

    [Theory]
    [InlineData(320, ForcedColors.Active, "light", "rtl")]
    [InlineData(390, ForcedColors.None, "dark", "rtl")]
    public async Task InputGroupDossierRemainsCenteredWithoutOverflowInResponsiveThemeModes(int width, ForcedColors forcedColors, string theme, string direction)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = theme == "dark" ? ColorScheme.Dark : ColorScheme.Light,
            ForcedColors = forcedColors
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/input-group?theme={theme}&dir={direction}").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var demo = page.GetByTestId("forms-dossier-input-group-demo");
        var group = page.GetByTestId("forms-dossier-input-group");
        await demo.EvaluateAsync("(element, direction) => { const scope = element.closest('.shadcn-scope') ?? document.documentElement; scope.setAttribute('dir', direction); }", direction);
        var demoBox = await demo.BoundingBoxAsync();
        var canvasBox = await page.Locator("#preview .component-preview__canvas").BoundingBoxAsync();
        Assert.NotNull(demoBox);
        Assert.NotNull(canvasBox);
        Assert.True(demoBox!.Width <= canvasBox!.Width);
        Assert.InRange(Math.Abs((demoBox.X + demoBox.Width / 2) - (canvasBox.X + canvasBox.Width / 2)), 0, 2);
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        await Assertions.Expect(group).ToHaveCSSAsync("direction", direction);
        await Assertions.Expect(page.GetByTestId("input-group-reset")).ToHaveAttributeAsync("aria-label", "Reset unit price");
    }
}
