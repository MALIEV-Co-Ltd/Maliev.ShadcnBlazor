using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class DocumentationAnnotationsBrowserTests(
    ShowcaseServerFixture server,
    PlaywrightFixture playwright)
{
    [Fact]
    public async Task GeneratedUsageSourceDeduplicatesExistingImports()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();

        var source = await page.Locator("#usage pre").InnerTextAsync();
        Assert.Equal(
            1,
            source.Split("@using Maliev.ShadcnBlazor.Components.Conversation", StringSplitOptions.None).Length - 1);
        Assert.Equal(
            1,
            source.Split("@using Maliev.ShadcnBlazor.Components.Content", StringSplitOptions.None).Length - 1);

        var tagColors = await page.Locator("#usage .shadcn-code-token-tag").EvaluateAllAsync<string[]>(
            "tokens => tokens.map(token => getComputedStyle(token).color)");
        Assert.True(tagColors.Length >= 6, "The Message usage source should exercise a full Razor component tree.");
        Assert.Single(tagColors.Distinct(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(1280, 900, false, false)]
    [InlineData(390, 844, true, false)]
    [InlineData(1280, 900, false, true)]
    public async Task CodeBlockToolbarLanguageCopyAndEditorTokensStayUsable(int width, int height, bool rtl, bool accessibilityMode)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = rtl ? ColorScheme.Dark : ColorScheme.Light,
            ForcedColors = accessibilityMode ? ForcedColors.Active : ForcedColors.None
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/code-block").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        if (accessibilityMode) await page.EvaluateAsync("document.documentElement.style.zoom='2'");
        if (rtl)
        {
            await page.GetByTestId("documentation-direction-toggle").ClickAsync();
            await Assertions.Expect(page.Locator(".documentation-root")).ToHaveAttributeAsync("dir", "rtl");
        }

        var preview = page.GetByTestId("component-preview-canvas");
        await Assertions.Expect(preview.Locator("[data-slot='code-block']")).ToHaveCountAsync(3);
        var first = preview.Locator("[data-slot='code-block']").First;
        var toolbar = first.Locator("[data-slot='code-block-toolbar']");
        var copy = first.GetByTestId("copy-source");
        var toolbarBox = await toolbar.BoundingBoxAsync();
        var copyBox = await copy.BoundingBoxAsync();
        Assert.NotNull(toolbarBox);
        Assert.NotNull(copyBox);
        var endMetrics = await toolbar.EvaluateAsync<double[]>(rtl
            ? "element => { const copy=element.querySelector('[data-testid=copy-source]').getBoundingClientRect(); const box=element.getBoundingClientRect(); return [copy.x-box.x, parseFloat(getComputedStyle(element).paddingLeft)]; }"
            : "element => { const copy=element.querySelector('[data-testid=copy-source]').getBoundingClientRect(); const box=element.getBoundingClientRect(); return [box.right-copy.right, parseFloat(getComputedStyle(element).paddingRight)]; }");
        if (!accessibilityMode)
            Assert.InRange(Math.Abs(endMetrics[0] - endMetrics[1]), 0, 1);
        Assert.NotEqual("absolute", await copy.EvaluateAsync<string>("element => getComputedStyle(element).position"));
        var stableBefore = await toolbar.EvaluateAsync<double[]>("element => { const language=element.querySelector('[data-slot=select-trigger]').getBoundingClientRect(); const copy=element.querySelector('[data-testid=copy-source]').getBoundingClientRect(); const box=element.getBoundingClientRect(); return [box.height, language.x-box.x, language.width, copy.x-box.x, copy.width]; }");
        Assert.True(rtl ? stableBefore[1] > stableBefore[3] : stableBefore[1] < stableBefore[3], "The language selector must precede the copy action in logical order.");
        Assert.InRange(stableBefore[2], 24, 96);
        await Assertions.Expect(copy).ToBeVisibleAsync();
        await Assertions.Expect(copy).ToHaveAccessibleNameAsync("Copy source");
        Assert.Equal("ltr", await first.Locator("pre").EvaluateAsync<string>("element => getComputedStyle(element).direction"));
        Assert.True(await first.Locator(".shadcn-code-token-directive").CountAsync() > 0);
        if (!accessibilityMode)
        {
            var semanticColors = await first.Locator(".shadcn-code-token").EvaluateAllAsync<string[]>(
                "tokens => tokens.map(token => getComputedStyle(token).color)");
            Assert.True(
                semanticColors.Distinct(StringComparer.Ordinal).Count() >= 5,
                "The Razor palette collapsed to fewer than five semantic colors.");
        }

        var select = first.Locator("[data-slot='select-trigger']");
        await select.FocusAsync();
        await select.PressAsync("ArrowDown");
        await select.PressAsync("End");
        await select.PressAsync("Enter");
        await Assertions.Expect(first).ToHaveAttributeAsync("data-language", "csharp");
        await Assertions.Expect(first.Locator("pre")).ToContainTextAsync("SaveAsync");
        var copyStableBefore = await toolbar.EvaluateAsync<double[]>("element => { const language=element.querySelector('[data-slot=select-trigger]').getBoundingClientRect(); const copy=element.querySelector('[data-testid=copy-source]').getBoundingClientRect(); const box=element.getBoundingClientRect(); return [box.height, language.x-box.x, language.width, copy.x-box.x, copy.width]; }");

        if (accessibilityMode)
        {
            await copy.FocusAsync();
            await copy.PressAsync("Enter");
        }
        else
        {
            await copy.ClickAsync();
        }
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copy-state", "copied");
        await Assertions.Expect(copy).ToHaveAccessibleNameAsync("Copied");
        var stableAfter = await toolbar.EvaluateAsync<double[]>("element => { const language=element.querySelector('[data-slot=select-trigger]').getBoundingClientRect(); const copy=element.querySelector('[data-testid=copy-source]').getBoundingClientRect(); const box=element.getBoundingClientRect(); return [box.height, language.x-box.x, language.width, copy.x-box.x, copy.width]; }");
        for (var index = 0; index < copyStableBefore.Length; index++)
        {
            var difference = Math.Abs(copyStableBefore[index] - stableAfter[index]);
            Assert.True(difference <= 1, $"Toolbar metric {index} shifted by {difference}: before={copyStableBefore[index]}, after={stableAfter[index]}.");
        }
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copy-state", "idle", new() { Timeout = 3000 });
        await Assertions.Expect(copy).ToHaveAccessibleNameAsync("Copy source");
        await Assertions.Expect(copy).ToBeVisibleAsync();
        if (accessibilityMode)
        {
            await copy.PressAsync("Enter");
        }
        else
        {
            await copy.ClickAsync();
        }
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copy-state", "copied");

        Assert.True(await first.Locator(".shadcn-code-token-keyword").CountAsync() > 0);
        Assert.True(await first.Locator(".shadcn-code-token-method").CountAsync() > 0);
        var overflowCode = preview.Locator("[data-slot='code-block']").Nth(2).Locator("pre");
        var codeOverflow = await overflowCode.EvaluateAsync<double[]>(
            "element => [element.scrollWidth, element.clientWidth]");
        Assert.True(codeOverflow[0] > codeOverflow[1], "The stress source should scroll horizontally instead of wrapping long code.");
        Assert.InRange(await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth"), 0, 1);
    }

    [Theory]
    [InlineData(1280, 900, false, false)]
    [InlineData(390, 844, true, false)]
    [InlineData(1280, 900, false, true)]
    public async Task ApiValuesWrapWithoutOverlapAndDocumentationListsUseLogicalIndentation(int width, int height, bool rtl, bool zoom)
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = width, Height = height },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/code-block").ToString());
        await page.GetByTestId("component-dossier").WaitForAsync();
        if (zoom) await page.EvaluateAsync("document.documentElement.style.zoom='2'");
        if (rtl) await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var scroller = page.Locator(".component-api__scroller");
        var rows = scroller.GetByTestId("api-row");
        Assert.True(await rows.CountAsync() >= 3);
        var valueCells = rows.Locator(".component-api__value, .component-api__identifier");
        for (var index = 0; index < await valueCells.CountAsync(); index++)
        {
            var value = valueCells.Nth(index);
            var cell = value.Locator("xpath=ancestor::*[self::td or self::th][1]");
            var valueBox = await value.BoundingBoxAsync();
            var cellBox = await cell.BoundingBoxAsync();
            Assert.NotNull(valueBox);
            Assert.NotNull(cellBox);
            Assert.True(valueBox.Width <= cellBox.Width + 1, $"API value {index} escaped its cell ({valueBox.Width}px > {cellBox.Width}px).");
            Assert.Equal("anywhere", await value.EvaluateAsync<string>("element => getComputedStyle(element).overflowWrap"));
        }

        var list = page.Locator(".component-token-guidance .documentation-prose-list");
        var item = list.Locator(":scope > li").First;
        Assert.Equal("0px", await item.EvaluateAsync<string>("element => getComputedStyle(element).marginInlineStart"));
        Assert.True(await list.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).paddingInlineStart)") >= 16);
        var pageOverflow = await page.EvaluateAsync<double>("document.documentElement.scrollWidth-document.documentElement.clientWidth");
        Assert.InRange(pageOverflow, 0, 1);
    }
}
