using System.Text.Json;
using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeScenarioBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Theory]
    [InlineData(1280, 900, false)]
    [InlineData(390, 844, true)]
    public async Task EveryScenarioRendersThroughOneKeyedHostWithoutPageErrors(int width, int height, bool mobileDarkRtl)
    {
        var errors = new List<string>();
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = mobileDarkRtl ? ColorScheme.Dark : ColorScheme.Light
        });
        var page = await context.NewPageAsync();
        var currentScenario = "accordion-default";
        page.Console += (_, message) =>
        {
            if (message.Type == "error") errors.Add($"{currentScenario}: {message.Text} ({message.Location})");
        };
        page.PageError += (_, error) => errors.Add($"{currentScenario}: {error}");
        await page.GotoAsync(new Uri(server.BaseUri, "/theme?component=accordion&scenario=accordion-default").ToString());
        await page.GetByTestId("theme-scenario-browser").WaitForAsync();
        await Assertions.Expect(page.Locator("[data-theme-scenario-id='accordion-default']")).ToHaveCountAsync(1);

        if (mobileDarkRtl)
        {
            await page.GetByTestId("mode-dark").ClickAsync();
            await page.GetByTestId("direction-rtl").ClickAsync();
        }

        foreach (var slug in ScenarioSlugs())
        {
            await page.Locator($"[data-theme-scenario-component='{slug}']").ClickAsync();
            foreach (var kind in new[] { "default", "stress", "accessible" })
            {
                currentScenario = $"{slug}-{kind}";
                await page.GetByTestId($"theme-scenario-kind-{kind}").ClickAsync();
                await Assertions.Expect(page.Locator($"[data-theme-scenario-id='{slug}-{kind}']")).ToHaveCountAsync(1);
                await Assertions.Expect(page.Locator("[data-theme-scenario-host]")).ToHaveCountAsync(1);
                Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
            }
        }

        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task DirectLinksHistorySearchAndFocusRemainDeterministic()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme?component=accordion&scenario=accordion-default").ToString());
        await page.GetByTestId("theme-scenario-browser").WaitForAsync();

        await page.GetByTestId("theme-scenario-search").FillAsync("invoice");
        await Assertions.Expect(page.Locator("[data-theme-scenario-component]")).ToHaveCountAsync(1);
        await page.Locator("[data-theme-scenario-component='table']").ClickAsync();
        await Assertions.Expect(page.Locator("[data-theme-scenario-id='table-default']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("theme-scenario-host")).ToBeFocusedAsync();
        await page.GetByTestId("theme-scenario-kind-stress").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-scenario-direct-link")).ToHaveAttributeAsync("href", "theme?component=table&scenario=table-stress");

        await page.GoBackAsync();
        await Assertions.Expect(page.Locator("[data-theme-scenario-id='table-default']")).ToHaveCountAsync(1);
        await page.GoBackAsync();
        await Assertions.Expect(page.Locator("[data-theme-scenario-id='accordion-default']")).ToHaveCountAsync(1);
        await page.GoForwardAsync();
        await Assertions.Expect(page.Locator("[data-theme-scenario-id='table-default']")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task ScenarioSelectionSurvivesThemeLocaleViewportAndAccessibilityMutations()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            DeviceScaleFactor = 1,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/theme?component=chart&scenario=chart-stress").ToString());
        await page.GetByTestId("theme-scenario-browser").WaitForAsync();
        await Assertions.Expect(page.Locator("[data-theme-scenario-id='chart-stress']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("theme-scenario-direct-link"))
            .ToHaveAttributeAsync("href", "theme?component=chart&scenario=chart-stress");
        var axe = await page.GetByTestId("theme-scenario-browser").RunAxe();
        Assert.DoesNotContain(axe.Violations, violation => violation.Impact is "serious" or "critical");

        await page.GetByTestId("mode-dark").ClickAsync();
        await page.GetByTestId("direction-rtl").ClickAsync();
        await page.GetByTestId("locale-thai").ClickAsync();
        await page.GetByTestId("viewport-mobile").ClickAsync();
        await page.GetByTestId("preview-reduced-motion").ClickAsync();
        await page.GetByTestId("preview-high-contrast").ClickAsync();
        await page.EmulateMediaAsync(new() { ForcedColors = ForcedColors.Active, ReducedMotion = ReducedMotion.Reduce });
        await page.EvaluateAsync("document.documentElement.style.zoom = '2'");

        await Assertions.Expect(page.Locator("[data-theme-scenario-id='chart-stress']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("theme-scenario-browser")).ToHaveAttributeAsync("lang", "th");
        await Assertions.Expect(page.GetByTestId("theme-preview-scope")).ToHaveAttributeAsync("dir", "rtl");
        var overflow = await page.EvaluateAsync<double>(
            "Math.max(document.documentElement.scrollWidth - document.documentElement.clientWidth, document.body.scrollWidth - document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
    }

    private static IReadOnlyList<string> ScenarioSlugs()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            VisualProof.FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "ThemeScenarios", "ThemeScenarioCatalog.json")));
        return document.RootElement.GetProperty("scenarios").EnumerateArray()
            .Select(value => value.GetProperty("componentSlug").GetString() ?? throw new InvalidDataException())
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }
}
