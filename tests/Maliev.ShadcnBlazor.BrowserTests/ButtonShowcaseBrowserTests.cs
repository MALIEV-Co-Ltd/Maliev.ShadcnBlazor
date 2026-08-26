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
        await Assertions.Expect(dossier.Locator(".showcase-button-dossier__icon-sizes [data-slot='button']")).ToHaveCountAsync(4);
        await Assertions.Expect(dossier.GetByTestId("button-variant-link")).ToHaveAttributeAsync("href", "#usage");

        var defaultButton = dossier.GetByTestId("button-variant-default");
        await Assertions.Expect(defaultButton).ToHaveCSSAsync("cursor", "pointer");
        await defaultButton.ClickAsync();
        await Assertions.Expect(dossier.GetByTestId("button-last-action")).ToContainTextAsync("Save changes pressed");

        await page.GetByTestId("control-button-disabled").CheckAsync();
        await Assertions.Expect(defaultButton).ToBeDisabledAsync();
        await Assertions.Expect(dossier.Locator("button[data-slot='button']:disabled")).ToHaveCountAsync(13);
        await Assertions.Expect(dossier.GetByTestId("button-variant-link")).ToHaveAttributeAsync("aria-disabled", "true");
        await Assertions.Expect(dossier.GetByTestId("button-variant-link")).Not.ToHaveAttributeAsync("href", "#usage");
        await Assertions.Expect(dossier.GetByTestId("button-last-action")).ToContainTextAsync("Save changes pressed");

        var preview = page.GetByTestId("component-preview").First;
        var sourceDisclosure = preview.Locator("details[data-testid='example-source']");
        await Assertions.Expect(sourceDisclosure).Not.ToHaveAttributeAsync("open", "");
        await sourceDisclosure.Locator("summary").ClickAsync();
        await Assertions.Expect(sourceDisclosure).ToHaveAttributeAsync("open", "");
        var source = sourceDisclosure.Locator("[data-slot='code-block']");
        await Assertions.Expect(source).ToBeVisibleAsync();
        var updatedSource = await source.InnerTextAsync();
        Assert.Contains("Disabled=\"true\"", updatedSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Disabled=\"false\"", updatedSource, StringComparison.Ordinal);
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

        var preview = page.GetByTestId("component-preview").First;
        var sourceDisclosure = preview.Locator("details[data-testid='example-source']");
        await Assertions.Expect(sourceDisclosure).Not.ToHaveAttributeAsync("open", "");
        await sourceDisclosure.Locator("summary").ClickAsync();
        await Assertions.Expect(sourceDisclosure).ToHaveAttributeAsync("open", "");
        var source = sourceDisclosure.Locator("[data-slot='code-block']");
        await Assertions.Expect(source).ToBeVisibleAsync();
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
            "ShadcnButtonSize.IconExtraSmall",
            "ShadcnButtonSize.IconSmall",
            "ShadcnButtonSize.Icon",
            "ShadcnButtonSize.IconLarge",
            "Href=\"#usage\"",
            "Disabled=\"false\"",
            "aria-live=\"polite\""
        })
        {
            Assert.Contains(token, sourceText, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("...", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("<input type=\"checkbox\"", sourceText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ButtonDossierRemainsUsableOnMobileDarkRtlAndForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/button?theme=dark&dir=rtl").ToString());
        var dossier = page.GetByTestId("button-dossier-preview");
        await dossier.WaitForAsync();
        await page.GetByTestId("component-preview-canvas").EvaluateAsync("element => element.setAttribute('dir', 'rtl')");
        await Assertions.Expect(dossier).ToHaveCSSAsync("direction", "rtl");
        await Assertions.Expect(dossier.Locator(".showcase-button-dossier__icon-sizes [data-slot='button']")).ToHaveCountAsync(4);

        var overflows = await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(overflows);

        var iconButton = dossier.GetByTestId("button-icon-iconextrasmall");
        await iconButton.FocusAsync();
        await Assertions.Expect(iconButton).ToBeFocusedAsync();
        await iconButton.ClickAsync();
        await Assertions.Expect(dossier.GetByTestId("button-last-action")).ToContainTextAsync("Save drawing");
    }
}
