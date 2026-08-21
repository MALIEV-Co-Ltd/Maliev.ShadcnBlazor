using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ToastShowcaseBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task ToastDossierCentersTheTriggerAndCompletesTheLocalizedUndoFlow()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/toast").ToString());

        var preview = page.GetByTestId("toast-dossier-preview");
        var trigger = preview.GetByRole(AriaRole.Button, new() { Name = "บันทึกใบงาน — Save work order" });
        await preview.WaitForAsync();
        var previewBox = await preview.BoundingBoxAsync();
        var triggerBox = await trigger.BoundingBoxAsync();
        Assert.NotNull(previewBox);
        Assert.NotNull(triggerBox);
        Assert.InRange(Math.Abs((triggerBox!.X + triggerBox.Width / 2) - (previewBox!.X + previewBox.Width / 2)), 0, 1);

        await trigger.ClickAsync();
        var toast = preview.Locator("[data-slot='toast']").Last;
        await Assertions.Expect(toast).ToContainTextAsync("บันทึกใบงานแล้ว — Work order saved");
        await Assertions.Expect(toast).ToContainTextAsync("ใบงาน WO-2048 พร้อมส่งให้ฝ่ายผลิต");
        await Assertions.Expect(toast.Locator("[data-slot='toast-icon'] svg")).ToHaveCountAsync(1);
        await Assertions.Expect(toast.Locator("[data-slot='toast-close'] svg")).ToHaveCountAsync(1);

        await toast.Locator("[data-slot='toast-action']").ClickAsync();
        await Assertions.Expect(preview.Locator("[data-slot='toast']").Last).ToContainTextAsync("ยกเลิกการบันทึกแล้ว — Save undone");

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("private void Show()");
        await Assertions.Expect(source).ToContainTextAsync("private Task UndoAsync()");
        await Assertions.Expect(source).ToContainTextAsync("Action: UndoAsync");
    }

    [Fact]
    public async Task ToastDossierTracksControlsInSourceAndRemainsUsableInMobileRtlForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/toast").ToString());
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();
        await page.GetByTestId("control-toast-limit").FillAsync("1");
        await page.GetByTestId("control-toast-limit").PressAsync("Tab");
        await page.GetByTestId("control-toast-start").CheckAsync();
        await page.GetByTestId("control-toast-reduced").CheckAsync();
        await page.ChooseOptionAsync("control-toast-type", "Error");
        await page.ChooseOptionAsync("control-toast-priority", "High");

        var preview = page.GetByTestId("toast-dossier-preview");
        await preview.GetByRole(AriaRole.Button, new() { Name = "บันทึกใบงาน — Save work order" }).ClickAsync();
        var viewport = preview.Locator("[data-slot='toast-viewport']");
        var toast = viewport.Locator("[data-slot='toast']");
        await Assertions.Expect(viewport).ToHaveAttributeAsync("data-placement", "bottom-start");
        await Assertions.Expect(viewport).ToHaveAttributeAsync("data-reduced-motion", "true");
        await Assertions.Expect(toast).ToHaveAttributeAsync("data-type", "error");
        await Assertions.Expect(toast).ToHaveAttributeAsync("data-priority", "high");
        Assert.Equal("none", await toast.EvaluateAsync<string>("element => getComputedStyle(element).transitionProperty"));
        Assert.Equal("1px", await toast.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.InRange(await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)"), 0, 1);

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("MaximumVisible=\"1\"");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnToastPlacement.BottomStart");
        await Assertions.Expect(source).ToContainTextAsync("ReducedMotion=\"true\"");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnToastType.Error");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnToastPriority.High");

        await page.Keyboard.PressAsync("F6");
        await Assertions.Expect(viewport).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(toast).ToHaveCountAsync(0);
    }
}
