using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class FieldDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task PaymentFieldCompositionIsInteractiveAccessibleAndResponsive()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/field?theme=dark&dir=rtl").ToString());
        await page.EvaluateAsync("document.documentElement.dir='rtl'; document.querySelector('.shadcn-scope')?.setAttribute('dir','rtl')");

        var preview = page.GetByTestId("field-dossier-preview");
        await preview.WaitForAsync();
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));

        var cardholder = preview.Locator("#field-cardholder");
        await cardholder.FillAsync("Niran Sutham");
        await preview.Locator("#field-comments").FillAsync("Use the purchasing card for this order.");
        await preview.Locator("#field-same-address").UncheckAsync();
        await preview.GetByRole(AriaRole.Button, new() { Name = "Review payment" }).ClickAsync();
        await Assertions.Expect(preview.GetByRole(AriaRole.Status)).ToContainTextAsync("Niran Sutham");

        await page.GetByTestId("control-field-orientation").SelectOptionAsync("Horizontal");
        await page.GetByTestId("control-field-invalid").CheckAsync();
        await Assertions.Expect(preview.Locator("#field-card-number")).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(preview.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("#preview .component-code pre")).ToContainTextAsync("Orientation=\"ShadcnFieldOrientation.Horizontal\"");
        await Assertions.Expect(page.Locator("#preview .component-code pre")).ToContainTextAsync("Invalid=\"true\"");

        await cardholder.FocusAsync();
        Assert.Equal("field-cardholder", await page.EvaluateAsync<string>("document.activeElement?.id ?? ''"));

        var axe = await preview.RunAxe();
        Assert.Empty(axe.Violations);
    }

    [Fact]
    public async Task PaymentFieldCompositionRemainsUsableInForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/field").ToString());

        var preview = page.GetByTestId("field-dossier-preview");
        await preview.WaitForAsync();
        var cardholder = preview.Locator("#field-cardholder");
        await cardholder.FocusAsync();

        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        Assert.Equal("field-cardholder", await page.EvaluateAsync<string>("document.activeElement?.id ?? ''"));
        await Assertions.Expect(preview.GetByRole(AriaRole.Button, new() { Name = "Review payment" })).ToBeVisibleAsync();
        await Assertions.Expect(preview.Locator("#field-same-address")).ToBeCheckedAsync();
    }
}
