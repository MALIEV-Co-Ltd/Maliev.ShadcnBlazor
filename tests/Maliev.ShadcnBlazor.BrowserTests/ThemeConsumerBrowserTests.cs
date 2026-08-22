using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeConsumerBrowserTests(PlaywrightFixture playwright) : IAsyncLifetime
{
    private readonly ThemeConsumerServerFixture _server = new();

    public Task InitializeAsync() => _server.InitializeAsync();

    public Task DisposeAsync() => _server.DisposeAsync();

    [Theory]
    [InlineData(1280, 800, false)]
    [InlineData(390, 844, true)]
    public async Task PackedThemeJourneyRendersAndRemainsOperable(int width, int height, bool forcedColors)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = forcedColors ? ForcedColors.Active : ForcedColors.None
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        await page.GotoAsync(_server.BaseUri.ToString());
        var shell = page.GetByTestId("theme-consumer-shell");
        await shell.WaitForAsync();

        var scope = page.Locator("[data-shadcn-scope]");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-shadcn-theme", "light");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-shadcn-reduced-motion", "system");
        Assert.Equal("ltr", await scope.GetAttributeAsync("dir"));
        Assert.Equal("oklch(0.205 0 0)", await scope.EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-primary').trim()"));

        await page.GetByTestId("confirm-theme").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-consumer-status"))
            .ToHaveTextAsync("Theme loaded and component interaction confirmed.");

        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        var axe = await shell.RunAxe();
        Assert.DoesNotContain(axe.Violations, violation => violation.Impact is "serious" or "critical");
        Assert.Empty(errors);
    }
}
