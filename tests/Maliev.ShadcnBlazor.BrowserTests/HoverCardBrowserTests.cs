using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class HoverCardBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task HoverCardBridgesPointerFocusAndContentAndClosesPredictably()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/hover-card").ToString());
        await page.GetByTestId("hover-card-dossier-preview").WaitForAsync();

        var trigger = page.Locator("#preview [data-slot='hover-card-trigger']");
        var content = page.Locator("#preview [data-slot='hover-card-content']");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        var triggerBeforeOpen = await trigger.BoundingBoxAsync();

        await trigger.FocusAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();
        await Assertions.Expect(content).ToHaveAttributeAsync("data-positioned", "true");
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        var contentId = await content.GetAttributeAsync("id");
        Assert.False(string.IsNullOrWhiteSpace(contentId));
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-controls", contentId!);
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-describedby", contentId!);
        var triggerAfterOpen = await trigger.BoundingBoxAsync();
        Assert.NotNull(triggerBeforeOpen);
        Assert.NotNull(triggerAfterOpen);
        Assert.InRange(Math.Abs(triggerAfterOpen!.X - triggerBeforeOpen!.X), 0, 1);
        Assert.InRange(Math.Abs(triggerAfterOpen.Y - triggerBeforeOpen.Y), 0, 1);

        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).ToBeFocusedAsync();
        await page.WaitForTimeoutAsync(700);
        await Assertions.Expect(content).ToHaveCountAsync(0);

        await page.EvaluateAsync("document.activeElement?.blur()");
        await page.GetByTestId("control-hover-card-fast").CheckAsync();
        await trigger.HoverAsync();
        await Assertions.Expect(content).ToBeVisibleAsync();
        await content.HoverAsync();
        await page.WaitForTimeoutAsync(350);
        await Assertions.Expect(content).ToBeVisibleAsync();
        await trigger.HoverAsync();
        await page.WaitForTimeoutAsync(350);
        await Assertions.Expect(content).ToBeVisibleAsync();

        await page.Locator("#overview").ClickAsync();
        await Assertions.Expect(content).ToHaveCountAsync(0);
        await Assertions.Expect(trigger).Not.ToBeFocusedAsync();
    }

    [Fact]
    public async Task HoverCardControlsUpdateTheExactCompleteRazorSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/hover-card").ToString());
        await page.GetByTestId("hover-card-dossier-preview").WaitForAsync();

        var source = page.Locator("#preview [data-slot='code-block']").First;
        await Assertions.Expect(source).ToContainTextAsync("OpenDelay=\"@TimeSpan.FromMilliseconds(600)\"");
        await Assertions.Expect(source).ToContainTextAsync("Side=\"ShadcnOverlaySide.Bottom\"");
        await Assertions.Expect(source).ToContainTextAsync("<ShadcnAvatar");
        await Assertions.Expect(source).ToContainTextAsync("@code {");

        await page.GetByTestId("control-hover-card-fast").CheckAsync();
        await page.GetByTestId("control-hover-card-top").CheckAsync();
        await Assertions.Expect(page.GetByTestId("hover-card-dossier-preview")).ToHaveAttributeAsync("data-fast", "true");
        await Assertions.Expect(page.GetByTestId("hover-card-dossier-preview")).ToHaveAttributeAsync("data-top", "true");
        await Assertions.Expect(source).ToContainTextAsync("OpenDelay=\"@TimeSpan.FromMilliseconds(100)\"");
        await Assertions.Expect(source).ToContainTextAsync("Side=\"ShadcnOverlaySide.Top\"");
        Assert.DoesNotContain("...", await source.InnerTextAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HoverCardRemainsCollisionSafeInNarrowRtlForcedColors()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ColorScheme = ColorScheme.Dark,
            ForcedColors = ForcedColors.Active,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/hover-card").ToString());
        await page.GetByTestId("hover-card-dossier-preview").WaitForAsync();
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var trigger = page.Locator("#preview [data-slot='hover-card-trigger']");
        await trigger.FocusAsync();
        var content = page.Locator("#preview [data-slot='hover-card-content']");
        await Assertions.Expect(content).ToBeVisibleAsync();
        var box = await content.BoundingBoxAsync();
        Assert.NotNull(box);
        Assert.InRange(box!.X, 0, 390 - box.Width);
        Assert.InRange(box.X + box.Width, box.Width, 390);
        await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
        Assert.Equal("rtl", await content.EvaluateAsync<string>("element => getComputedStyle(element).direction"));
        Assert.Equal("none", await content.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        Assert.Equal("solid", await content.EvaluateAsync<string>("element => getComputedStyle(element).borderTopStyle"));
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
