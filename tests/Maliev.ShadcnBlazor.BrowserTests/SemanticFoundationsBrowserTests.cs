using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class SemanticFoundationsBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    public static TheoryData<int, int, string, string, string> Viewports => new()
    {
        { 1440, 900, "light", "ltr", "en" },
        { 768, 1024, "dark", "rtl", "en" },
        { 390, 844, "light", "ltr", "th" },
        { 320, 568, "dark", "rtl", "th" }
    };

    [Theory]
    [MemberData(nameof(Viewports))]
    public async Task FixtureHasHealthyResponsiveThemeDirectionAndLocalizedGeometry(
        int width,
        int height,
        string theme,
        string direction,
        string locale)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);

        var url = new Uri(server.BaseUri,
            $"/components/semantic-foundations?theme={theme}&dir={direction}&locale={locale}&fixture=all");
        await page.GotoAsync(url.ToString());
        await page.GetByTestId("semantic-foundations-fixture").WaitForAsync();

        var root = page.Locator("[data-shadcn-scope]");
        await Assertions.Expect(root).ToHaveAttributeAsync("data-shadcn-theme", theme);
        await Assertions.Expect(root).ToHaveAttributeAsync("dir", direction);
        await Assertions.Expect(page.GetByTestId("semantic-foundations-fixture")).ToHaveAttributeAsync("data-locale", locale);
        Assert.Empty(errors);

        var overflow = await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);

        var nestedDirection = page.GetByTestId("nested-direction-fixture");
        await Assertions.Expect(nestedDirection).ToHaveAttributeAsync("dir", "rtl");

        var screenshot = Path.Combine(Path.GetTempPath(), $"maliev-shadcn-semantic-{width}-{theme}-{direction}-{locale}.png");
        await page.ScreenshotAsync(new() { Path = screenshot, FullPage = true, Animations = ScreenshotAnimations.Disabled });
        Assert.True(File.Exists(screenshot));
    }

    [Fact]
    public async Task ComponentSemanticsFocusAndComputedGeometryMatchContracts()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1440, Height = 1000 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/semantic-foundations?fixture=all").ToString());
        await page.GetByTestId("semantic-foundations-fixture").WaitForAsync();

        foreach (var pair in new[] { (Name: "landscape", Ratio: 16d / 9d), (Name: "square", Ratio: 1d), (Name: "portrait", Ratio: 9d / 16d) })
        {
            var box = await page.Locator($"[data-ratio='{pair.Name}']").BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.InRange(Math.Abs((box!.Width / box.Height) - pair.Ratio), 0, 0.01);
        }

        await Assertions.Expect(page.Locator("label[for='display-name']")).ToHaveTextAsync("Display name");
        await Assertions.Expect(page.Locator("#display-name")).ToHaveAttributeAsync("aria-describedby", "display-name-help");
        await Assertions.Expect(page.Locator("#email-error")).ToHaveAttributeAsync("role", "alert");
        Assert.Equal(1, await page.Locator("#email-error").CountAsync());
        Assert.Equal(0, await page.Locator("#email-error li").CountAsync());

        var responsive = page.Locator("[data-testid='responsive-field']");
        await Assertions.Expect(responsive).ToHaveCSSAsync("flex-direction", "row");

        var itemLink = page.Locator("a[data-slot='item']");
        await itemLink.FocusAsync();
        var focusShadow = await itemLink.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");
        Assert.NotEqual("none", focusShadow);

        await Assertions.Expect(page.Locator("[data-slot='kbd']").First).ToHaveCSSAsync("height", "20px");
        await Assertions.Expect(page.Locator("[data-slot='separator']").First).ToHaveCSSAsync("height", "1px");
        await Assertions.Expect(page.GetByTestId("empty-fixture").Locator("[data-slot='empty']")).ToHaveCSSAsync("gap", "24px");
    }

    [Fact]
    public async Task ResponsiveFieldStacksBelowItsContainerBreakpoint()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 320, Height = 700 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/components/semantic-foundations?locale=th").ToString());
        await page.GetByTestId("semantic-foundations-fixture").WaitForAsync();

        await Assertions.Expect(page.GetByTestId("responsive-field")).ToHaveCSSAsync("flex-direction", "column");
    }

    [Fact]
    public async Task KbdDossierKeepsShortcutOrderInRtlAndSynchronizesPlatformSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 768, Height = 900 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/kbd").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var canvas = page.GetByTestId("component-preview-canvas");
        var groups = canvas.Locator("[data-slot='kbd-group']");
        await Assertions.Expect(groups).ToHaveCountAsync(3);
        var keyCounts = await groups.EvaluateAllAsync<int[]>("elements => elements.map(element => element.querySelectorAll('[data-slot=kbd]').length)");
        Assert.Equal(new[] { 1, 2, 3 }, keyCounts);
        Assert.All(await groups.EvaluateAllAsync<string[]>("elements => elements.map(element => element.tagName)"), tag => Assert.Equal("KBD", tag));

        await page.GetByTestId("documentation-direction-toggle").EvaluateAsync("element => element.click()");
        await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("dir", "rtl");
        await Assertions.Expect(groups.Nth(1)).ToHaveCSSAsync("direction", "ltr");

        await page.GetByTestId("control-kbd-platform").SelectOptionAsync("macOS");
        await Assertions.Expect(canvas).ToContainTextAsync("⌘");
        await Assertions.Expect(canvas).Not.ToContainTextAsync("Ctrl");
        await Assertions.Expect(groups.Nth(1)).ToHaveAttributeAsync("aria-label", "Command K");
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).ToContainTextAsync("<ShadcnKbd>⌘</ShadcnKbd>");
        await Assertions.Expect(page.Locator("#preview .component-code pre").First).Not.ToContainTextAsync("<ShadcnKbd>Ctrl</ShadcnKbd>");
    }
}
