using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class TooltipBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task TooltipOpensFromHoverAndFocusClosesOnEscapeAndRestoresFocus()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/tooltip").ToString());

        var trigger = page.Locator("#preview [data-slot='tooltip-trigger']");
        await trigger.WaitForAsync();
        await trigger.HoverAsync();
        var content = page.Locator("#preview [data-slot='tooltip-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        await Assertions.Expect(content).ToContainTextAsync("Save quotation");
        var triggerBox = await trigger.BoundingBoxAsync();
        var contentBox = await content.BoundingBoxAsync();
        Assert.NotNull(triggerBox);
        Assert.NotNull(contentBox);
        Assert.True(contentBox!.Width <= 320, $"Tooltip should remain compact but was {contentBox.Width}px wide.");
        Assert.True(contentBox.Height < 120, $"Tooltip should remain compact but was {contentBox.Height}px tall.");

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
        await page.WaitForTimeoutAsync(350);
        await Assertions.Expect(content).ToHaveCountAsync(0);

        await page.EvaluateAsync("document.activeElement?.blur()");
        await trigger.FocusAsync();
        await Assertions.Expect(trigger).ToBeFocusedAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
    }

    [Fact]
    public async Task TooltipSourceIsCompleteAndDoesNotUseAbbreviatedPlaceholder()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/tooltip").ToString());

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await source.WaitForAsync();
        var sourceText = await source.InnerTextAsync();
        Assert.Contains("<ShadcnTooltipProvider", sourceText, StringComparison.Ordinal);
        Assert.Contains("OpenDelay=\"@(TimeSpan.FromMilliseconds(200))\"", sourceText, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTooltipTrigger>Save</ShadcnTooltipTrigger>", sourceText, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTooltipContent", sourceText, StringComparison.Ordinal);
        Assert.Contains("Save quotation", sourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("...", sourceText, StringComparison.Ordinal);
    }
}
