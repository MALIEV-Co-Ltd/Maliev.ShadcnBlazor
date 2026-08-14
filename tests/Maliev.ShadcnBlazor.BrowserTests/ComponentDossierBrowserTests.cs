using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ComponentDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task SemanticDossierUpdatesRealPreviewCopiesSourceAndListsPublicApi()
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            Permissions = ["clipboard-read", "clipboard-write"]
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/aspect-ratio").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var ratio = page.GetByTestId("control-aspect-ratio");
        await ratio.SelectOptionAsync("1");
        await Assertions.Expect(page.Locator("[data-slot='aspect-ratio']")).ToHaveAttributeAsync("style", new Regex("aspect-ratio: 1(?:;|$)"));

        await page.GetByTestId("copy-source").ClickAsync();
        await Assertions.Expect(page.Locator(".component-code__announcement")).ToHaveTextAsync("Source copied to clipboard.");
        var copied = await page.EvaluateAsync<string>("navigator.clipboard.readText()");
        Assert.Contains("<ShadcnAspectRatio", copied, StringComparison.Ordinal);

        var api = page.GetByTestId("component-api");
        await Assertions.Expect(api.GetByTestId("api-row")).ToHaveCountAsync(5);
        await Assertions.Expect(api.Locator("[data-parameter='Ratio']")).ToContainTextAsync("Must be positive and finite.");
        var evidenceRows = page.GetByTestId("evidence-row");
        await Assertions.Expect(evidenceRows).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='true']")).ToHaveCountAsync(6);
        await Assertions.Expect(page.Locator("[data-evidence='integration']")).ToHaveAttributeAsync("data-complete", "false");
        Assert.Empty(errors);
    }

    [Fact]
    public async Task SemanticDossierControlsDriveRealAccessibleDomState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/direction").ToString());
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "rtl");
        await page.GetByTestId("control-direction").SelectOptionAsync("LeftToRight");
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "ltr");
        await page.GetByTestId("control-direction").SelectOptionAsync("Inherited");
        await Assertions.Expect(page.GetByTestId("direction-example")).ToHaveAttributeAsync("dir", "rtl");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/field").ToString());
        var input = page.Locator("#dossier-field-input");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "dossier-field-help dossier-field-error");
        await page.GetByTestId("control-field-invalid").UncheckAsync();
        await Assertions.Expect(input).Not.ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(input).ToHaveAttributeAsync("aria-describedby", "dossier-field-help");
        await page.GetByTestId("control-field-disabled").CheckAsync();
        await Assertions.Expect(input).ToBeDisabledAsync();
        await Assertions.Expect(page.Locator("[data-slot='field-set']")).ToHaveAttributeAsync("disabled", string.Empty);
        await page.GetByTestId("control-field-legend-variant").SelectOptionAsync("Label");
        await Assertions.Expect(page.Locator("[data-slot='field-legend']")).ToHaveAttributeAsync("data-variant", "label");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/empty").ToString());
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = "Create project" })).ToBeVisibleAsync();
        await page.GetByTestId("control-empty-media-variant").SelectOptionAsync("Default");
        await Assertions.Expect(page.Locator("[data-slot='empty-icon']")).ToHaveAttributeAsync("data-variant", "default");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/typography").ToString());
        await page.GetByTestId("control-typeset-tag").SelectOptionAsync("article");
        await page.GetByTestId("control-typography-variant").SelectOptionAsync("H1");
        await Assertions.Expect(page.Locator("article[data-slot='typeset'] h1[data-slot='typography']")).ToHaveCountAsync(1);
    }

    [Theory]
    [InlineData("input")]
    [InlineData("select")]
    public async Task CertifiedFormsDossiersExposePreviewSourceApiAndCompleteEvidence(string slug)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/{slug}").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("planned-component-notice")).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByTestId("component-preview")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("copy-source")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("component-api")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("evidence-row")).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='true']")).ToHaveCountAsync(7);
        await Assertions.Expect(page.Locator("[data-testid='evidence-row'][data-complete='false']")).ToHaveCountAsync(0);
    }
}
