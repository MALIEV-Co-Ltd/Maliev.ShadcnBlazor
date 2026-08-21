using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class AlertDialogBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task AlertDialogStartsClosedTrapsFocusAndRestoresTheTrigger()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/alert-dialog").ToString());

        var preview = page.GetByTestId("alert-dialog-dossier");
        var trigger = preview.GetByRole(AriaRole.Button, new() { Name = "Delete saved quotation" });
        var dialog = page.GetByRole(AriaRole.Alertdialog);
        await Assertions.Expect(trigger).ToBeVisibleAsync();
        await Assertions.Expect(dialog).ToHaveCountAsync(0);

        await trigger.ClickAsync();
        await Assertions.Expect(dialog).ToBeVisibleAsync();
        await Assertions.Expect(dialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" })).ToBeFocusedAsync();
        await Assertions.Expect(dialog).ToHaveAttributeAsync("aria-modal", "true");
        await page.Keyboard.PressAsync("Shift+Tab");
        await Assertions.Expect(dialog.GetByRole(AriaRole.Button, new() { Name = "Delete quotation" })).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Tab");
        await Assertions.Expect(dialog.GetByRole(AriaRole.Button, new() { Name = "Cancel" })).ToBeFocusedAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(dialog).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
        await trigger.ClickAsync();
        await dialog.GetByRole(AriaRole.Button, new() { Name = "Delete quotation" }).ClickAsync();
        await Assertions.Expect(dialog).ToHaveCountAsync(0);
        await Assertions.Expect(preview.GetByRole(AriaRole.Status)).ToContainTextAsync("Quotation deleted");
    }

    [Fact]
    public async Task AlertDialogRemainsUsableAtMobileWidthInDarkRtlMode()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/alert-dialog").ToString());
        await page.EvaluateAsync("document.documentElement.setAttribute('dir', 'rtl')");
        await page.GetByTestId("alert-dialog-dossier").GetByRole(AriaRole.Button, new() { Name = "Delete saved quotation" }).ClickAsync();
        var dialog = page.GetByRole(AriaRole.Alertdialog);
        await Assertions.Expect(dialog).ToBeVisibleAsync();
        var box = await dialog.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.True(box!.X >= 8, $"Dialog escaped the inline-start viewport edge at {box.X}px.");
        Assert.True(box.X + box.Width <= 382, $"Dialog escaped the inline-end viewport edge at {box.X + box.Width}px.");
        await Assertions.Expect(dialog).ToHaveCSSAsync("animation-name", "none");
    }
}
