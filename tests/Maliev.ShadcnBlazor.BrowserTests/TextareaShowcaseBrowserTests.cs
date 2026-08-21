using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class TextareaShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task TextareaRowsValidationTypingAndSourceStaySynchronized()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/textarea").ToString());

        var dossier = page.GetByTestId("textarea-dossier-preview");
        var textarea = page.GetByTestId("forms-dossier-textarea");
        await dossier.WaitForAsync();
        await Assertions.Expect(textarea).ToHaveAttributeAsync("rows", "3");
        var initialHeight = (await textarea.BoundingBoxAsync())!.Height;

        const string note = "Inspect every critical edge.";
        await textarea.FillAsync(note);
        await Assertions.Expect(dossier.Locator("[aria-live='polite']")).ToContainTextAsync(note.Length.ToString());

        await page.ChooseOptionAsync("control-textarea-rows", "5");
        await Assertions.Expect(textarea).ToHaveAttributeAsync("rows", "5");
        Assert.True((await textarea.BoundingBoxAsync())!.Height > initialHeight + 10);
        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("Rows=\"5\"");

        await page.GetByTestId("control-textarea-invalid").CheckAsync();
        await Assertions.Expect(textarea).ToHaveAttributeAsync("aria-invalid", "true");
        await Assertions.Expect(textarea).ToHaveAttributeAsync("aria-describedby", "manufacturing-notes-description manufacturing-notes-error");
        await Assertions.Expect(dossier.GetByRole(AriaRole.Alert)).ToBeVisibleAsync();
        await Assertions.Expect(source).ToContainTextAsync("Invalid=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("Add the critical manufacturing instructions");

        await textarea.FocusAsync();
        Assert.NotEqual("none", await textarea.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
    }

    [Fact]
    public async Task TextareaDossierRemainsContainedInDarkRtlForcedColorsAtPhoneWidth()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 320, Height = 568 },
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce,
            HasTouch = true
        });
        var page = await context.NewPageAsync();
        await page.EmulateMediaAsync(new() { ForcedColors = ForcedColors.Active, ReducedMotion = ReducedMotion.Reduce });
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/textarea").ToString());
        await page.GetByTestId("documentation-theme-toggle").EvaluateAsync("element => element.click()");
        await page.GetByTestId("documentation-direction-toggle").EvaluateAsync("element => element.click()");

        var dossier = page.GetByTestId("textarea-dossier-preview");
        var textarea = page.GetByTestId("forms-dossier-textarea");
        await dossier.WaitForAsync();
        await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("data-shadcn-theme", "dark");
        await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(textarea).ToHaveAttributeAsync("dir", "auto");
        Assert.InRange(await dossier.EvaluateAsync<double>("element => element.scrollWidth - element.clientWidth"), 0, 1);
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth - document.documentElement.clientWidth"), 0, 1);
        Assert.Equal("solid", await textarea.EvaluateAsync<string>("element => getComputedStyle(element).borderStyle"));
        Assert.Equal("reduce", await page.EvaluateAsync<string>("matchMedia('(prefers-reduced-motion: reduce)').matches ? 'reduce' : 'motion'"));
        Assert.Equal("active", await page.EvaluateAsync<string>("matchMedia('(forced-colors: active)').matches ? 'active' : 'none'"));
    }
}
