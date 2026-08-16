using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ButtonShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task ButtonDossierShowsAllTreatmentsAndSupportsDirectActions()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/button").ToString());

        var dossier = page.GetByTestId("button-dossier-preview");
        await dossier.WaitForAsync();
        await Assertions.Expect(dossier.Locator("[data-testid^='button-variant-']")).ToHaveCountAsync(6);
        await Assertions.Expect(dossier.Locator(".showcase-button-dossier__sizes [data-slot='button']")).ToHaveCountAsync(4);

        var defaultButton = dossier.GetByTestId("button-variant-default");
        await Assertions.Expect(defaultButton).ToHaveCSSAsync("cursor", "pointer");
        await defaultButton.ClickAsync();
        await Assertions.Expect(dossier.GetByTestId("button-last-action")).ToContainTextAsync("Save changes pressed");

        await page.GetByTestId("control-button-disabled").CheckAsync();
        await Assertions.Expect(defaultButton).ToBeDisabledAsync();
        await Assertions.Expect(dossier.GetByTestId("button-last-action")).ToContainTextAsync("Save changes pressed");
    }

    [Fact]
    public async Task ButtonDossierSourceContainsTheRenderedVariantsAndSizes()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/button").ToString());

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await source.WaitForAsync();
        var sourceText = await source.InnerTextAsync();
        foreach (var token in new[]
        {
            "ShadcnButtonVariant.Default",
            "ShadcnButtonVariant.Destructive",
            "ShadcnButtonVariant.Outline",
            "ShadcnButtonVariant.Secondary",
            "ShadcnButtonVariant.Ghost",
            "ShadcnButtonVariant.Link",
            "ShadcnButtonSize.ExtraSmall",
            "ShadcnButtonSize.Small",
            "ShadcnButtonSize.Default",
            "ShadcnButtonSize.Large",
            "@bind=\"disabled\"",
            "aria-live=\"polite\""
        })
        {
            Assert.Contains(token, sourceText, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("...", sourceText, StringComparison.Ordinal);
    }
}
