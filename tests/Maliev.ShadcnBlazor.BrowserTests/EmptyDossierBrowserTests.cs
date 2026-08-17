using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class EmptyDossierBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    public static TheoryData<int, int, string, string> Viewports => new()
    {
        { 1280, 900, "light", "ltr" },
        { 390, 844, "dark", "rtl" }
    };

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task EmptyDossierIsCenteredResponsiveKeyboardOperableAndThemeSafe(
        int width,
        int height,
        string theme,
        string direction)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = width == 390 ? ForcedColors.Active : ForcedColors.None
        });
        var page = await context.NewPageAsync();
        var errors = new List<string>();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/empty?theme={theme}&dir={direction}").ToString());

        var dossier = page.Locator("#preview .showcase-empty-dossier");
        await Assertions.Expect(dossier).ToBeVisibleAsync();
        await Assertions.Expect(dossier.Locator("[data-slot='empty']")).ToBeVisibleAsync();
        await Assertions.Expect(dossier.Locator("[data-slot='button']")).ToHaveCountAsync(2);

        var create = dossier.Locator("[data-empty-action='create']");
        await create.FocusAsync();
        await Assertions.Expect(create).ToBeFocusedAsync();
        await create.PressAsync("Enter");
        await Assertions.Expect(dossier.GetByRole(AriaRole.Status)).ToHaveTextAsync("A new project workspace is ready.");

        var import = dossier.Locator("[data-empty-action='import']");
        await import.FocusAsync();
        await import.PressAsync("Space");
        await Assertions.Expect(dossier.GetByRole(AriaRole.Status)).ToHaveTextAsync("Project import opened. Select a project archive to continue.");

        var box = await dossier.BoundingBoxAsync();
        var canvasBox = await page.Locator("#preview .component-preview__canvas").BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.NotNull(canvasBox);
        Assert.InRange(Math.Abs((box!.X + box.Width / 2) - (canvasBox!.X + canvasBox.Width / 2)), 0, 2);
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);
        Assert.Empty(errors);
    }
}
