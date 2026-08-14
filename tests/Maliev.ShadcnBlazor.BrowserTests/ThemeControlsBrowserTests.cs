using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeControlsBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task CustomControlsChangeRealMudComponentComputedStylesWithoutChangingDefaults()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.NoPreference
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/mud-inventory").ToString());
        await page.GetByTestId("mud-inventory-fixture").WaitForAsync();

        var button = page.GetByTestId("button-default");
        var inlineCode = await page.Locator("[data-shadcn-scope]").EvaluateHandleAsync("""
            element => {
                const code = document.createElement('code');
                code.className = 'shadcn-typography--inline-code';
                code.dataset.testid = 'theme-inline-code';
                code.textContent = 'theme';
                element.append(code);
                return code;
            }
        """);
        var defaults = await ReadButtonStyles(button);
        Assert.Equal(6, defaults.Gap);
        Assert.Equal(10, defaults.PaddingInlineStart);
        Assert.Equal(0.1, defaults.TransitionDuration, 3);
        Assert.Equal("ease, ease, ease, ease", defaults.TransitionTimingFunction);

        await page.Locator("[data-shadcn-scope]").EvaluateAsync("""
            element => {
                element.style.setProperty('--shadcn-spacing-multiplier', '2');
                element.style.setProperty('--shadcn-focus-ring-width', '5px');
                element.style.setProperty('--shadcn-focus-ring-offset', '2px');
                element.style.setProperty('--shadcn-motion-duration-fast', '200ms');
                element.style.setProperty('--shadcn-motion-easing-standard', 'linear');
                element.style.setProperty('--shadcn-font-mono', '"Courier New", monospace');
            }
        """);
        await button.FocusAsync();
        await page.WaitForTimeoutAsync(300);

        var customized = await ReadButtonStyles(button);
        Assert.Equal(12, customized.Gap);
        Assert.Equal(20, customized.PaddingInlineStart);
        Assert.Equal(0.2, customized.TransitionDuration, 3);
        Assert.Equal("linear, linear, linear, linear", customized.TransitionTimingFunction);
        Assert.Contains("2px", customized.BoxShadow, StringComparison.Ordinal);
        Assert.Contains("7px", customized.BoxShadow, StringComparison.Ordinal);
        Assert.Contains("Courier New", await inlineCode.AsElement()!
            .EvaluateAsync<string>("element => getComputedStyle(element).fontFamily"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task AlwaysAndSystemReducedMotionPoliciesBothReduceRealTransitions()
    {
        await using var normalContext = await playwright.Browser.NewContextAsync(new()
        {
            ReducedMotion = ReducedMotion.NoPreference
        });
        var normalPage = await normalContext.NewPageAsync();
        await normalPage.GotoAsync(new Uri(server.BaseUri, "/components/mud-inventory").ToString());
        await normalPage.GetByTestId("mud-inventory-fixture").WaitForAsync();
        var normalButton = normalPage.GetByTestId("button-default");
        Assert.Equal(0.1, await ReadTransitionDuration(normalButton), 3);

        await normalPage.Locator("[data-shadcn-scope]")
            .EvaluateAsync("element => element.setAttribute('data-shadcn-reduced-motion', 'always')");
        Assert.InRange(await ReadTransitionDuration(normalButton), 0, 0.001);

        await using var reducedContext = await playwright.Browser.NewContextAsync(new()
        {
            ReducedMotion = ReducedMotion.Reduce
        });
        var reducedPage = await reducedContext.NewPageAsync();
        await reducedPage.GotoAsync(new Uri(server.BaseUri, "/components/mud-inventory").ToString());
        await reducedPage.GetByTestId("mud-inventory-fixture").WaitForAsync();
        var reducedButton = reducedPage.GetByTestId("button-default");
        Assert.InRange(await ReadTransitionDuration(reducedButton), 0, 0.001);

        await reducedPage.Locator("[data-shadcn-scope]")
            .EvaluateAsync("element => element.setAttribute('data-shadcn-reduced-motion', 'always')");
        Assert.InRange(await ReadTransitionDuration(reducedButton), 0, 0.001);
    }

    private static Task<ButtonStyles> ReadButtonStyles(ILocator button) =>
        button.EvaluateAsync<ButtonStyles>("""
            element => {
                const style = getComputedStyle(element);
                return {
                    gap: parseFloat(style.gap),
                    paddingInlineStart: parseFloat(style.paddingInlineStart),
                    transitionDuration: parseFloat(style.transitionDuration),
                    transitionTimingFunction: style.transitionTimingFunction,
                    boxShadow: style.boxShadow
                };
            }
            """);

    private static Task<double> ReadTransitionDuration(ILocator element) =>
        element.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).transitionDuration)");

    private sealed class ButtonStyles
    {
        public double Gap { get; set; }
        public double PaddingInlineStart { get; set; }
        public double TransitionDuration { get; set; }
        public string TransitionTimingFunction { get; set; } = string.Empty;
        public string BoxShadow { get; set; } = string.Empty;
    }
}
