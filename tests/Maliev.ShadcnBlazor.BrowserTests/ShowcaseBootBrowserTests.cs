using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ShowcaseBootBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task FailedRuntimeLoadOffersAnAccessibleRetryThatRecoversTheRequestedRoute()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        var failRuntimeOnce = true;
        await page.RouteAsync("**/_framework/blazor.webassembly.js*", async route =>
        {
            if (failRuntimeOnce)
            {
                failRuntimeOnce = false;
                await route.AbortAsync();
                return;
            }

            await route.ContinueAsync();
        });

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        var boot = page.Locator(".showcase-boot");
        await Assertions.Expect(boot).ToHaveAttributeAsync("data-state", "error", new() { Timeout = 15_000 });
        await Assertions.Expect(boot).ToHaveAttributeAsync("aria-busy", "false");
        await Assertions.Expect(boot).ToHaveAttributeAsync("role", "alert");
        await Assertions.Expect(boot).ToContainTextAsync("Showcase could not start");

        var retry = page.GetByRole(AriaRole.Button, new() { Name = "Retry loading" });
        await Assertions.Expect(retry).ToBeVisibleAsync();
        var target = await retry.BoundingBoxAsync();
        Assert.NotNull(target);
        Assert.True(target!.Width >= 44 && target.Height >= 44);

        await retry.ClickAsync();
        await Assertions.Expect(page.Locator("#preview")).ToBeVisibleAsync(new() { Timeout = 30_000 });
        await Assertions.Expect(page.Locator(".showcase-boot")).ToHaveCountAsync(0);
    }
}
