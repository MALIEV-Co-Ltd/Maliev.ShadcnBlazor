using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class InputShowcaseBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task ContextualInputDossierBindsStateAndKeepsSourceInSync()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/input").ToString());
        var preview = page.GetByTestId("input-dossier-preview");
        var key = page.GetByTestId("forms-dossier-input");
        var source = page.Locator("#preview .component-code pre");

        await Assertions.Expect(preview.Locator("[data-slot='card']")).ToBeVisibleAsync();
        await Assertions.Expect(preview.Locator("input[data-slot='input']")).ToHaveCountAsync(2);
        await Assertions.Expect(key).ToHaveAttributeAsync("type", "password");
        await key.FillAsync("api_live_demo_updated");
        await page.GetByTestId("forms-dossier-save").ClickAsync();
        await Assertions.Expect(page.GetByTestId("forms-dossier-status")).ToHaveTextAsync("Credentials saved for this demo.");

        await page.GetByTestId("control-input-masked").UncheckAsync();
        await Assertions.Expect(key).ToHaveAttributeAsync("type", "text");
        await Assertions.Expect(source).ToContainTextAsync("Type=\"text\"");

        await page.GetByTestId("control-input-invalid").CheckAsync();
        await Assertions.Expect(key).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(key).ToHaveAttributeAsync("aria-describedby", "integration-key-help integration-key-error");
        await Assertions.Expect(preview.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Assertions.Expect(source).ToContainTextAsync("Invalid=\"true\"");
    }

    [Fact]
    public async Task InputDossierRemainsUsableOnMobileDarkRtlAndForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            HasTouch = true
        });
        var page = await context.NewPageAsync();
        await page.EmulateMediaAsync(new() { ForcedColors = ForcedColors.Active, ReducedMotion = ReducedMotion.Reduce });

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/input").ToString());
        await page.GetByTestId("input-dossier-preview").EvaluateAsync("element => element.setAttribute('dir', 'rtl')");

        var key = page.GetByTestId("forms-dossier-input");
        await key.FocusAsync();
        await Assertions.Expect(key).ToBeFocusedAsync();
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);
        Assert.Equal("none", await page.GetByTestId("forms-dossier-file").EvaluateAsync<string>("element => getComputedStyle(element, '::file-selector-button').borderTopStyle"));
        Assert.True(await key.EvaluateAsync<bool>("element => { const input = element.getBoundingClientRect(); const card = element.closest('[data-slot=card]').getBoundingClientRect(); return input.left >= card.left && input.right <= card.right; }"));
    }
}
