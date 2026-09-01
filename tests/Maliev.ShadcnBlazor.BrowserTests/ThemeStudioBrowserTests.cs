using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeStudioBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    public static TheoryData<int, int> ReleaseViewports => new() { { 1440, 900 }, { 1024, 768 }, { 768, 1024 }, { 390, 844 }, { 320, 568 } };
    public static TheoryData<int, int> PaletteWorkbenchViewports => new() { { 1440, 900 }, { 1055, 800 }, { 1025, 768 }, { 1024, 768 }, { 390, 844 }, { 320, 568 } };

    [Theory]
    [MemberData(nameof(PaletteWorkbenchViewports))]
    public async Task PaletteCustomizationExpandsInlineWithoutStealingPreviewWidthOrOverflowing(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, width <= 1024);
        var page = await OpenAsync(context);
        if (width <= 1024)
            await OpenSettingsAsync(page);

        var previewBefore = await page.Locator(".theme-preview-region").BoundingBoxAsync();
        var customize = page.GetByTestId("theme-palette-customize");
        await customize.ScrollIntoViewIfNeededAsync();
        await customize.ClickAsync();
        var workbench = page.GetByTestId("theme-palette-workbench");

        await Assertions.Expect(customize).ToHaveAttributeAsync("aria-expanded", "true");
        await Assertions.Expect(workbench).ToBeVisibleAsync();
        Assert.True(await workbench.EvaluateAsync<bool>("element => Boolean(element.closest('[data-testid=theme-palette-summary]'))"));
        Assert.True(await workbench.EvaluateAsync<bool>("element => Boolean(element.closest('[data-testid=theme-studio-sidebar]'))"));
        Assert.Null(await workbench.GetAttributeAsync("role"));
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
        Assert.True(await page.Locator(".theme-palette-summary__swatches").EvaluateAsync<double>("element => element.getBoundingClientRect().height") >= 48);

        var previewAfter = await page.Locator(".theme-preview-region").BoundingBoxAsync();
        Assert.NotNull(previewBefore);
        Assert.NotNull(previewAfter);
        Assert.InRange(Math.Abs(previewBefore!.Width - previewAfter!.Width), 0, 1);
    }

    [Fact]
    public async Task RapidPaletteGenerationIsCoalescedAndDerivesFromTheChosenMainColor()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        await context.AddInitScriptAsync("""
            (() => {
                window.__paletteSaves = 0;
                const original = Storage.prototype.setItem;
                Storage.prototype.setItem = function(key, value) {
                    if (key === 'maliev.shadcn.theme-studio.document.v2') window.__paletteSaves++;
                    return original.call(this, key, value);
                };
            })();
            """);
        var page = await OpenAsync(context);
        await page.GetByTestId("theme-palette-customize").ClickAsync();
        var main = page.GetByTestId("theme-palette-anchor-brand").Locator("input[type='text']");
        await main.FillAsync("#2563eb");
        await main.PressAsync("Enter");
        await page.WaitForFunctionAsync("() => localStorage.getItem('maliev.shadcn.theme-studio.document.v2') !== null");
        await page.EvaluateAsync("window.__paletteSaves = 0");

        await page.GetByTestId("theme-palette-generate").EvaluateAsync("button => { for (let index = 0; index < 12; index++) button.click(); }");
        await Assertions.Expect(page.GetByTestId("theme-palette-status"))
            .ToHaveTextAsync(new Regex("^Palette generated: Seed [0-9]+$"));
        await page.WaitForTimeoutAsync(700);

        Assert.InRange(await page.EvaluateAsync<int>("window.__paletteSaves"), 1, 2);
        var saved = await page.EvaluateAsync<string>("localStorage.getItem('maliev.shadcn.theme-studio.document.v2')");
        using var document = System.Text.Json.JsonDocument.Parse(saved);
        var palette = document.RootElement.GetProperty("palette");
        Assert.Equal("brand", string.Join(",", palette.GetProperty("lockedAnchors").EnumerateArray().Select(item => item.GetString())));
        Assert.NotEqual(palette.GetProperty("anchors").GetProperty("brand").GetString(), palette.GetProperty("anchors").GetProperty("support").GetString());
        await page.GetByTestId("theme-category-filter-overlays").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-category-filter-overlays")).ToHaveAttributeAsync("aria-pressed", "true");
    }

    [Fact]
    public async Task OrderMixRendersEveryGeneratedPaletteAnchor()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await page.GetByTestId("theme-category-filter-data").ClickAsync();
        var chart = page.Locator("[data-use-case-id='order-mix'] .shadcn-chart");
        await Assertions.Expect(chart).ToBeVisibleAsync();

        var fills = await chart.Locator("path[data-chart-shape='arc']")
            .EvaluateAllAsync<string[]>("paths => paths.map(path => getComputedStyle(path).fill)");

        Assert.Equal(5, fills.Length);
        Assert.Equal(5, fills.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData(1440, 900)]
    [InlineData(390, 844)]
    public async Task InlinePaletteCustomizationLocalizesAndHasNoSeriousAccessibilityViolations(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, width <= 390);
        var page = await OpenAsync(context);
        if (width <= 1024)
            await OpenSettingsAsync(page);
        await page.GetByTestId("theme-palette-customize").ClickAsync();
        var workbench = page.GetByTestId("theme-palette-workbench");
        await Assertions.Expect(workbench.GetByRole(AriaRole.Heading, new() { Name = "Main color", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(workbench.GetByRole(AriaRole.Button, new() { Name = "Generate from main color", Exact = true })).ToBeVisibleAsync();

        await page.GetByTestId("locale-thai").ClickAsync();
        await Assertions.Expect(workbench.GetByRole(AriaRole.Heading, new() { Name = "สีหลัก", Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(workbench.GetByRole(AriaRole.Button, new() { Name = "สร้างจากสีหลัก", Exact = true })).ToBeVisibleAsync();
        var accessibility = await workbench.RunAxe();
        Assert.DoesNotContain(accessibility.Violations, violation => violation.Impact is "serious" or "critical");
    }


    [Fact]
    public async Task MobileHeaderBrandMarkMatchesCompactActionGeometry()
    {
        await using var context = await NewContextAsync(390, 844, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var brandMark = page.Locator(".documentation-brand__mark");
        var catalogTrigger = page.GetByTestId("catalog-trigger");
        var catalogIcon = catalogTrigger.Locator(".documentation-trigger__icon");
        var versionLink = page.GetByTestId("documentation-version-link");
        var brandBox = await brandMark.BoundingBoxAsync();
        var triggerBox = await catalogTrigger.BoundingBoxAsync();
        var iconBox = await catalogIcon.BoundingBoxAsync();

        Assert.NotNull(brandBox);
        Assert.NotNull(triggerBox);
        Assert.NotNull(iconBox);
        Assert.InRange(Math.Abs(brandBox.Width - triggerBox.Width), 0, 0.5);
        Assert.InRange(Math.Abs(brandBox.Height - triggerBox.Height), 0, 0.5);
        Assert.InRange(
            Math.Abs((brandBox.Y + brandBox.Height / 2) - (triggerBox.Y + triggerBox.Height / 2)),
            0,
            0.5);
        Assert.InRange(
            Math.Abs((iconBox.X + iconBox.Width / 2) - (triggerBox.X + triggerBox.Width / 2)),
            0,
            0.5);
        Assert.InRange(
            Math.Abs((iconBox.Y + iconBox.Height / 2) - (triggerBox.Y + triggerBox.Height / 2)),
            0,
            0.5);
        Assert.Equal("0px", await versionLink.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
    }

    [Fact]
    public async Task DesktopSettingsCollapseToAnIconRailThatPreviewsOnHoverAndPinsOnClick()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var provider = page.GetByTestId("theme-studio-sidebar").Locator("..");
        var sidebar = page.GetByTestId("theme-studio-sidebar");
        var preview = page.Locator(".theme-preview-region");
        var appBarToggle = page.GetByTestId("theme-controls-toggle");

        await Assertions.Expect(appBarToggle).ToBeHiddenAsync();
        await page.GetByTestId("theme-sidebar-collapse").ClickAsync();
        await Assertions.Expect(sidebar).ToHaveAttributeAsync("data-state", "collapsed");
        await Assertions.Expect(page.GetByTestId("theme-sidebar-compact-menu")).ToBeVisibleAsync();

        var collapsedProvider = await provider.BoundingBoxAsync();
        var collapsedPreview = await preview.BoundingBoxAsync();
        Assert.NotNull(collapsedProvider);
        Assert.NotNull(collapsedPreview);
        Assert.InRange(collapsedProvider.Width, 48, 72);

        await provider.HoverAsync();
        await Assertions.Expect(sidebar).ToHaveAttributeAsync("data-preview-expanded", "true");
        var hoveredSidebar = await sidebar.BoundingBoxAsync();
        var hoveredProvider = await provider.BoundingBoxAsync();
        var hoveredPreview = await preview.BoundingBoxAsync();
        Assert.NotNull(hoveredSidebar);
        Assert.NotNull(hoveredProvider);
        Assert.NotNull(hoveredPreview);
        Assert.True(hoveredSidebar.Width >= 300, $"Expected the hover preview to expand to at least 300px, but it was {hoveredSidebar.Width}px.");
        Assert.InRange(hoveredProvider.Width, 48, 72);
        Assert.InRange(Math.Abs(hoveredPreview.X - collapsedPreview.X), 0, 0.5);

        await preview.HoverAsync(new() { Position = new() { X = 200, Y = 100 } });
        await Assertions.Expect(sidebar).ToHaveAttributeAsync("data-preview-expanded", "false");
        await page.GetByTestId("theme-sidebar-rail-toggle").ClickAsync();
        await Assertions.Expect(sidebar).ToHaveAttributeAsync("data-state", "expanded");
        await Assertions.Expect(provider).ToHaveCSSAsync("width", new Regex("^(?:3\\d{2}|[4-9]\\d{2}|\\d{4,})px$"));
        Assert.True((await provider.BoundingBoxAsync())!.Width >= 300);
    }

    [Fact]
    public async Task MobileSettingsTogglePrecedesTheBrandAndControlsTheOffCanvasSidebar()
    {
        await using var context = await NewContextAsync(390, 844, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var toggle = page.GetByTestId("theme-controls-toggle");
        var leading = page.Locator(".documentation-header__leading");

        await Assertions.Expect(toggle).ToBeVisibleAsync();
        Assert.True(await leading.EvaluateAsync<bool>("element => element.firstElementChild?.dataset.testid === 'theme-controls-toggle'"));
        await toggle.ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-studio-sidebar")).ToBeVisibleAsync();
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");

        await page.GetByTestId("theme-sidebar-collapse").ClickAsync();
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(page.Locator(".documentation-header")).Not.ToHaveAttributeAsync("inert", "");
        await toggle.ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-studio-sidebar")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task FilteringKeepsSidebarAndPreviewAlignedAndCategoryTabsScrollWithoutAVisibleTrack()
    {
        await using var context = await NewContextAsync(1025, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var provider = page.GetByTestId("theme-studio-sidebar").Locator("..");
        var preview = page.Locator(".theme-preview-region");
        var categories = page.Locator(".theme-bento__category-scroll");

        await page.GetByTestId("theme-category-filter-security").ClickAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id]")).ToHaveCountAsync(3);
        var providerBox = await provider.BoundingBoxAsync();
        var previewBox = await preview.BoundingBoxAsync();
        Assert.NotNull(providerBox);
        Assert.NotNull(previewBox);
        Assert.InRange(Math.Abs(providerBox.Y - previewBox.Y), 0, 0.5);

        Assert.Equal("auto", await categories.EvaluateAsync<string>("element => getComputedStyle(element).overflowX"));
        Assert.Equal("none", await categories.EvaluateAsync<string>("element => getComputedStyle(element).scrollbarWidth"));
        Assert.True(await categories.EvaluateAsync<bool>("element => element.scrollWidth > element.clientWidth"));
        await categories.EvaluateAsync("element => element.scrollLeft = 120");
        Assert.True(Math.Abs(await categories.EvaluateAsync<double>("element => element.scrollLeft")) > 0);
    }

    [Fact]
    public async Task BentoLayoutStylesheetIsServedAndAppliedAtCompactDesktopWidth()
    {
        await using var context = await NewContextAsync(1121, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var stylesheet = await page.EvaluateAsync<string>(
            "async () => await (await fetch('_content/Maliev.ShadcnBlazor/css/shadcn-layout.css?v=2.1.4')).text()");
        Assert.Contains(".shadcn-bento-grid__layout", stylesheet, StringComparison.Ordinal);

        var layout = page.Locator(".theme-bento__grid [data-slot='bento-grid-layout']");
        await Assertions.Expect(layout).ToHaveCSSAsync("display", "grid");
        Assert.Equal(2, await layout.EvaluateAsync<int>("element => getComputedStyle(element).gridTemplateColumns.split(' ').length"));
    }

    [Fact]
    public async Task BentoUsesOneOrderedScrollableCanvasWithoutMirrorsOrClippedBorders()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var bento = page.GetByTestId("theme-bento");
        var cards = bento.Locator("[data-use-case-id]");
        await Assertions.Expect(bento).ToBeVisibleAsync();
        Assert.Equal(45, await cards.CountAsync());
        Assert.Equal(45, (await CardIdsAsync(page)).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(0, await bento.Locator("[data-component-slug]").CountAsync());
        await Assertions.Expect(page.Locator("[data-mirror], [data-runway-track]")).ToHaveCountAsync(0);
        var first = cards.First;
        Assert.Equal("1px", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderLeftWidth"));
        Assert.Equal("1px", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderRightWidth"));
        Assert.Equal("rgba(0, 0, 0, 0)", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderLeftColor"));
        Assert.NotEqual("rgba(0, 0, 0, 0)", await first.EvaluateAsync<string>("element => getComputedStyle(element, '::after').borderLeftColor"));
        Assert.Equal(
            await first.EvaluateAsync<string>("element => getComputedStyle(element).borderTopLeftRadius"),
            await first.EvaluateAsync<string>("element => getComputedStyle(element, '::after').borderTopLeftRadius"));
        var preview = page.Locator(".theme-preview-region");
        Assert.True(await preview.EvaluateAsync<bool>("element => element.scrollHeight > element.clientHeight"));
        await preview.EvaluateAsync("element => element.scrollTop = 700");
        Assert.True(await preview.EvaluateAsync<double>("element => element.scrollTop") > 100);
    }

    [Theory]
    [InlineData(1920, 4, 2)]
    [InlineData(1440, 2, 2)]
    [InlineData(1024, 2, 2)]
    [InlineData(390, 1, 1)]
    public async Task WideWorkflowsSpanResponsiveBentoTracks(int width, int expectedTracks, int expectedSpan)
    {
        await using var context = await NewContextAsync(width, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var layout = page.Locator(".theme-bento__grid [data-slot='bento-grid-layout']");
        var wide = page.Locator("[data-use-case-item='production-analytics']");
        Assert.Equal(expectedTracks, await layout.EvaluateAsync<int>("element => getComputedStyle(element).gridTemplateColumns.split(' ').length"));
        var columns = await wide.EvaluateAsync<string>("element => `${getComputedStyle(element).gridColumnStart} / ${getComputedStyle(element).gridColumnEnd}`");
        Assert.Contains(expectedSpan == expectedTracks && expectedTracks > 1 ? "1 / -1" : $"span {expectedSpan}", columns, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BentoMasonryClosesShortCardGapsAndKeepsWideContentReadable()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var grid = page.Locator(".theme-bento__grid");
        var layout = grid.Locator("[data-slot='bento-grid-layout']");
        await Assertions.Expect(grid).ToHaveAttributeAsync("data-layout", "masonry");
        await Assertions.Expect(grid).ToHaveAttributeAsync("data-masonry-ready", "true");
        Assert.Contains("dense", await layout.EvaluateAsync<string>("element => getComputedStyle(element).gridAutoFlow"), StringComparison.Ordinal);

        var capacity = page.Locator("[data-use-case-item='production-capacity']");
        var capacityBox = await capacity.BoundingBoxAsync();
        Assert.NotNull(capacityBox);
        var placement = await capacity.EvaluateAsync<string>(
            "target => JSON.stringify(Array.from(target.parentElement.children).map(item => { const box = item.getBoundingClientRect(); return { id: item.dataset.useCaseItem, left: box.left, top: box.top, bottom: box.bottom }; }))");
        var closesGap = await capacity.EvaluateAsync<bool>(
            "target => Array.from(target.parentElement.children).some(item => { const box = item.getBoundingClientRect(); return item !== target && Math.abs(box.left - target.getBoundingClientRect().left) < 2 && box.top >= target.getBoundingClientRect().bottom && box.top - target.getBoundingClientRect().bottom < 48; })");
        Assert.True(closesGap, placement);

        var attachment = page.Locator("[data-use-case-item='drawing-attachment']");
        Assert.Equal("2", await attachment.GetAttributeAsync("data-column-span"));
        Assert.True((await attachment.BoundingBoxAsync())!.Width > capacityBox!.Width * 1.8);
        Assert.InRange(await page.Locator("[data-use-case-id='production-capacity'] .theme-runway-stat strong").EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).fontSize)"), 28, 40);
    }

    [Fact]
    public async Task InspectionSchedulingDatePickerEscapesCardClipping()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='inspection-scheduling']");
        await card.ScrollIntoViewIfNeededAsync();

        await card.Locator("[data-slot='date-picker-trigger']").ClickAsync();
        var popup = card.Locator("[data-slot='date-picker-content']");
        await Assertions.Expect(popup).ToBeVisibleAsync();

        Assert.True(
            await popup.EvaluateAsync<bool>("element => element.matches(':popover-open')"),
            "The date-picker popup must use the browser top layer so clipped ancestors cannot crop it.");
    }

    [Fact]
    public async Task InspectionSchedulingCalendarCentersGridAndKeepsMutedTextReadable()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var calendar = page.Locator("[data-use-case-id='inspection-scheduling'] [data-slot='calendar']");
        await calendar.ScrollIntoViewIfNeededAsync();
        var navigation = calendar.Locator("[data-slot='calendar-nav']");
        var navigationOffset = await navigation.EvaluateAsync<double[]>(
            """
            element => {
                const center = node => { const bounds = node.getBoundingClientRect(); return bounds.left + bounds.width / 2; };
                const buttons = element.querySelectorAll('button');
                const weekdays = element.closest('[data-slot=calendar]').querySelectorAll('.shadcn-calendar-weekday');
                return [Math.abs(center(buttons[0]) - center(weekdays[0])), Math.abs(center(buttons[1]) - center(weekdays[6]))];
            }
            """);
        Assert.All(navigationOffset, offset => Assert.InRange(offset, 0, 1));

        var mutedContrast = await calendar.Locator("[data-slot='calendar-weekday']").First.EvaluateAsync<double>(
            """
            element => {
                const canvas = document.createElement('canvas');
                canvas.width = canvas.height = 1;
                const context = canvas.getContext('2d', { willReadFrequently: true });
                const channels = value => {
                    context.clearRect(0, 0, 1, 1);
                    context.fillStyle = value;
                    context.fillRect(0, 0, 1, 1);
                    return Array.from(context.getImageData(0, 0, 1, 1).data.slice(0, 3));
                };
                const luminance = value => {
                    const linear = channels(value).map(channel => {
                        const normalized = channel / 255;
                        return normalized <= 0.04045 ? normalized / 12.92 : Math.pow((normalized + 0.055) / 1.055, 2.4);
                    });
                    return 0.2126 * linear[0] + 0.7152 * linear[1] + 0.0722 * linear[2];
                };
                let surface = element;
                while (surface && getComputedStyle(surface).backgroundColor === 'rgba(0, 0, 0, 0)') surface = surface.parentElement;
                const foreground = luminance(getComputedStyle(element).color);
                const background = luminance(surface ? getComputedStyle(surface).backgroundColor : 'rgb(255, 255, 255)');
                return (Math.max(foreground, background) + 0.05) / (Math.min(foreground, background) + 0.05);
            }
            """);
        Assert.True(mutedContrast >= 7, $"Calendar weekday contrast was {mutedContrast:F2}:1 instead of at least 7:1.");
    }

    [Fact]
    public async Task MaterialRoutingSelectUsesOneFocusRingAroundTheCompositeControl()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var root = page.Locator("[data-use-case-id='material-routing'] [data-slot='select']").First;
        await root.ScrollIntoViewIfNeededAsync();
        var trigger = root.Locator("[data-slot='select-trigger']");

        await trigger.FocusAsync();

        Assert.NotEqual("none", await root.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow"));
        await Assertions.Expect(trigger).ToHaveCSSAsync("box-shadow", "none");
        await Assertions.Expect(trigger).ToHaveCSSAsync("outline-style", "none");
        var radius = await trigger.EvaluateAsync<double[]>("element => { const style = getComputedStyle(element); return [parseFloat(style.borderRadius), element.getBoundingClientRect().height]; }");
        Assert.True(radius[0] < radius[1] / 2, $"Select radius {radius[0]}px should not create a pill inside a {radius[1]}px control.");
    }

    [Fact]
    public async Task MaterialRoutingSelectEscapesCardClipping()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var root = page.Locator("[data-use-case-id='material-routing'] [data-slot='select']").First;
        await root.ScrollIntoViewIfNeededAsync();

        await root.Locator("[data-slot='select-trigger']").ClickAsync();
        var popup = root.Locator("[data-slot='select-content']");
        await Assertions.Expect(popup).ToBeVisibleAsync();
        Assert.True(
            await popup.EvaluateAsync<bool>("element => element.matches(':popover-open')"),
            "The select options must use the browser top layer so clipped ancestors cannot crop them.");
    }

    [Fact]
    public async Task InspectorSelectUsesAvailableViewportSpaceWithoutOverlappingItsTrigger()
    {
        await using var context = await NewContextAsync(390, 568, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await OpenSettingsAsync(page);
        var trigger = page.GetByTestId("theme-visual-style");
        await trigger.EvaluateAsync("element => element.scrollIntoView({ block: 'end' })");

        await trigger.ClickAsync();
        var popup = page.Locator("[data-slot='select-content']");
        await Assertions.Expect(popup).ToBeVisibleAsync();
        var geometry = await popup.EvaluateAsync<double[]>(
            """
            element => {
                const popup = element.getBoundingClientRect();
                const trigger = element.closest('[data-slot=select]').querySelector('[data-slot=select-trigger]').getBoundingClientRect();
                return [popup.top, popup.bottom, trigger.top, trigger.bottom, innerHeight];
            }
            """);

        Assert.True(geometry[1] <= geometry[2] || geometry[0] >= geometry[3], $"Popup overlapped its trigger: {string.Join(", ", geometry)}");
        Assert.InRange(geometry[0], 8, geometry[4] - 8);
        Assert.InRange(geometry[1], 8, geometry[4] - 8);
    }

    [Fact]
    public async Task MaterialRoutingComboboxHoverFollowsTheCompositeControlRadius()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var root = page.Locator("[data-use-case-id='material-routing'] [data-slot='combobox']");
        await root.ScrollIntoViewIfNeededAsync();
        var inputGroup = root.Locator("[data-slot='input-group']");
        var trigger = root.Locator("[data-slot='combobox-trigger']");

        await trigger.HoverAsync();

        var radii = await trigger.EvaluateAsync<double[]>(
            """
            element => {
                const triggerStyle = getComputedStyle(element);
                const groupStyle = getComputedStyle(element.closest('[data-slot=input-group]'));
                return [
                    parseFloat(triggerStyle.borderTopRightRadius),
                    parseFloat(groupStyle.borderTopRightRadius),
                    parseFloat(triggerStyle.borderBottomRightRadius),
                    parseFloat(groupStyle.borderBottomRightRadius)
                ];
            }
            """);
        Assert.Equal(radii[1], radii[0], 2);
        Assert.Equal(radii[3], radii[2], 2);
        Assert.NotEqual(
            "rgba(0, 0, 0, 0)",
            await trigger.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        await Assertions.Expect(inputGroup).ToHaveCSSAsync("overflow", "clip");
        var control = root.Locator("[data-slot='combobox-input']");
        await control.FocusAsync();
        await Assertions.Expect(control).ToHaveCSSAsync("border-top-width", "0px");
        await Assertions.Expect(control).ToHaveCSSAsync("box-shadow", "none");
    }

    [Theory]
    [InlineData(1569, 1032)]
    [InlineData(390, 844)]
    public async Task OverlayFeedbackToastViewportUsesTheTopLayerAndCapsTheVisibleQueue(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, width <= 768);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='overlay-feedback-lab']");
        await card.ScrollIntoViewIfNeededAsync();
        var trigger = card.GetByRole(AriaRole.Button, new() { Name = "Show toast" });
        var viewport = card.Locator("[data-slot='toast-viewport']");
        var toasts = viewport.Locator("[data-slot='toast']");

        await trigger.ClickAsync();
        await Assertions.Expect(toasts).ToHaveCountAsync(1);
        await viewport.HoverAsync();
        await trigger.EvaluateAsync(
            "element => element.dispatchEvent(new MouseEvent('click', { bubbles: true }))");
        await Assertions.Expect(toasts).ToHaveCountAsync(2);
        await trigger.EvaluateAsync(
            "element => element.dispatchEvent(new MouseEvent('click', { bubbles: true }))");
        await Assertions.Expect(toasts).ToHaveCountAsync(2);
        Assert.True(
            await viewport.EvaluateAsync<bool>("element => element.matches(':popover-open')"),
            "The toast viewport must use the browser top layer so containing blocks cannot capture fixed positioning.");

        var bounds = await toasts.EvaluateAllAsync<double[][]>(
            "elements => elements.map(element => { const box = element.getBoundingClientRect(); return [box.left, box.top, box.right, box.bottom, innerWidth, innerHeight]; })");
        Assert.All(bounds, box =>
        {
            Assert.True(box[0] >= 8 && box[2] <= box[4] - 8, $"Toast escaped the horizontal viewport: {string.Join(", ", box)}");
            Assert.True(box[1] >= 8 && box[3] <= box[5] - 8, $"Toast escaped the vertical viewport: {string.Join(", ", box)}");
        });
    }

    [Fact]
    public async Task OverlayFeedbackToastHoverKeepsTheVisibleStackStationary()
    {
        await using var context = await NewContextAsync(900, 720, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='overlay-feedback-lab']");
        await card.ScrollIntoViewIfNeededAsync();
        var trigger = card.GetByRole(AriaRole.Button, new() { Name = "Show toast" });
        await trigger.ClickAsync();
        await trigger.ClickAsync();
        var viewport = card.Locator("[data-slot='toast-viewport']");
        var presentedToasts = viewport.Locator("[data-slot='toast']:not([data-limited='true'])");
        await Assertions.Expect(presentedToasts).ToHaveCountAsync(2);
        var before = await presentedToasts.EvaluateAllAsync<double[][]>(
            "elements => elements.map(element => { const box = element.getBoundingClientRect(); return [box.left, box.top, box.width, box.height]; })");

        await viewport.DispatchEventAsync("mouseenter");
        await Assertions.Expect(viewport).ToHaveAttributeAsync("data-expanded", "true");
        var after = await presentedToasts.EvaluateAllAsync<double[][]>(
            "elements => elements.map(element => { const box = element.getBoundingClientRect(); return [box.left, box.top, box.width, box.height]; })");

        Assert.Equal(before.Length, after.Length);
        for (var index = 0; index < before.Length; index++)
        {
            for (var metric = 0; metric < before[index].Length; metric++)
                Assert.InRange(Math.Abs(before[index][metric] - after[index][metric]), 0, 1);
        }
    }

    [Theory]
    [InlineData(1569, 1032)]
    [InlineData(390, 844)]
    public async Task ProductionCarouselKeepsEverySelectedSlideWithinItsViewport(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, width <= 768);
        var page = await OpenAsync(context);
        var carousel = page.Locator("[data-use-case-id='production-carousel'] [data-slot='carousel']");
        await carousel.ScrollIntoViewIfNeededAsync();
        var viewport = carousel.Locator("[data-slot='carousel-content']");
        var items = carousel.Locator("[data-slot='carousel-item']");

        for (var index = 0; index < 3; index++)
        {
            if (index > 0)
                await carousel.Locator("[data-slot='carousel-next']").ClickAsync();

            var item = items.Nth(index);
            await Assertions.Expect(item).ToHaveAttributeAsync("data-selected", "true");
            var sizes = await item.EvaluateAsync<double[]>(
                """
                element => {
                    const item = element.getBoundingClientRect();
                    const viewport = element.closest('[data-slot=carousel]').querySelector('[data-slot=carousel-content]').getBoundingClientRect();
                    const stage = element.querySelector('.theme-carousel-stage').getBoundingClientRect();
                    return [item.width, viewport.width, stage.left - viewport.left, viewport.right - stage.right];
                }
                """);
            Assert.InRange(Math.Abs(sizes[0] - sizes[1]), 0, 1);
            Assert.True(sizes[2] is >= -1 and <= 1, $"Slide {index + 1} leading inset was {sizes[2]:F2}px. Geometry: {string.Join(", ", sizes)}");
            Assert.True(sizes[3] is >= 0 and <= 20, $"Slide {index + 1} trailing inset was {sizes[3]:F2}px. Geometry: {string.Join(", ", sizes)}");
        }

        await Assertions.Expect(carousel.Locator("[data-slot='carousel-previous']")).ToHaveAttributeAsync("aria-label", "Previous production stage");
        await Assertions.Expect(carousel.Locator("[data-slot='carousel-next']")).ToHaveAttributeAsync("aria-label", "Next production stage");
    }

    [Fact]
    public async Task ProductionCarouselPresentsStageCopyAsSeparateReadableRows()
    {
        await using var context = await NewContextAsync(900, 720, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var stage = page.Locator("[data-use-case-id='production-carousel'] .theme-carousel-stage").First;
        await stage.ScrollIntoViewIfNeededAsync();
        await stage.Locator("span").First.EvaluateAsync(
            "element => { document.documentElement.style.fontSize = '200%'; element.textContent = 'InspectionEvidencePackage'.repeat(12); }");
        var rows = stage.Locator(":scope strong, :scope span, :scope [data-slot='badge']");

        var positions = await rows.EvaluateAllAsync<double[]>("elements => elements.map(element => element.getBoundingClientRect().top)");
        Assert.Equal(3, positions.Length);
        Assert.True(positions[0] < positions[1] && positions[1] < positions[2], $"Stage rows were not vertically ordered: {string.Join(", ", positions)}");
        var overflow = await stage.EvaluateAsync<double[]>(
            """
            element => {
                const body = element.querySelector('.theme-carousel-stage__body');
                const detail = body.querySelector('span');
                return [element.clientWidth, element.scrollWidth, body.clientWidth, body.scrollWidth, detail.clientWidth, detail.scrollWidth];
            }
            """);
        Assert.True(overflow[1] <= overflow[0], $"Stage content must wrap without horizontal overflow. Geometry: {string.Join(", ", overflow)}");
    }

    [Fact]
    public async Task BentoMasonryReclaimsRowsAfterInteractiveContentShrinks()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var grid = page.Locator(".theme-bento__grid");
        var item = page.Locator("[data-use-case-item='production-analytics']");
        await Assertions.Expect(grid).ToHaveAttributeAsync("data-masonry-ready", "true");

        var initialSpan = await MasonrySpanAsync(item);
        await item.Locator(":scope > *").EvaluateAsync("element => element.style.minBlockSize = '60rem'");
        await WaitForMasonrySpanAsync(item, span => span > initialSpan);
        var expandedSpan = await MasonrySpanAsync(item);

        await item.Locator(":scope > *").EvaluateAsync("element => element.style.removeProperty('min-block-size')");
        await WaitForMasonrySpanAsync(item, span => span < expandedSpan);

        Assert.InRange(await MasonrySpanAsync(item), initialSpan - 1, initialSpan + 1);
    }

    [Fact]
    public async Task BentoMasonryKeepsVisualPackingNearTheConfiguredGap()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var grid = page.Locator(".theme-bento__grid");
        await Assertions.Expect(grid).ToHaveAttributeAsync("data-masonry-ready", "true");
        await page.GetByTestId("locale-thai").ClickAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='production-capacity']")).ToContainTextAsync("กำลังการผลิตรายสัปดาห์");
        await Assertions.Expect(grid.Locator("[data-slot='bento-grid-layout']")).ToHaveCSSAsync("grid-auto-rows", "1px");

        var largestGap = await grid.Locator("[data-slot='bento-grid-layout']").EvaluateAsync<double>("""
            layout => {
                const items = Array.from(layout.children);
                const boxes = items.map(item => item.getBoundingClientRect());
                return Math.max(...boxes.map((current, index) => {
                    const nearestBottom = Math.max(
                        ...boxes
                            .filter((other, otherIndex) => otherIndex !== index
                                && other.bottom <= current.top + 1
                                && other.right > current.left + 1
                                && other.left < current.right - 1)
                            .map(other => other.bottom),
                        current.top);
                    return current.top - nearestBottom;
                }));
            }
            """);

        Assert.InRange(largestGap, 0, 33);
    }

    [Fact]
    public async Task ScrolledBentoPreservesClipRevealAndRoundedBorders()
    {
        await using var context = await NewContextAsync(1900, 1032, ReducedMotion.NoPreference);
        var page = await OpenAsync(context);
        var clippedCard = page.Locator("[data-use-case-item='work-order-navigation'] .theme-bento__reveal");
        var ordinaryCard = page.Locator("[data-use-case-item='project-questionnaire'] .theme-bento__reveal");

        await Assertions.Expect(clippedCard).ToHaveAttributeAsync("data-reveal-effect", "clip");
        await clippedCard.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(clippedCard).ToHaveAttributeAsync("data-reveal-state", "revealed");
        await Assertions.Expect(clippedCard).ToHaveCSSAsync("opacity", "1");
        await Assertions.Expect(ordinaryCard).ToHaveCSSAsync("clip-path", "none");
    }

    [Fact]
    public async Task ProductionAnalyticsUsesDistinctSemanticSeriesColors()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var chart = page.Locator("[data-use-case-id='production-analytics']");
        var milling = chart.Locator("rect[data-series='milling']").First;
        var turning = chart.Locator("rect[data-series='turning']").First;
        await Assertions.Expect(milling).ToBeVisibleAsync();
        var millingFill = await milling.EvaluateAsync<string>("element => getComputedStyle(element).fill");
        var turningFill = await turning.EvaluateAsync<string>("element => getComputedStyle(element).fill");
        Assert.NotEqual(millingFill, turningFill);
        Assert.DoesNotContain(millingFill, new[] { "rgb(0, 0, 0)", "rgba(0, 0, 0, 1)" });
        Assert.DoesNotContain(turningFill, new[] { "rgb(0, 0, 0)", "rgba(0, 0, 0, 1)" });
    }

    [Fact]
    public async Task IconLibrarySelectionUpdatesVisibleWorkflowIconsWithoutResettingCardState()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var icons = page.Locator("[data-theme-workflow-icon] [data-slot='icon']");
        await Assertions.Expect(icons.First).ToBeVisibleAsync();
        Assert.True(await icons.CountAsync() >= 6);
        foreach (var useCase in new[] { "production-capacity", "operator-profile", "quotation-files", "shipping-handoff", "quotation-actions", "inspection-camera" })
            await Assertions.Expect(page.Locator($"[data-use-case-id='{useCase}'] [data-theme-workflow-icon]").First).ToBeVisibleAsync();
        await Assertions.Expect(icons.First).ToHaveAttributeAsync("data-library", "lucide");
        var semanticIcons = page.Locator("[data-theme-semantic-icon] [data-slot='icon']");
        await Assertions.Expect(semanticIcons.First).ToBeVisibleAsync();
        Assert.True(await semanticIcons.CountAsync() >= 8);
        await Assertions.Expect(page.Locator("[data-use-case-id='drawing-attachment'] [data-theme-semantic-icon='file']").First).ToBeVisibleAsync();
        await Assertions.Expect(semanticIcons.First).ToHaveAttributeAsync("data-library", "lucide");

        var profileName = page.Locator("[data-use-case-id='operator-profile'] input").First;
        await profileName.FillAsync("Kanda T.");
        await page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^Icon library") }).ClickAsync();
        await page.GetByTestId("theme-icon-library-phosphor").ClickAsync();

        await Assertions.Expect(icons.First).ToHaveAttributeAsync("data-library", "phosphor");
        await Assertions.Expect(semanticIcons.First).ToHaveAttributeAsync("data-library", "phosphor");
        Assert.True(await semanticIcons.EvaluateAllAsync<bool>("nodes => nodes.every(node => node.dataset.library === 'phosphor')"));
        await Assertions.Expect(profileName).ToHaveValueAsync("Kanda T.");
        Assert.Equal("phosphor", await page.GetByTestId("theme-preview-scope").GetAttributeAsync("data-theme-icon-library"));
    }

    [Fact]
    public async Task ThaiLocaleTranslatesRepresentativeWorkflowsWithoutResettingInteractiveState()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var profileName = page.Locator("[data-use-case-id='operator-profile'] input").First;
        await profileName.FillAsync("กานดา ท.");

        await page.GetByTestId("locale-thai").ClickAsync();

        await Assertions.Expect(page.Locator("[data-use-case-id='production-capacity']")).ToContainTextAsync("กำลังการผลิตรายสัปดาห์");
        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile']")).ToContainTextAsync("บันทึกโปรไฟล์");
        await Assertions.Expect(page.Locator("[data-use-case-id='quotation-files']")).ToContainTextAsync("วางแบบงานผลิตที่นี่");
        await Assertions.Expect(page.Locator("[data-use-case-id='inspection-table']")).ToContainTextAsync("ค่าที่วัดได้");
        await Assertions.Expect(page.Locator("[data-use-case-id='quality-alert']")).ToContainTextAsync("ระงับเพื่อรอตรวจสอบ");
        await Assertions.Expect(page.Locator("[data-use-case-id='assistant-conversation']")).ToContainTextAsync("ผู้ช่วย MALIEV");
        await Assertions.Expect(page.Locator("[data-use-case-id='production-planning-suite']")).ToContainTextAsync("ศูนย์วางแผนการผลิต");
        await Assertions.Expect(page.Locator("[data-console='production']")).ToContainTextAsync("แผนกำลังการผลิตมีความพร้อม");
        await Assertions.Expect(profileName).ToHaveValueAsync("กานดา ท.");
        await Assertions.Expect(page.Locator("[data-use-case-id='production-capacity']")).Not.ToContainTextAsync("Weekly capacity");
    }

    [Fact]
    public async Task CuratedChartsCredentialsAndMediaHaveThreeVisibleInteractiveUses()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        await Assertions.Expect(page.Locator("[data-use-case-id='production-analytics'] .shadcn-chart")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='quality-trend'] .shadcn-chart")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='order-mix'] .shadcn-chart")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='api-credentials'] .shadcn-secret-input")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='machine-password'] .shadcn-secret-input")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='webhook-secret'] .shadcn-secret-input")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='drawing-preview'] .shadcn-aspect-ratio")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='inspection-camera'] .shadcn-aspect-ratio")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='customer-proof'] .shadcn-aspect-ratio")).ToBeVisibleAsync();

        var bar = page.Locator("[data-use-case-id='production-analytics'] rect[data-series='milling']").First;
        await bar.HoverAsync();
        var tooltip = page.Locator("[data-use-case-id='production-analytics'] [data-slot='chart-tooltip-content']");
        await Assertions.Expect(tooltip).ToContainTextAsync("W1");
        await Assertions.Expect(tooltip).ToContainTextAsync("Milling");
        await Assertions.Expect(tooltip).ToContainTextAsync("42");
        await Assertions.Expect(tooltip).ToContainTextAsync("Turning");
        await Assertions.Expect(tooltip).ToContainTextAsync("31");
        await Assertions.Expect(tooltip).ToHaveAttributeAsync("data-active-point", "0");

        var credential = page.Locator("[data-use-case-id='api-credentials'] .shadcn-secret-input");
        await Assertions.Expect(credential).ToHaveAttributeAsync("data-revealed", "false");
        await credential.GetByRole(AriaRole.Button, new() { Name = "Show API key", Exact = true }).ClickAsync();
        await Assertions.Expect(credential).ToHaveAttributeAsync("data-revealed", "true");
    }

    [Fact]
    public async Task RotatingIntegrationCredentialGeneratesAndDisplaysANewKey()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='api-credentials']");
        var credential = card.Locator(".shadcn-secret-input");
        var input = credential.Locator("input");

        await credential.GetByRole(AriaRole.Button, new() { Name = "Show API key", Exact = true }).ClickAsync();
        var originalKey = await input.InputValueAsync();
        Assert.Equal("example-maliev-credential-0001", originalKey);
        await page.GetByTestId("locale-thai").ClickAsync();

        var rotateButton = card.GetByRole(AriaRole.Button, new() { Name = "เปลี่ยนคีย์", Exact = true });
        await rotateButton.ClickAsync();
        await Assertions.Expect(input).Not.ToHaveValueAsync(originalKey);
        var firstReplacement = await input.InputValueAsync();
        Assert.Equal("example-maliev-credential-0002", firstReplacement);

        await rotateButton.ClickAsync();
        await Assertions.Expect(input).Not.ToHaveValueAsync(firstReplacement);
        Assert.Equal("example-maliev-credential-0003", await input.InputValueAsync());
        await Assertions.Expect(card.GetByRole(AriaRole.Status)).ToContainTextAsync("คีย์ทดแทนพร้อมใช้งาน");
    }

    [Fact]
    public async Task VerifyingMachineAccessTransitionsTheActionToCompletedState()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='machine-password']");
        var action = card.GetByRole(AriaRole.Button, new() { Name = "Verify access", Exact = true });
        await card.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(card.Locator("[data-slot='alert']")).ToHaveCountAsync(0);

        await action.ClickAsync();

        await Assertions.Expect(card.Locator("[data-slot='alert']")).ToContainTextAsync("Maintenance mode is available for 10 minutes.");
        await Assertions.Expect(card.GetByRole(AriaRole.Button, new() { Name = "Access verified", Exact = true })).ToBeDisabledAsync();
    }

    [Fact]
    public async Task CuratedChartSubmitButtonsSwitchAndDropzoneExposeCompleteFeedback()
    {
        await using var context = await NewContextAsync(1569, 1032);
        var page = await OpenAsync(context);

        var chart = page.Locator("[data-use-case-id='production-analytics']");
        await chart.Locator("rect[data-series='milling']").First.HoverAsync();
        await Assertions.Expect(chart.Locator("[data-slot='chart-tooltip-content']")).ToContainTextAsync("W1");
        await Assertions.Expect(chart.Locator("[data-slot='chart-tooltip-content']")).ToContainTextAsync("42");

        var profile = page.Locator("[data-use-case-id='operator-profile']");
        var save = profile.Locator("button[data-operation-state]");
        await Assertions.Expect(save).ToHaveTextAsync("Save profile");
        await RecordOperationStatesAsync(save);
        await save.EvaluateAsync("element => element.click()");
        await Assertions.Expect(profile.GetByRole(AriaRole.Status)).ToContainTextAsync("Profile saved");
        await Assertions.Expect(save).ToContainTextAsync("Save profile", new() { Timeout = 2_000 });
        await AssertOperationStatesAsync(save);

        var handoff = page.Locator("[data-use-case-id='shipping-handoff']");
        var confirm = handoff.Locator("button[data-operation-state]");
        await Assertions.Expect(confirm).ToHaveTextAsync("Confirm address");
        await RecordOperationStatesAsync(confirm);
        await confirm.EvaluateAsync("element => element.click()");
        await Assertions.Expect(handoff.GetByRole(AriaRole.Status)).ToContainTextAsync("Handoff saved for dispatch.");
        await Assertions.Expect(confirm).ToContainTextAsync("Confirm address", new() { Timeout = 4_000 });
        await AssertOperationStatesAsync(confirm);

        var switchControl = page.GetByRole(AriaRole.Switch, new() { Name = "Use quiet hours", Exact = true });
        var switchTrack = switchControl.Locator("xpath=parent::*");
        Assert.True(await switchTrack.EvaluateAsync<bool>("element => parseFloat(getComputedStyle(element).width) >= 34"));
        var switchDiagnostic = await switchControl.EvaluateAsync<string>("element => { const style = getComputedStyle(element); return JSON.stringify({ width: style.width, height: style.height, radius: style.borderRadius }); }");
        Assert.True(await switchControl.EvaluateAsync<bool>("element => { const style = getComputedStyle(element); return parseFloat(style.borderRadius) >= parseFloat(style.height) / 2 - 1; }"), switchDiagnostic);

        var dropzone = page.Locator("[data-use-case-id='quotation-files']");
        await dropzone.Locator("input[type=file]").SetInputFilesAsync(new FilePayload
        {
            Name = "fixture.step",
            MimeType = "application/octet-stream",
            Buffer = new byte[1_024]
        });
        await Assertions.Expect(dropzone).ToContainTextAsync("fixture.step");
        await Assertions.Expect(dropzone).ToContainTextAsync("100%", new() { Timeout = 3_000 });
        await Assertions.Expect(dropzone.Locator("[data-slot='attachment']")).ToHaveCountAsync(1);
        await dropzone.GetByRole(AriaRole.Button, new() { Name = "Remove fixture.step", Exact = true }).ClickAsync();
        await Assertions.Expect(dropzone.Locator("[data-slot='attachment']")).ToHaveCountAsync(0);
        await Assertions.Expect(dropzone).ToContainTextAsync("No production drawings selected");
    }

    private static Task RecordOperationStatesAsync(ILocator button) => button.EvaluateAsync("""
        element => {
            element.__operationStates = [element.getAttribute('data-operation-state')];
            new MutationObserver(records => {
                for (const record of records) {
                    if (record.attributeName === 'data-operation-state') {
                        element.__operationStates.push(record.oldValue);
                    }
                }
                element.__operationStates.push(element.getAttribute('data-operation-state'));
            }).observe(element, {
                attributes: true,
                attributeFilter: ['data-operation-state'],
                attributeOldValue: true
            });
        }
        """);

    private static async Task AssertOperationStatesAsync(ILocator button)
    {
        var states = await button.EvaluateAsync<string[]>("element => element.__operationStates");
        Assert.Contains("busy", states);
        Assert.Contains("success", states);
        Assert.Equal("idle", states[^1]);
    }

    [Fact]
    public async Task CuratedUploadProgressFillMatchesItsLivePercentage()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var progress = page.Locator("[data-use-case-id='quotation-files'] [data-slot='progress']");

        await page.WaitForFunctionAsync("""
            () => {
                const progress = document.querySelector("[data-use-case-id='quotation-files'] [data-slot='progress']");
                const value = Number(progress?.getAttribute('aria-valuenow'));
                return value >= 5 && value <= 95;
            }
            """);
        var measurement = await progress.EvaluateAsync<double[]>("""
            element => {
                const value = Number(element.getAttribute('aria-valuenow'));
                const track = element.querySelector('[data-slot="progress-track"]').getBoundingClientRect();
                const indicator = element.querySelector('[data-slot="progress-indicator"]').getBoundingClientRect();
                return [value / 100, indicator.width / track.width];
            }
            """);

        Assert.InRange(measurement[1], measurement[0] - 0.02, measurement[0] + 0.02);
    }

    [Fact]
    public async Task MachineCellProgressFillMatchesItsDisplayedSpindleLoad()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='machine-cell']");
        var progress = card.Locator("[data-slot='progress']");

        await page.WaitForFunctionAsync("""
            () => {
                const progress = document.querySelector("[data-use-case-id='machine-cell'] [data-slot='progress']");
                const value = Number(progress?.getAttribute('aria-valuenow'));
                return value >= 5 && value <= 95;
            }
            """);
        var measurement = await progress.EvaluateAsync<double[]>("""
            element => {
                const value = Number(element.getAttribute('aria-valuenow'));
                const track = element.querySelector('[data-slot="progress-track"]').getBoundingClientRect();
                const indicator = element.querySelector('[data-slot="progress-indicator"]').getBoundingClientRect();
                return [value, indicator.width / track.width];
            }
            """);
        var displayedPercent = double.Parse(
            (await card.Locator("[data-testid='machine-load-percent']").InnerTextAsync()).TrimEnd('%'),
            CultureInfo.InvariantCulture);

        Assert.Equal(displayedPercent, measurement[0]);
        Assert.InRange(measurement[1], measurement[0] / 100 - 0.02, measurement[0] / 100 + 0.02);
    }

    [Fact]
    public async Task ReviewerDueTooltipMaintainsReadableContrastInTheThemePreview()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var details = page.Locator("[data-use-case-id='reviewer-details']");

        await details.GetByRole(AriaRole.Button, new() { Name = "Due 15:30", Exact = true }).ClickAsync();
        var tooltip = page.GetByRole(AriaRole.Tooltip);
        await Assertions.Expect(tooltip).ToContainTextAsync("45 minutes remaining");

        var colors = await tooltip.EvaluateAsync<string[]>("""
            element => {
                const style = getComputedStyle(element);
                const arrowStyle = getComputedStyle(element.querySelector('[data-slot="tooltip-arrow"]'));
                return [style.color, style.backgroundColor, arrowStyle.backgroundColor];
            }
            """);

        Assert.NotEqual(colors[0], colors[1]);
        Assert.Equal(colors[1], colors[2]);
    }

    [Fact]
    public async Task ProductionContactDialogRemainsHiddenUntilItsPortalIsReady()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.NoPreference);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='contact-dialog']");
        await card.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(card).ToBeVisibleAsync();
        await page.EvaluateAsync("""
            () => {
                window.__contactDialogPresentation = { animationStarts: 0, frames: [] };
                document.addEventListener('animationstart', event => {
                    if (event.target.matches("[data-use-case-id='contact-dialog'] [data-slot='dialog-content']"))
                        window.__contactDialogPresentation.animationStarts++;
                });
                const trigger = document.querySelector("[data-use-case-id='contact-dialog'] [data-slot='dialog-trigger']");
                trigger.addEventListener('click', () => {
                    const started = performance.now();
                    const sample = now => {
                        const content = document.querySelector("[data-use-case-id='contact-dialog'] [data-slot='dialog-content']");
                        if (content) {
                            const portal = content.closest("[data-slot='dialog-portal']");
                            const style = getComputedStyle(content);
                            const box = content.getBoundingClientRect();
                            window.__contactDialogPresentation.frames.push({
                                elapsed: now - started,
                                visible: style.visibility !== 'hidden' && style.display !== 'none' && style.opacity !== '0' && (!portal || getComputedStyle(portal).opacity !== '0') && box.width > 0 && box.height > 0,
                                promoted: portal?.matches(':popover-open') ?? false,
                                portalReady: portal?.hasAttribute('data-shadcn-dialog-ready') ?? false,
                                portalPopover: portal?.getAttribute('popover') ?? null,
                                portalOpacity: portal ? getComputedStyle(portal).opacity : null
                            });
                        }
                        if (now - started < 500) requestAnimationFrame(sample);
                    };
                    requestAnimationFrame(sample);
                }, { once: true, capture: true });
            }
            """);

        await card
            .GetByRole(AriaRole.Button, new() { Name = "Edit contact", Exact = true })
            .ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog)).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(600);
        var presentation = await page.EvaluateAsync<string>("""
            () => {
                const state = window.__contactDialogPresentation;
                return JSON.stringify({
                    animationStarts: state.animationStarts,
                    unpromotedVisible: state.frames.some(frame => frame.visible && !frame.promoted),
                    frames: state.frames
                });
            }
            """);

        Assert.False(presentation.Contains("\"unpromotedVisible\":true", StringComparison.Ordinal), presentation);
        Assert.Contains("\"animationStarts\":1", presentation, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DrawingAttachmentShowsDeterminateAndIndeterminateUploadProgress()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var attachment = page.Locator("[data-use-case-id='drawing-attachment']");
        var progress = attachment.Locator("[data-slot='attachment-progress']");

        await Assertions.Expect(progress).ToHaveCountAsync(2);
        await Assertions.Expect(attachment.Locator("[data-slot='attachment-progress'][data-state='indeterminate']")).ToHaveCountAsync(1);
        await Assertions.Expect(progress.Nth(0)).ToHaveAttributeAsync("aria-valuenow", new Regex("^[0-9]+(?:\\.[0-9]+)?$"));
        await Assertions.Expect(progress.Nth(1)).Not.ToHaveAttributeAsync("aria-valuenow", new Regex(".+"));
        var indeterminateFill = await progress.Nth(1).EvaluateAsync<double>("element => element.firstElementChild.getBoundingClientRect().width / element.getBoundingClientRect().width");
        Assert.InRange(indeterminateFill, 0.39, 0.41);
    }

    [Fact]
    public async Task ComplexCuratedWorkflowsReceiveWideSpansWithoutNestedFrames()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        foreach (var id in new[] { "drawing-attachment", "quality-alert", "project-questionnaire" })
            Assert.Equal("2", await page.Locator($"[data-use-case-item='{id}']").GetAttributeAsync("data-column-span"));

        var questionnaire = page.Locator("[data-use-case-id='project-questionnaire']");
        await Assertions.Expect(questionnaire.GetByText("Interactive questionnaire", new() { Exact = true })).ToHaveCountAsync(0);
        Assert.Equal("1px", await questionnaire.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.Equal("0px", await questionnaire.Locator("form[data-slot='questionnaire']").EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.Equal("1px", await questionnaire.Locator("[data-slot='questionnaire-choice']").First.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));

        var dataTableFrame = page.Locator("[data-use-case-id='quotation-data-table'] .shadcn-data-table-frame");
        Assert.Equal("0px", await dataTableFrame.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
    }

    [Fact]
    public async Task CuratedQuestionnaireUsesSemanticActionsAndResponsiveTextInput()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var questionnaire = page.Locator("[data-use-case-id='project-questionnaire']");

        await questionnaire.Locator("[data-slot='questionnaire-choice']").First.ClickAsync();
        await questionnaire.Locator("[data-slot='questionnaire-next']").ClickAsync();

        var previous = questionnaire.Locator("[data-slot='questionnaire-previous']");
        var skip = questionnaire.Locator("[data-slot='questionnaire-skip']");
        var submit = questionnaire.Locator("[data-slot='questionnaire-submit']");
        await Assertions.Expect(previous).ToHaveAttributeAsync("data-variant", "outline");
        await Assertions.Expect(skip).ToHaveAttributeAsync("data-variant", "ghost");
        await Assertions.Expect(submit).ToHaveAttributeAsync("data-variant", "primary");

        var input = questionnaire.Locator("[data-slot='questionnaire-input']");
        await input.EvaluateAsync("element => { window.__questionnaireBusyTransitions = 0; const form = element.closest('form'); new MutationObserver(() => window.__questionnaireBusyTransitions++).observe(form, { attributes: true, attributeFilter: ['aria-busy'] }); }");
        await input.PressSequentiallyAsync("Customer tolerance applies", new() { Delay = 5 });

        await Assertions.Expect(input).ToHaveValueAsync("Customer tolerance applies");
        Assert.Null(await questionnaire.Locator("form[data-slot='questionnaire']").GetAttributeAsync("aria-busy"));
        Assert.Equal(0, await page.EvaluateAsync<int>("window.__questionnaireBusyTransitions"));
    }

    [Fact]
    public async Task QuestionnaireSeparatesPromptCopyFromItsAnswerChoices()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var questionnaire = page.Locator("[data-use-case-id='project-questionnaire']");

        var promptGap = await questionnaire.EvaluateAsync<double>("""
            element => {
                const description = element.querySelector('[data-slot="questionnaire-description"]').getBoundingClientRect();
                const choices = element.querySelector('[data-slot="questionnaire-choices"]').getBoundingClientRect();
                return choices.top - description.bottom;
            }
            """);

        Assert.InRange(promptGap, 11, 13);
    }

    [Fact]
    public async Task QuestionnaireCompletesWithSubmittedAndSkippedAnswerSummaries()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var questionnaire = page.Locator("[data-use-case-id='project-questionnaire']");

        await questionnaire.Locator("[data-slot='questionnaire-choice']").Filter(new() { HasText = "Quality" }).ClickAsync();
        await questionnaire.Locator("[data-slot='questionnaire-next']").ClickAsync();
        await questionnaire.Locator("[data-slot='questionnaire-input']").FillAsync("Customer tolerance applies");
        await questionnaire.Locator("[data-slot='questionnaire-submit']").ClickAsync();

        var summary = questionnaire.GetByRole(AriaRole.Region, new() { Name = "Review submission summary", Exact = true });
        await Assertions.Expect(summary).ToContainTextAsync("3 / 3");
        await Assertions.Expect(summary).ToContainTextAsync("Quality");
        await Assertions.Expect(summary).ToContainTextAsync("Customer tolerance applies");
        await Assertions.Expect(questionnaire.Locator("form[data-slot='questionnaire']")).ToHaveCountAsync(0);

        await summary.GetByRole(AriaRole.Button, new() { Name = "Start another review", Exact = true }).ClickAsync();
        await questionnaire.Locator("[data-slot='questionnaire-choice']").Filter(new() { HasText = "Machining" }).ClickAsync();
        await questionnaire.Locator("[data-slot='questionnaire-next']").ClickAsync();
        await questionnaire.Locator("[data-slot='questionnaire-skip']").ClickAsync();

        summary = questionnaire.GetByRole(AriaRole.Region, new() { Name = "Review submission summary", Exact = true });
        await Assertions.Expect(summary).ToContainTextAsync("Machining");
        await Assertions.Expect(summary).ToContainTextAsync("No inspection notes added");
    }

    [Fact]
    public async Task QuotationTableShowsItsActivePageSizeAndKeepsOneDataColumnVisible()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var table = page.Locator("[data-use-case-id='quotation-data-table']");
        var pageSize = table.Locator("select[data-slot='data-table-page-size']");

        await Assertions.Expect(pageSize).ToHaveValueAsync("3");
        Assert.Equal("3", await pageSize.EvaluateAsync<string>("element => element.selectedOptions[0].textContent.trim()"));

        var visibilityToggles = table.Locator("input[data-column-visibility]");
        var toggleCount = await visibilityToggles.CountAsync();
        Assert.True(toggleCount > 1);
        for (var index = 0; index < toggleCount - 1; index++)
            await visibilityToggles.Nth(index).UncheckAsync();

        var finalVisibleToggle = table.Locator("input[data-column-visibility]:checked");
        await Assertions.Expect(finalVisibleToggle).ToHaveCountAsync(1);
        await Assertions.Expect(finalVisibleToggle).ToBeDisabledAsync();
        await Assertions.Expect(table.Locator("th[data-column]")).ToHaveCountAsync(1);
        await Assertions.Expect(table.Locator("tbody tr[data-row-key] td[data-column]")).ToHaveCountAsync(3);

        await Assertions.Expect(table).ToContainTextAsync("Page 1 of 3");
        await table.GetByRole(AriaRole.Button, new() { Name = "Next", Exact = true }).ClickAsync();
        await Assertions.Expect(table).ToContainTextAsync("Page 2 of 3");
        await Assertions.Expect(table.Locator("tbody tr[data-row-key]")).ToHaveCountAsync(3);
    }

    [Fact]
    public async Task ReportedWorkflowExamplesUseTheirAvailableSpaceAndRemainEditableAndReadable()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        var chart = page.Locator("[data-use-case-id='production-analytics'] .theme-production-chart");
        await chart.ScrollIntoViewIfNeededAsync();
        var chartRatio = await chart.EvaluateAsync<double>("element => { const content = element.closest('[data-slot=card-content]'); const style = getComputedStyle(content); const available = content.clientWidth - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight); return element.getBoundingClientRect().width / available; }");
        Assert.True(chartRatio >= .98, $"Production chart used only {chartRatio:P0} of the available card content width.");

        var issue = page.Locator("[data-use-case-id='issue-report']");
        var subject = issue.Locator("input");
        var details = issue.Locator("textarea");
        await subject.FillAsync("Surface finish evidence missing");
        await details.FillAsync("Attach the latest three-trace measurement before release.");
        await Assertions.Expect(subject).ToHaveValueAsync("Surface finish evidence missing");
        await Assertions.Expect(details).ToHaveValueAsync("Attach the latest three-trace measurement before release.");

        var marker = page.Locator("[data-use-case-id='conversation-marker']");
        await Assertions.Expect(marker.Locator(".theme-marker-conversation [data-slot='bubble']")).ToHaveCountAsync(2);
        await Assertions.Expect(page.Locator("[data-use-case-id='quality-alert'] .theme-quality-checklist li")).ToHaveCountAsync(2);

        var reviewerLabel = page.Locator("[data-use-case-id='assigned-reviewers'] .theme-runway-reviewers li > span").First;
        await Assertions.Expect(reviewerLabel).ToHaveCSSAsync("white-space", "nowrap");
        var destination = page.Locator("[data-use-case-id='dispatch-confirmation'] .theme-runway-summary dt").First;
        await Assertions.Expect(destination).ToHaveCSSAsync("white-space", "nowrap");

        var typingWords = page.Locator("[data-use-case-id='assistant-conversation'] .theme-typing-word");
        Assert.True(await typingWords.CountAsync() > 5);
        Assert.True(await typingWords.EvaluateAllAsync<bool>("elements => elements.every(element => { const glyphs = [...element.children]; return glyphs.length < 2 || glyphs.every(glyph => Math.abs(glyph.getBoundingClientRect().top - glyphs[0].getBoundingClientRect().top) < 1); })"));
    }

    [Fact]
    public async Task GuidanceTooltipShowsItsArrowWithoutAnInternalScrollbar()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var guidance = page.Locator("[data-use-case-id='tooltip-guidance']");
        var trigger = guidance.GetByRole(AriaRole.Button, new() { Name = "Diameter", Exact = true });
        await trigger.ScrollIntoViewIfNeededAsync();
        await trigger.HoverAsync();

        var tooltip = page.GetByRole(AriaRole.Tooltip);
        await Assertions.Expect(tooltip).ToBeVisibleAsync();
        var result = await tooltip.EvaluateAsync<double[]>("element => { const style = getComputedStyle(element); const arrow = element.querySelector('[data-slot=tooltip-arrow]').getBoundingClientRect(); return [style.overflowY === 'visible' ? 1 : 0, element.scrollHeight - element.clientHeight, arrow.width, arrow.height]; }");
        Assert.Equal(1, result[0]);
        Assert.True(result[2] > 0 && result[3] > 0, "Tooltip arrow must render with measurable geometry.");
    }

    [Fact]
    public async Task CuratedWorkflowActionsExposeVisibleStateAndReviewerExpansion()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        var shipping = page.Locator("[data-use-case-id='shipping-handoff']");
        await shipping.GetByRole(AriaRole.Button, new() { Name = "Confirm address", Exact = true }).ClickAsync();
        await Assertions.Expect(shipping.GetByRole(AriaRole.Status)).ToContainTextAsync("Handoff saved");

        var deposit = page.Locator("[data-use-case-id='deposit-approval']");
        await deposit.GetByRole(AriaRole.Button, new() { Name = "Approve deposit", Exact = true }).ClickAsync();
        await Assertions.Expect(deposit.GetByRole(AriaRole.Status)).ToContainTextAsync("Deposit approved");

        var dispatch = page.Locator("[data-use-case-id='dispatch-confirmation']");
        await dispatch.GetByRole(AriaRole.Button, new() { Name = "Confirm dispatch", Exact = true }).ClickAsync();
        await Assertions.Expect(dispatch.GetByRole(AriaRole.Status)).ToContainTextAsync("Dispatch confirmed");

        var reviewers = page.Locator("[data-use-case-id='assigned-reviewers']");
        var count = reviewers.GetByRole(AriaRole.Button, new() { Name = "Show four more reviewers", Exact = true });
        await count.ClickAsync();
        await Assertions.Expect(count).ToHaveCountAsync(0);
        await Assertions.Expect(reviewers.Locator("[data-slot='avatar']")).ToHaveCountAsync(7);
        var hoveredAvatar = reviewers.Locator("[data-slot='avatar']").Nth(3);
        await hoveredAvatar.HoverAsync();
        Assert.Equal("3", await hoveredAvatar.EvaluateAsync<string>("element => getComputedStyle(element).zIndex"));
        await Assertions.Expect(reviewers.GetByRole(AriaRole.Button, new() { Name = "Show four more reviewers", Exact = true })).ToBeVisibleAsync(new() { Timeout = 7_000 });
        await Assertions.Expect(reviewers.Locator("[data-slot='avatar']")).ToHaveCountAsync(3);

        var details = page.Locator("[data-use-case-id='reviewer-details']");
        await details.GetByRole(AriaRole.Button, new() { Name = "Due 15:30", Exact = true }).ClickAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Tooltip)).ToContainTextAsync("45 minutes remaining");
    }

    [Fact]
    public async Task QualityDepositAndReviewerActionsProduceObservableResults()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        var quality = page.Locator("[data-use-case-id='quality-alert']");
        await quality.GetByRole(AriaRole.Button, new() { Name = "Open report", Exact = true }).ClickAsync();
        await Assertions.Expect(quality.GetByRole(AriaRole.Region, new() { Name = "Inspection report" })).ToBeVisibleAsync();

        var deposit = page.Locator("[data-use-case-id='deposit-approval']");
        await deposit.GetByRole(AriaRole.Button, new() { Name = "Approve deposit", Exact = true }).ClickAsync();
        await deposit.GetByRole(AriaRole.Button, new() { Name = "Review", Exact = true }).ClickAsync();
        await Assertions.Expect(deposit.GetByRole(AriaRole.Button, new() { Name = "Approve deposit", Exact = true })).ToBeEnabledAsync();
        await Assertions.Expect(deposit.GetByRole(AriaRole.Status)).ToContainTextAsync("pending approval");

        var reviewers = page.Locator("[data-use-case-id='assigned-reviewers']");
        await reviewers.GetByRole(AriaRole.Button, new() { Name = "Manage reviewers", Exact = true }).ClickAsync();
        await Assertions.Expect(reviewers.GetByRole(AriaRole.Region, new() { Name = "Reviewer management" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task TypingInConversationDoesNotRepositionThePreviewCanvas()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var preview = page.Locator(".theme-preview-region");
        var input = page.Locator("[data-use-case-id='assistant-conversation'] .theme-runway-composer input");
        await input.ScrollIntoViewIfNeededAsync();
        await input.FocusAsync();
        var before = await preview.EvaluateAsync<double>("element => element.scrollTop");
        var beforeLayout = await input.EvaluateAsync<string>("element => JSON.stringify({ input: element.getBoundingClientRect().top, item: element.closest('[data-slot=bento-item]').getBoundingClientRect().top, preview: element.closest('.theme-preview-region').getBoundingClientRect().top, height: element.closest('.theme-preview-region').scrollHeight, active: document.activeElement === element })");
        await input.PressSequentiallyAsync("stable composer", new() { Delay = 20 });
        var after = await preview.EvaluateAsync<double>("element => element.scrollTop");
        var afterLayout = await input.EvaluateAsync<string>("element => JSON.stringify({ input: element.getBoundingClientRect().top, item: element.closest('[data-slot=bento-item]').getBoundingClientRect().top, preview: element.closest('.theme-preview-region').getBoundingClientRect().top, height: element.closest('.theme-preview-region').scrollHeight, active: document.activeElement === element })");
        Assert.True(Math.Abs(after - before) <= 1, $"scroll {before} -> {after}; before {beforeLayout}; after {afterLayout}");
    }

    [Fact]
    public async Task CuratedActionControlsAreNotDecorativeDeadEnds()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        var capacity = page.Locator("[data-use-case-id='production-capacity']");
        await capacity.GetByRole(AriaRole.Button, new() { Name = "Review plan", Exact = true }).ClickAsync();
        await Assertions.Expect(capacity.GetByRole(AriaRole.Region, new() { Name = "Capacity plan review" })).ToBeVisibleAsync();

        var profile = page.Locator("[data-use-case-id='operator-profile']");
        await profile.GetByRole(AriaRole.Button, new() { Name = "Save profile", Exact = true }).ClickAsync();
        await Assertions.Expect(profile.GetByRole(AriaRole.Status)).ToContainTextAsync("Profile saved");

        var attachment = page.Locator("[data-use-case-id='drawing-attachment']");
        await attachment.GetByRole(AriaRole.Button, new() { Name = "Cancel drawing upload", Exact = true }).ClickAsync();
        await Assertions.Expect(attachment.GetByRole(AriaRole.Status)).ToContainTextAsync("cancelled");

        var navigation = page.Locator("[data-use-case-id='work-order-navigation']");
        await navigation.GetByRole(AriaRole.Button, new() { Name = "Open process editor", Exact = true }).ClickAsync();
        var processEditor = page.GetByRole(AriaRole.Dialog, new() { Name = "Process plan editor", Exact = true });
        await Assertions.Expect(processEditor).ToBeVisibleAsync();
        await processEditor.GetByRole(AriaRole.Button, new() { Name = "Save revision", Exact = true }).ClickAsync();
        await Assertions.Expect(processEditor).ToBeHiddenAsync();
        await Assertions.Expect(navigation.GetByRole(AriaRole.Status)).ToContainTextAsync("Process revision saved for review");

        var quotation = page.Locator("[data-use-case-id='quotation-actions']");
        await quotation.GetByRole(AriaRole.Button, new() { Name = "Actions", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Menuitem, new() { Name = "Duplicate revision", Exact = true }).ClickAsync();
        await Assertions.Expect(quotation.GetByRole(AriaRole.Status)).ToContainTextAsync("duplicated");

        var drawing = page.Locator("[data-use-case-id='file-context']");
        await drawing.GetByRole(AriaRole.Button, new() { Name = "Download revision C", Exact = true }).ClickAsync();
        await Assertions.Expect(drawing.GetByRole(AriaRole.Status)).ToContainTextAsync("download prepared");
        await drawing.GetByRole(AriaRole.Button, new() { Name = "Open viewer", Exact = true }).ClickAsync();
        var viewer = page.GetByRole(AriaRole.Dialog).Filter(new() { HasText = "3D drawing review" });
        await Assertions.Expect(viewer).ToBeVisibleAsync();
        await viewer.GetByRole(AriaRole.Button, new() { Name = "Rotate model", Exact = true }).ClickAsync();
        await Assertions.Expect(viewer.GetByText("Rotation 45°", new() { Exact = true })).ToBeVisibleAsync();
        await viewer.GetByRole(AriaRole.Button, new() { Name = "Approve drawing", Exact = true }).ClickAsync();
        await Assertions.Expect(viewer).ToBeHiddenAsync();
        await Assertions.Expect(drawing.GetByRole(AriaRole.Status)).ToContainTextAsync("approved for inspection");

        var qualityConsole = page.Locator("[data-console='quality']");
        await qualityConsole.GetByRole(AriaRole.Button, new() { Name = "Save review", Exact = true }).ClickAsync();
        await Assertions.Expect(qualityConsole.GetByRole(AriaRole.Status)).ToContainTextAsync("saved as a draft");

        var handoffConsole = page.Locator("[data-console='handoff']");
        await handoffConsole.Locator("input[type='checkbox']").CheckAsync();
        await handoffConsole.GetByRole(AriaRole.Button, new() { Name = "Save handoff", Exact = true }).ClickAsync();
        await Assertions.Expect(handoffConsole.GetByRole(AriaRole.Status)).ToContainTextAsync("saved with recipient confirmation");
    }

    [Fact]
    public async Task DenseWorkflowCardsKeepActionsChartsAndTablesInsideReadableBounds()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        Assert.Equal("2", await page.Locator("[data-use-case-item='conversation-marker']").GetAttributeAsync("data-column-span"));
        foreach (var id in new[] { "drawing-attachment", "file-context" })
        {
            var card = page.Locator($"[data-use-case-id='{id}']");
            Assert.True(await card.EvaluateAsync<bool>("element => Array.from(element.querySelectorAll('[data-slot=attachment-action]')).every(action => { const a = action.getBoundingClientRect(); const attachment = action.closest('[data-slot=attachment]').getBoundingClientRect(); return a.left >= attachment.left && a.right <= attachment.right; })"));
        }

        var console = page.Locator("[data-console='production']");
        Assert.Equal("1", await console.Locator(".theme-operations-console__overview").EvaluateAsync<string>("element => getComputedStyle(element).gridTemplateColumns.split(' ').length.toString()"));
        var completed = console.Locator("rect[data-series='completed']").First;
        var inspected = console.Locator("rect[data-series='inspected']").First;
        var completedFill = await completed.EvaluateAsync<string>("element => getComputedStyle(element).fill");
        var inspectedFill = await inspected.EvaluateAsync<string>("element => getComputedStyle(element).fill");
        Assert.NotEqual(completedFill, inspectedFill);
        Assert.DoesNotContain(completedFill, new[] { "rgb(0, 0, 0)", "rgba(0, 0, 0, 1)" });
        Assert.DoesNotContain(inspectedFill, new[] { "rgb(0, 0, 0)", "rgba(0, 0, 0, 1)" });
        Assert.True(await console.Locator(".shadcn-table-container").EvaluateAsync<bool>("element => element.scrollWidth <= element.clientWidth + 1"));
    }

    [Fact]
    public async Task HandoffStatusBadgesUseTheThemeRadiusInsteadOfOvalGeometry()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var console = page.Locator("[data-console='handoff']");
        await console.ScrollIntoViewIfNeededAsync();
        await console.GetByRole(AriaRole.Tab, new() { Name = "Overview", Exact = true }).ClickAsync();
        var badge = console.Locator(".theme-handoff-console__timeline .shadcn-badge").First;
        await Assertions.Expect(badge).ToBeVisibleAsync();

        var geometry = await badge.EvaluateAsync<double[]>("element => { const style = getComputedStyle(element); return [parseFloat(style.borderRadius), element.getBoundingClientRect().height]; }");

        Assert.True(geometry[0] < geometry[1] / 2, $"Expected a rounded badge, but its radius {geometry[0]}px made its {geometry[1]}px height fully oval.");
    }

    [Fact]
    public async Task AnimatedIssueFieldsAndSidebarTypographyControlsRemainReadable()
    {
        await using var context = await NewContextAsync(1569, 1032);
        var page = await OpenAsync(context);

        var issue = page.Locator("[data-use-case-id='issue-report']");
        await issue.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(issue).ToBeVisibleAsync();
        await Assertions.Expect(issue.Locator("[data-animated-input]")).ToHaveCountAsync(1);
        await Assertions.Expect(issue.Locator("[data-animated-textarea]")).ToHaveCountAsync(1);
        var typedDetails = issue.Locator("[data-animated-textarea] [data-typing-text]");
        await Assertions.Expect(typedDetails).ToHaveCountAsync(1, new() { Timeout = 5000 });
        var glyphs = typedDetails.Locator(".theme-typing-glyph");
        Assert.True(await glyphs.CountAsync() > 20);
        Assert.Equal("Measured 24.982 mm against 25.000 ±0.010 mm.", (await typedDetails.InnerTextAsync()).Trim());
        var firstDelay = await glyphs.First.EvaluateAsync<string>("element => getComputedStyle(element).animationDelay");
        var lastDelay = await glyphs.Last.EvaluateAsync<string>("element => getComputedStyle(element).animationDelay");
        Assert.NotEqual(firstDelay, lastDelay);

        await OpenAdvancedAsync(page, "theme-typography-section");
        await OpenAdvancedAsync(page, "theme-advanced-typography");
        var weight = page.GetByTestId("theme-role-body-weight");
        Assert.True((await weight.BoundingBoxAsync())!.Width >= 96);
    }

    [Theory]
    [InlineData(1569, 1032)]
    [InlineData(390, 844)]
    public async Task AnimatedPrefillFontSizeMatchesTheControlBeforeInteraction(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var animatedControls = page.Locator("[data-animated-input]:has([data-typing-text]), [data-animated-textarea]:has([data-typing-text])");
        Assert.True(await animatedControls.CountAsync() > 0, "Expected animated prefilled controls in the workflow catalog.");

        foreach (var wrapper in await animatedControls.AllAsync())
        {
            var control = wrapper.Locator(":scope > :is(.shadcn-input, .shadcn-textarea)");
            var ink = wrapper.Locator(":scope > [data-typing-text]");
            var controlFontSize = await control.EvaluateAsync<string>("element => getComputedStyle(element).fontSize");

            await Assertions.Expect(ink).ToHaveCSSAsync("font-size", controlFontSize);
        }

        var issueDetails = page.Locator("[data-use-case-id='issue-report'] [data-animated-textarea]");
        var issueDetailsInk = issueDetails.Locator(":scope > [data-typing-text]");
        await Assertions.Expect(issueDetailsInk).ToHaveCountAsync(1, new() { Timeout = 8000 });
        var textareaFontSize = await issueDetails.Locator(":scope > .shadcn-textarea")
            .EvaluateAsync<string>("element => getComputedStyle(element).fontSize");
        await Assertions.Expect(issueDetailsInk).ToHaveCSSAsync("font-size", textareaFontSize);

        var factory = page.Locator("[data-use-case-id='shipping-handoff'] [data-animated-input]").First;
        var factoryInput = factory.Locator(":scope > .shadcn-input");
        var fontSizeBeforeInteraction = await factoryInput.EvaluateAsync<string>("element => getComputedStyle(element).fontSize");

        await factoryInput.ClickAsync(new() { Force = true });

        await Assertions.Expect(factory.Locator(":scope > [data-typing-text]")).ToHaveCountAsync(0);
        await Assertions.Expect(factoryInput).ToHaveCSSAsync("font-size", fontSizeBeforeInteraction);
    }

    [Fact]
    public async Task ConversationAcceptsUserMessagesAndRevealsEachAssistantReplyForwardOnce()
    {
        await using var context = await NewContextAsync(1440, 900);
        var page = await OpenAsync(context);
        var conversation = page.Locator("[data-use-case-id='assistant-conversation']");
        await conversation.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(conversation).ToBeVisibleAsync();
        var turns = conversation.Locator("[data-slot='message-scroller-item']");
        await Assertions.Expect(turns).ToHaveCountAsync(2);
        await conversation.Locator(".theme-runway-composer input").FillAsync("สถานะการตรวจสอบล่าสุดเป็นอย่างไร");
        await conversation.GetByRole(AriaRole.Button, new() { Name = "Send message" }).ClickAsync();
        await Assertions.Expect(turns).ToHaveCountAsync(4);
        await Assertions.Expect(conversation.GetByText("สถานะการตรวจสอบล่าสุดเป็นอย่างไร", new() { Exact = true })).ToBeVisibleAsync();
        var typingText = conversation.Locator(".theme-runway-typing-text").Last;
        Assert.DoesNotContain('\uFFFD', await typingText.InnerTextAsync());
        var typingGlyphs = typingText.Locator(".theme-typing-glyph");
        Assert.True(await typingGlyphs.CountAsync() > 20);
        await Assertions.Expect(typingGlyphs.Last).ToHaveCSSAsync("opacity", "1", new() { Timeout = 8000 });
    }

    [Fact]
    public async Task ConversationActionsReserveTheirLayoutAndSubmittedRepliesKeepTheirQuote()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var conversation = page.Locator("[data-use-case-id='assistant-conversation']");
        var assistantMessage = conversation.Locator(".shadcn-message[data-align='start']").First;
        await assistantMessage.ScrollIntoViewIfNeededAsync();

        var before = await assistantMessage.EvaluateAsync<string>("element => JSON.stringify({ height: element.getBoundingClientRect().height, nextTop: element.closest('[data-slot=message-scroller-item]').nextElementSibling?.getBoundingClientRect().top ?? 0 })");
        await assistantMessage.HoverAsync();
        var after = await assistantMessage.EvaluateAsync<string>("element => JSON.stringify({ height: element.getBoundingClientRect().height, nextTop: element.closest('[data-slot=message-scroller-item]').nextElementSibling?.getBoundingClientRect().top ?? 0 })");
        Assert.Equal(before, after);

        await assistantMessage.GetByRole(AriaRole.Button, new() { Name = "Reply to message" }).ClickAsync();
        await conversation.Locator(".theme-runway-composer input").FillAsync("Please confirm the final inspection owner.");
        await conversation.GetByRole(AriaRole.Button, new() { Name = "Send message" }).ClickAsync();

        var submittedReply = conversation.Locator(".shadcn-message[data-align='end']")
            .Filter(new() { HasText = "Please confirm the final inspection owner." });
        await Assertions.Expect(submittedReply).ToHaveCountAsync(1);
        var attachedQuote = submittedReply.Locator("[data-slot='message-reply-quote']");
        await Assertions.Expect(attachedQuote).ToHaveCountAsync(1);
        await Assertions.Expect(attachedQuote).ToContainTextAsync("Delivery remains Friday at 16:00.");
    }

    [Fact]
    public async Task DarkSecondaryMessagesStayBorderlessAndInputGroupsKeepSingleBoundaries()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await page.GetByTestId("documentation-theme-toggle").ClickAsync();

        var conversation = page.Locator("[data-use-case-id='assistant-conversation']");
        var assistantMessage = conversation.Locator(".shadcn-message").Filter(new() { HasText = "MALIEV Assistant" }).First;
        Assert.True(await assistantMessage.EvaluateAsync<bool>("element => { const bubble = element.querySelector('.shadcn-bubble[data-variant=secondary]'); const header = element.querySelector('.shadcn-message-header'); const content = element.querySelector('.shadcn-bubble-content'); if (!bubble || !header || !content) return false; const style = getComputedStyle(content); const headerBox = header.getBoundingClientRect(); const contentBox = content.getBoundingClientRect(); return style.borderTopWidth === '0px' && style.boxShadow === 'none' && Math.abs(headerBox.left - contentBox.left) <= 1; }"));

        var qualityConsole = page.Locator("[data-console='quality']");
        await qualityConsole.GetByRole(AriaRole.Tab, new() { Name = "Support", Exact = true }).ClickAsync();
        var handoffNote = qualityConsole.Locator("input[placeholder='Add an inspection note']");
        await handoffNote.ClickAsync();
        var focusStyles = await handoffNote.EvaluateAsync<string>("element => { const group = element.closest('.shadcn-input-group'); if (!group) return 'missing-group'; const inputStyle = getComputedStyle(element); const groupStyle = getComputedStyle(group); return [inputStyle.outlineStyle, inputStyle.outlineWidth, inputStyle.boxShadow, groupStyle.boxShadow, groupStyle.borderTopWidth].join('|'); }");
        var focusParts = focusStyles.Split('|');
        Assert.True(focusParts[0] == "none" || focusParts[1] == "0px", focusStyles);
        Assert.Equal("none", focusParts[2]);
        Assert.NotEqual("none", focusParts[3]);
        Assert.Equal("1px", focusParts[4]);
    }

    [Fact]
    public async Task ThemeStudioDoesNotRenderDocumentationScenarioCards()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await Assertions.Expect(page.Locator("[data-component-slug], [data-theme-scenario-host]")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-use-case-id='production-analytics'] .shadcn-chart")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ShuffleChangesOnlyTheCuratedThemeAndPreservesCanvasPositionAndCards()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var preview = page.Locator(".theme-preview-region");
        var bodyFont = page.GetByTestId("theme-font-slot-body").Locator("strong");
        var thaiFont = page.GetByTestId("theme-font-slot-thaifallback").Locator("strong");
        var codeFont = page.GetByTestId("theme-font-slot-code").Locator("strong");
        await preview.EvaluateAsync("element => element.scrollTop = 420");
        var beforeScroll = await preview.EvaluateAsync<double>("element => element.scrollTop");
        var beforeCards = await CardIdsAsync(page);
        var beforePreset = await page.GetByTestId("theme-preset").InnerTextAsync();
        var beforeBodyFont = await bodyFont.InnerTextAsync();
        var beforeThaiFont = await thaiFont.InnerTextAsync();
        var beforeCodeFont = await codeFont.InnerTextAsync();
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        Assert.Equal(beforeCards, await CardIdsAsync(page));
        Assert.NotEqual(beforePreset, await page.GetByTestId("theme-preset").InnerTextAsync());
        Assert.NotEqual(beforeBodyFont, await bodyFont.InnerTextAsync());
        Assert.NotEqual(beforeThaiFont, await thaiFont.InnerTextAsync());
        Assert.NotEqual(beforeCodeFont, await codeFont.InnerTextAsync());
        Assert.InRange(Math.Abs(await preview.EvaluateAsync<double>("element => element.scrollTop") - beforeScroll), 0, 2);
    }

    [Fact]
    public async Task ReusableRevealShowsVisibleCardsAndRevealsMoreAsThePreviewScrolls()
    {
        await using var context = await NewContextAsync(1440, 900);
        var page = await OpenAsync(context);
        var preview = page.Locator(".theme-preview-region");
        var group = page.GetByTestId("theme-bento");
        var reveals = group.Locator("[data-slot='reveal']");

        await Assertions.Expect(group).ToHaveAttributeAsync("data-reveal-reduced-motion", "false");
        Assert.Equal(45, await reveals.CountAsync());
        await page.WaitForTimeoutAsync(900);
        var revealedBefore = await reveals.EvaluateAllAsync<int>("items => items.filter(item => item.dataset.revealState === 'revealed').length");
        Assert.InRange(revealedBefore, 1, 36);

        await preview.EvaluateAsync("element => element.scrollTop = 1800");
        await page.WaitForTimeoutAsync(900);

        var revealedAfter = await reveals.EvaluateAllAsync<int>("items => items.filter(item => item.dataset.revealState === 'revealed').length");
        Assert.True(revealedAfter > revealedBefore, $"Expected scrolling to reveal more cards, but the count stayed at {revealedBefore}.");
    }

    [Fact]
    public async Task ShuffleUpdatesThemeWithoutRecreatingOrReanimatingCuratedComponents()
    {
        await using var context = await NewContextAsync(1440, 900);
        var page = await OpenAsync(context);
        var profile = page.Locator("[data-use-case-id='operator-profile']");
        await profile.EvaluateAsync("element => element.scrollIntoView({ block: 'center' })");
        await Assertions.Expect(profile).ToBeVisibleAsync();
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("preview-animation-pause").ClickAsync();
        var bento = page.GetByTestId("theme-bento");
        await Assertions.Expect(bento).ToHaveAttributeAsync("data-animation-paused", "true");
        var renderCycle = await bento.GetAttributeAsync("data-render-cycle");
        var name = profile.Locator("input").First;
        await name.FillAsync("Kanda T.");
        await page.WaitForTimeoutAsync(700);
        var profileReveal = profile.Locator("xpath=ancestor::*[@data-slot='reveal']");
        var profileRevealState = await profileReveal.GetAttributeAsync("data-reveal-state");
        Assert.Contains(profileRevealState, new[] { "pending", "revealed" });
        await page.EvaluateAsync("() => { const card = document.querySelector('[data-use-case-id=operator-profile]'); window.__themeShuffleCard = card; window.__themeShuffleTransitions = 0; document.querySelector('[data-testid=theme-bento]').addEventListener('transitionrun', event => { if (event.target?.dataset?.slot === 'reveal') window.__themeShuffleTransitions++; }); }");

        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        await page.WaitForTimeoutAsync(700);

        Assert.True(await page.EvaluateAsync<bool>("() => window.__themeShuffleCard === document.querySelector('[data-use-case-id=operator-profile]')"));
        await Assertions.Expect(name).ToHaveValueAsync("Kanda T.");
        await Assertions.Expect(bento).ToHaveAttributeAsync("data-render-cycle", renderCycle!);
        await Assertions.Expect(profileReveal).ToHaveAttributeAsync("data-reveal-state", profileRevealState!);
        Assert.Equal(0, await page.EvaluateAsync<int>("() => window.__themeShuffleTransitions"));
    }

    [Fact]
    public async Task ThemeAndTypographySettingsRemainScopedToThePreview()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var shellFont = await ComputedFontAsync(page.Locator(".documentation-header"));
        var shellBackground = await page.Locator(".theme-studio-shell").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        await OpenAdvancedAsync(page, "theme-typography-section");
        await OpenAdvancedAsync(page, "theme-advanced-typography");
        await page.GetByTestId("theme-font-search").FillAsync("DM Sans");
        await page.GetByTestId("theme-font-result-dm-sans").ClickAsync();
        Assert.Contains("DM Sans", await ComputedFontAsync(page.Locator("[data-use-case-id='operator-profile']")));
        Assert.Equal(shellFont, await ComputedFontAsync(page.Locator(".documentation-header")));
        Assert.Equal(shellBackground, await page.Locator(".theme-studio-shell").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }

    [Fact]
    public async Task TypographyOffersRecommendedFamiliesBeforeSearchAndRemainsCollapsible()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var section = page.GetByTestId("theme-typography-section");
        var trigger = section.Locator(":scope > [data-slot='collapsible-trigger']");

        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(page.GetByTestId("theme-typography-editor")).ToHaveCountAsync(1);
        await Assertions.Expect(page.GetByTestId("theme-font-slot-body")).ToHaveCountAsync(1);
        Assert.False(await page.EvaluateAsync<bool>("performance.getEntriesByType('resource').some(entry => entry.name.includes('data/google-fonts-catalog.json'))"));
        await trigger.ClickAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "true");
        await Assertions.Expect(page.GetByTestId("theme-font-results").Locator("li")).ToHaveCountAsync(10);
        await Assertions.Expect(page.GetByTestId("theme-font-results")).ToHaveAttributeAsync("aria-label", "Recommended font families");
        await Assertions.Expect(page.GetByTestId("theme-font-result-inter")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("theme-font-result-noto-sans-thai")).ToBeVisibleAsync();

        await page.GetByTestId("theme-font-search").FillAsync("Space Grotesk");
        await Assertions.Expect(page.GetByTestId("theme-font-result-space-grotesk")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("theme-font-results")).ToHaveAttributeAsync("aria-label", "Font search results");

        await trigger.ClickAsync();
        await Assertions.Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(page.GetByTestId("theme-font-results")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task SelectedThaiFallbackParticipatesInPreviewAndExportedFontStack()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            AcceptDownloads = true,
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await OpenAsync(context);
        await OpenAdvancedAsync(page, "theme-typography-section");

        await page.GetByTestId("theme-font-search").FillAsync("Space Grotesk");
        await Assertions.Expect(page.GetByTestId("theme-font-result-space-grotesk")).ToBeVisibleAsync();
        await page.GetByTestId("theme-font-result-space-grotesk").ClickAsync();
        await page.GetByTestId("theme-font-slot-thaifallback").ClickAsync();
        await page.GetByTestId("theme-font-search").FillAsync("Anuphan");
        await Assertions.Expect(page.GetByTestId("theme-font-result-anuphan")).ToBeVisibleAsync();
        await page.GetByTestId("theme-font-result-anuphan").ClickAsync();

        var preview = page.GetByTestId("theme-preview-scope");
        var effectiveStack = await preview.EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--shadcn-font-sans').trim()");
        AssertFamilyOrder(effectiveStack, "Space Grotesk", "Anuphan", "Noto Sans Thai", "ui-sans-serif", "system-ui");
        Assert.EndsWith("sans-serif", effectiveStack, StringComparison.Ordinal);
        Assert.Single(Regex.Matches(effectiveStack, "Noto Sans Thai", RegexOptions.CultureInvariant).Cast<Match>());
        Assert.Contains("Space Grotesk", await ComputedFontAsync(page.Locator("[data-use-case-id='production-capacity'] .shadcn-card-title")));

        await page.GetByTestId("locale-thai").ClickAsync();
        var thaiHeading = page.Locator("[data-use-case-id='production-capacity'] .shadcn-card-title");
        await Assertions.Expect(thaiHeading).ToBeVisibleAsync();
        await Assertions.Expect(thaiHeading).ToHaveTextAsync("กำลังการผลิต");
        AssertFamilyOrder(await ComputedFontAsync(thaiHeading), "Space Grotesk", "Anuphan", "Noto Sans Thai");

        await page.GetByTestId("theme-export-open").ClickAsync();
        var acknowledgement = page.GetByTestId("theme-export-warning-ack");
        if (await acknowledgement.CountAsync() > 0)
            await acknowledgement.CheckAsync();
        var download = await page.RunAndWaitForDownloadAsync(
            () => page.GetByTestId("theme-download").ClickAsync(),
            new() { Timeout = 90_000 });
        var downloadPath = await download.PathAsync();
        Assert.NotNull(downloadPath);
        using var archive = ZipFile.OpenRead(downloadPath);
        var cssEntry = archive.GetEntry("theme.css");
        Assert.NotNull(cssEntry);
        await using var cssStream = cssEntry.Open();
        using var reader = new StreamReader(cssStream, Encoding.UTF8);
        var exportedCss = await reader.ReadToEndAsync();
        Assert.Contains(
            "--shadcn-font-sans: 'Space Grotesk', 'Anuphan', 'Noto Sans Thai', ui-sans-serif, system-ui, sans-serif",
            exportedCss,
            StringComparison.Ordinal);

        await page.ReloadAsync();
        await page.GetByTestId("theme-studio").WaitForAsync();
        var restoredPreview = page.GetByTestId("theme-preview-scope");
        var restoredStack = await restoredPreview.EvaluateAsync<string>(
            "element => getComputedStyle(element).getPropertyValue('--shadcn-font-sans').trim()");
        AssertFamilyOrder(restoredStack, "Space Grotesk", "Anuphan", "Noto Sans Thai", "ui-sans-serif", "system-ui");
        await Assertions.Expect(page.GetByTestId("locale-thai")).ToHaveAttributeAsync("aria-pressed", "true");
        var restoredThaiHeading = page.Locator("[data-use-case-id='production-capacity'] .shadcn-card-title");
        await Assertions.Expect(restoredThaiHeading).ToHaveTextAsync("กำลังการผลิต");
        AssertFamilyOrder(await ComputedFontAsync(restoredThaiHeading), "Space Grotesk", "Anuphan", "Noto Sans Thai");
    }

    [Fact]
    public async Task GraphiteControlInheritsTheUniversalColorMode()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        await SelectOptionAsync(page, "theme-preset", "Graphite Control");

        await Assertions.Expect(page.GetByTestId("theme-visual-style-scope")).ToHaveAttributeAsync("data-color-treatment", "inherit");
        await Assertions.Expect(page.GetByTestId("theme-color-treatment")).ToContainTextAsync("Theme colors");
    }

    [Fact]
    public async Task ComposableVisualTreatmentsUpdateOnlyThePreviewScope()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var shell = page.Locator(".theme-studio-shell");
        var shellBackground = await shell.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");

        await SelectOptionAsync(page, "theme-visual-style", "Frosted glass");
        await SelectOptionAsync(page, "theme-color-treatment", "Vibrant night");
        await SelectOptionAsync(page, "theme-depth-treatment", "Floating");
        await SelectOptionAsync(page, "theme-motion-treatment", "Expressive");
        await SelectOptionAsync(page, "theme-style-intensity", "Strong");

        var scope = page.GetByTestId("theme-visual-style-scope");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-visual-style", "glass");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-color-treatment", "vibrant-dark");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-depth", "floating");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-motion", "expressive");
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-intensity", "strong");
        Assert.True(await scope.EvaluateAsync<bool>("element => Boolean(element.closest('[data-testid=theme-preview-scope]'))"));
        Assert.Equal(0, await page.Locator("[data-testid='theme-studio'] > [data-slot='visual-style-scope']").CountAsync());
        Assert.Equal(shellBackground, await shell.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }

    [Fact]
    public async Task FrostedGlassRefractsAnAmbientFieldWithoutVisibleBorders()
    {
        await using var context = await NewContextAsync(1900, 1032, ReducedMotion.NoPreference);
        var page = await OpenAsync(context);

        await SelectOptionAsync(page, "theme-visual-style", "Frosted glass");
        await SelectOptionAsync(page, "theme-depth-treatment", "Floating");

        var region = page.Locator(".theme-preview-region");
        var scope = page.GetByTestId("theme-visual-style-scope");
        var card = page.Locator("[data-use-case-id='production-capacity']");
        var input = page.Locator("[data-use-case-id='operator-profile'] .shadcn-input").First;
        var cardAlpha = await card.EvaluateAsync<double>("""
            element => {
                const color = getComputedStyle(element).backgroundColor;
                const modernAlpha = color.match(/\/\s*([\d.]+)/);
                if (modernAlpha) return Number(modernAlpha[1]);
                const legacyAlpha = color.match(/^rgba\([^,]+,[^,]+,[^,]+,\s*([\d.]+)\)$/);
                return legacyAlpha ? Number(legacyAlpha[1]) : 1;
            }
            """);

        var regionBackground = await region.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage");
        Assert.Contains("gradient", regionBackground, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("url(", regionBackground, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("none", await scope.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"));
        Assert.InRange(cardAlpha, .35, .70);
        Assert.NotEqual("none", await card.EvaluateAsync<string>("element => getComputedStyle(element).backdropFilter"));
        Assert.Equal("0px", await card.EvaluateAsync<string>("element => getComputedStyle(element).borderTopWidth"));
        Assert.Equal("none", await card.EvaluateAsync<string>("element => getComputedStyle(element, '::after').borderTopStyle"));
        Assert.Equal("rgba(0, 0, 0, 0)", await input.EvaluateAsync<string>("element => getComputedStyle(element).borderTopColor"));
    }

    [Fact]
    public async Task SpatialGlassRendersAnAmbientFieldAndLayeredComponentMaterials()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.NoPreference);
        var page = await OpenAsync(context);

        await SelectOptionAsync(page, "theme-visual-style", "Spatial glass");
        await SelectOptionAsync(page, "theme-depth-treatment", "Floating");

        var region = page.Locator(".theme-preview-region");
        var scope = page.GetByTestId("theme-visual-style-scope");
        var card = page.Locator("[data-use-case-id='production-capacity']");
        var input = page.Locator("[data-use-case-id='operator-profile'] .shadcn-input").First;

        await Assertions.Expect(scope).ToHaveAttributeAsync("data-visual-style", "liquid-glass");
        var regionBackground = await region.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage");
        Assert.Contains("gradient", regionBackground, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("url(", regionBackground, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("gradient", await card.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("url(", await card.EvaluateAsync<string>("element => getComputedStyle(element).backdropFilter"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("gradient", await input.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("url(", await input.EvaluateAsync<string>("element => getComputedStyle(element).backdropFilter"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SpatialGlassFillsPreviewGuttersAndUsesDirectionalOpticalEdges()
    {
        await using var context = await NewContextAsync(1900, 1032, ReducedMotion.NoPreference);
        var page = await OpenAsync(context);

        await SelectOptionAsync(page, "theme-visual-style", "Spatial glass");
        await SelectOptionAsync(page, "theme-depth-treatment", "Spatial");

        var region = page.Locator(".theme-preview-region");
        var scope = page.GetByTestId("theme-visual-style-scope");
        var card = page.Locator("[data-use-case-id='production-capacity']");

        Assert.Contains("gradient", await region.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("none", await scope.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"));
        Assert.Equal("rgba(0, 0, 0, 0)", await card.EvaluateAsync<string>("element => getComputedStyle(element).borderTopColor"));
        Assert.True(
            (await card.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage"))
                .Split("gradient", StringSplitOptions.None).Length >= 3,
            "Spatial glass cards should layer their material and directional edge gradients.");
        Assert.Equal("none", await card.EvaluateAsync<string>("element => getComputedStyle(element, '::after').borderTopStyle"));
    }

    [Theory]
    [InlineData("Frosted glass", "glass")]
    [InlineData("Spatial glass", "liquid-glass")]
    public async Task GlassMaterialsFillCardsAndControlsWithoutRevealFilterFlicker(string option, string expectedStyle)
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.NoPreference);
        var page = await OpenAsync(context);
        await SelectOptionAsync(page, "theme-visual-style", option);
        await page.GetByTestId("locale-thai").ClickAsync();

        var scope = page.GetByTestId("theme-visual-style-scope");
        var card = page.Locator("[data-use-case-id='quotation-files']");
        var dropzone = card.Locator(".shadcn-dropzone");
        var reveal = card.Locator("..");
        await card.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(scope).ToHaveAttributeAsync("data-visual-style", expectedStyle);
        await Assertions.Expect(reveal).ToHaveCSSAsync("transform", "none");
        Assert.DoesNotContain("transform", await reveal.EvaluateAsync<string>("element => getComputedStyle(element).transitionProperty"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("border-box", await card.EvaluateAsync<string>("element => getComputedStyle(element).backgroundOrigin"), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("border-box", await dropzone.EvaluateAsync<string>("element => getComputedStyle(element).backgroundOrigin"), StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual("fixed", await page.Locator(".theme-preview-region").EvaluateAsync<string>("element => getComputedStyle(element).backgroundAttachment"));
    }

    [Fact]
    public async Task ThaiInspectionCaptionHasReadableInsetAndLineBoxAlignment()
    {
        await using var context = await NewContextAsync(1569, 1032, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await page.GetByTestId("locale-thai").ClickAsync();
        var caption = page.Locator("[data-use-case-id='inspection-table'] .shadcn-table-caption");
        await caption.ScrollIntoViewIfNeededAsync();

        Assert.True(await caption.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).paddingInlineStart)") >= 12);
        Assert.True(await caption.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).lineHeight)") >= 18);
        Assert.Equal("start", await caption.EvaluateAsync<string>("element => getComputedStyle(element).textAlign"));
    }

    [Fact]
    public async Task TransferActionsKeepTextClearOfEdgesAtCompactSidebarWidth()
    {
        await using var context = await NewContextAsync(1121, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await OpenAdvancedAsync(page, "theme-advanced-transfer");
        var actions = page.Locator(".theme-transfer-actions");
        var buttons = actions.Locator(".shadcn-button");
        await Assertions.Expect(buttons).ToHaveCountAsync(3);

        Assert.True(await buttons.EvaluateAllAsync<bool>("nodes => nodes.every(node => { const style = getComputedStyle(node); return parseFloat(style.paddingInlineStart) >= 12 && parseFloat(style.paddingInlineEnd) >= 12 && node.scrollWidth <= node.clientWidth; })"));
        Assert.True(await actions.EvaluateAsync<bool>("element => getComputedStyle(element).gridTemplateColumns.split(' ').length <= 2"));
    }

    [Fact]
    public async Task RadiusHighContrastAndAnimationControlsAffectThePreviewOnly()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='production-capacity']");
        await page.GetByTestId("theme-radius-select").ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = "Sharp · 0", Exact = true }).ClickAsync();
        Assert.Equal(0, await card.EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).borderRadius)"));
        var normalBorder = await page.GetByTestId("theme-bento").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-border').trim()");
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("preview-high-contrast").ClickAsync();
        Assert.NotEqual(normalBorder, await page.GetByTestId("theme-bento").EvaluateAsync<string>("element => getComputedStyle(element).getPropertyValue('--shadcn-border').trim()"));
        await page.GetByTestId("preview-animation-pause").ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-bento")).ToHaveAttributeAsync("data-animation-paused", "true");
    }

    [Fact]
    public async Task OverlayExamplesAreFullyInteractive()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await page.Locator("[data-use-case-id='quotation-actions']").GetByText("Actions", new() { Exact = true }).ClickAsync();
        var menuItem = page.GetByText("Duplicate revision", new() { Exact = true });
        await Assertions.Expect(menuItem).ToBeVisibleAsync();
        var menuBackground = await menuItem.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        await menuItem.HoverAsync();
        var hoveredMenuBackground = await menuItem.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        var hoverDiagnostics = await menuItem.EvaluateAsync<string>("element => JSON.stringify({ hover: element.matches(':hover'), className: element.className, slot: element.dataset.slot, accent: getComputedStyle(element).getPropertyValue('--shadcn-accent') })");
        Assert.True(menuBackground != hoveredMenuBackground, hoverDiagnostics);
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(menuItem).ToBeHiddenAsync();
        await page.Locator("[data-use-case-id='contact-dialog']").GetByText("Edit contact", new() { Exact = true }).ClickAsync();
        var contactDialog = page.GetByRole(AriaRole.Dialog);
        await Assertions.Expect(contactDialog.GetByText("Production contact", new() { Exact = true })).ToBeVisibleAsync();
        await contactDialog.GetByRole(AriaRole.Button, new() { Name = "Close contact editor", Exact = true }).ClickAsync();
        await Assertions.Expect(contactDialog).ToBeHiddenAsync();
        var drawingReview = page.Locator("[data-use-case-id='file-context'] .theme-drawing-review");
        await drawingReview.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(drawingReview).ToBeInViewportAsync();
        await drawingReview.ClickAsync(new() { Button = MouseButton.Right });
        await Assertions.Expect(page.GetByText("Open drawing", new() { Exact = true })).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(1200);
        await Assertions.Expect(page.GetByText("Open drawing", new() { Exact = true })).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");
        await page.Locator("[data-use-case-id='dispatch-drawer']").GetByText("Review dispatch", new() { Exact = true }).ClickAsync();
        var drawer = page.Locator("[data-slot='drawer-content']");
        var drawerBox = await drawer.BoundingBoxAsync();
        Assert.NotNull(drawerBox);
        Assert.InRange(drawerBox!.Width, 638, 642);
        Assert.InRange(Math.Abs(drawerBox.X - (1440 - drawerBox.Width) / 2), 0, 1);
        Assert.Equal("row", await drawer.Locator("[data-slot='drawer-footer']").EvaluateAsync<string>("element => getComputedStyle(element).flexDirection"));
        Assert.Equal(new[] { "Cancel", "Confirm dispatch" }, await drawer.Locator("[data-slot='drawer-footer'] button").AllTextContentsAsync());
        await Assertions.Expect(drawer.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true })).ToHaveAttributeAsync("data-variant", "outline");
        await Assertions.Expect(drawer.GetByRole(AriaRole.Button, new() { Name = "Confirm dispatch", Exact = true })).ToHaveAttributeAsync("data-variant", "default");
        await drawer.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true }).ClickAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='reviewer-details']").GetByText("Kanda T.", new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='tooltip-guidance']").GetByText("Surface finish", new() { Exact = true })).ToBeEnabledAsync();
    }

    [Fact]
    public async Task OverlayWorkflowActionsPersistUsefulResultsOnTheirCards()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        var contact = page.Locator("[data-use-case-id='contact-dialog']");
        await contact.GetByRole(AriaRole.Button, new() { Name = "Edit contact", Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Button, new() { Name = "Save contact", Exact = true }).ClickAsync();
        await Assertions.Expect(contact.GetByRole(AriaRole.Status)).ToContainTextAsync("Production contact saved");

        var dispatch = page.Locator("[data-use-case-id='dispatch-drawer']");
        await dispatch.GetByRole(AriaRole.Button, new() { Name = "Review dispatch", Exact = true }).ClickAsync();
        var drawer = page.Locator("[data-slot='drawer-content']");
        await drawer.GetByRole(AriaRole.Button, new() { Name = "Confirm dispatch", Exact = true }).ClickAsync();
        await Assertions.Expect(drawer).ToBeHiddenAsync();
        await Assertions.Expect(dispatch.GetByRole(AriaRole.Status)).ToContainTextAsync("Dispatch released");

        var schedule = page.Locator("[data-use-case-id='delivery-sheet']");
        await schedule.GetByRole(AriaRole.Button, new() { Name = "Open schedule", Exact = true }).ClickAsync();
        var sheet = page.Locator("[data-slot='sheet-content']");
        await Assertions.Expect(sheet).ToBeVisibleAsync();
        await sheet.GetByRole(AriaRole.Button, new() { Name = "Save schedule", Exact = true }).ClickAsync();
        await Assertions.Expect(schedule.GetByRole(AriaRole.Status)).ToContainTextAsync("Delivery schedule saved");
    }

    [Fact]
    public async Task DeliverySheetBackdropCoversTheViewportAndActionsShareTheFooterRow()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var schedule = page.Locator("[data-use-case-id='delivery-sheet']");

        await schedule.GetByRole(AriaRole.Button, new() { Name = "Open schedule", Exact = true }).ClickAsync();

        var overlay = page.Locator("[data-slot='sheet-overlay']");
        var overlayBox = await overlay.BoundingBoxAsync();
        Assert.NotNull(overlayBox);
        Assert.InRange(overlayBox!.X, -0.5, 0.5);
        Assert.InRange(overlayBox.Y, -0.5, 0.5);
        Assert.InRange(overlayBox.Width, 1439.5, 1440.5);
        Assert.InRange(overlayBox.Height, 899.5, 900.5);

        var footer = page.Locator("[data-slot='sheet-footer']");
        Assert.Equal("row", await footer.EvaluateAsync<string>("element => getComputedStyle(element).flexDirection"));
        var actionBoxes = await footer.Locator("button").EvaluateAllAsync<double[]>("buttons => buttons.map(button => button.getBoundingClientRect().top)");
        Assert.Equal(2, actionBoxes.Length);
        Assert.InRange(Math.Abs(actionBoxes[0] - actionBoxes[1]), 0, 1);
    }

    [Fact]
    public async Task DeliverySheetActionsCommunicateSecondaryAndPrimaryIntent()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var schedule = page.Locator("[data-use-case-id='delivery-sheet']");

        await schedule.GetByRole(AriaRole.Button, new() { Name = "Open schedule", Exact = true }).ClickAsync();

        var sheet = page.Locator("[data-slot='sheet-content']");
        var cancel = sheet.GetByRole(AriaRole.Button, new() { Name = "Cancel", Exact = true });
        var save = sheet.GetByRole(AriaRole.Button, new() { Name = "Save schedule", Exact = true });
        await Assertions.Expect(cancel).ToHaveAttributeAsync("data-variant", "outline");
        await Assertions.Expect(save).ToHaveAttributeAsync("data-variant", "default");
        var backgrounds = await Task.WhenAll(
            cancel.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"),
            save.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));

        Assert.NotEqual(backgrounds[0], backgrounds[1]);
    }

    [Fact]
    public async Task QuotationActionsMenuEscapesTheRevealedCardClippingBoundary()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var card = page.Locator("[data-use-case-id='quotation-actions']");
        await card.ScrollIntoViewIfNeededAsync();

        await card.GetByRole(AriaRole.Button, new() { Name = "Actions", Exact = true }).ClickAsync();

        var menu = page.Locator("[data-slot='dropdown-menu-content']");
        await Assertions.Expect(menu).ToBeVisibleAsync();
        await Assertions.Expect(menu).ToHaveAttributeAsync("data-positioned", "true");
        Assert.True(await menu.EvaluateAsync<bool>("element => element.matches(':popover-open')"));
        Assert.True(await menu.EvaluateAsync<bool>("""
            element => {
                const box = element.getBoundingClientRect();
                const x = Math.min(innerWidth - 2, Math.max(2, box.left + Math.min(12, box.width / 2)));
                const y = Math.min(innerHeight - 2, Math.max(2, box.bottom - Math.min(12, box.height / 2)));
                const hit = document.elementFromPoint(x, y);
                return hit !== null && element.contains(hit);
            }
            """));
    }

    [Theory]
    [MemberData(nameof(ReleaseViewports))]
    public async Task EveryCuratedCardContainsItsVisibleControlsAndMaintainsReadableType(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, width <= 768);
        var page = await OpenAsync(context);
        var diagnostics = await page.Locator("[data-use-case-id]").EvaluateAllAsync<string>("""
            cards => JSON.stringify(cards.flatMap(card => {
                const cardBox = card.getBoundingClientRect();
                const issues = [];
                if (card.scrollWidth > card.clientWidth + 1) {
                    const culprit = Array.from(card.querySelectorAll('*')).find(element => element.scrollWidth > element.clientWidth + 1);
                    issues.push(`${culprit?.tagName}.${culprit?.className} ${culprit?.scrollWidth}/${culprit?.clientWidth}; ${card.dataset.useCaseId}: card overflow ${card.scrollWidth}/${card.clientWidth}`);
                }
                const title = card.querySelector('.shadcn-card-title');
                if (title && parseFloat(getComputedStyle(title).fontSize) > 24)
                    issues.push(`${card.dataset.useCaseId}: title ${getComputedStyle(title).fontSize}`);
                card.querySelectorAll('button, input, textarea, select, [role=combobox]').forEach(control => {
                    const style = getComputedStyle(control);
                    const box = control.getBoundingClientRect();
                    if (style.display === 'none' || style.visibility === 'hidden' || box.width === 0 || box.height === 0)
                        return;
                    const horizontalScroller = Array.from(control.parentElement?.closest('[data-use-case-id]')?.querySelectorAll('*') ?? [])
                        .find(candidate => candidate.contains(control) && candidate.scrollWidth > candidate.clientWidth + 1 && ['auto', 'scroll'].includes(getComputedStyle(candidate).overflowX));
                    if (!horizontalScroller && (box.left < cardBox.left - 1 || box.right > cardBox.right + 1))
                        issues.push(`${card.dataset.useCaseId}: ${control.tagName.toLowerCase()} outside card`);
                });
                return issues;
            }))
            """);
        Assert.True(diagnostics == "[]", diagnostics);
    }

    [Fact]
    public async Task CardsUsePackageComponentsAndCorrectPercentages()
    {
        await using var context = await NewContextAsync(1280, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile'].shadcn-card")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='operator-profile'] .shadcn-avatar-image")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='assistant-conversation'] .shadcn-message-group")).ToBeVisibleAsync();
        var machinePercent = await page.Locator("[data-use-case-id='machine-cell'] [data-testid='machine-load-percent']").InnerTextAsync();
        Assert.Matches("^([0-9]|[1-9][0-9]|100)%$", machinePercent);
        Assert.Equal("0", await page.Locator("[data-use-case-id='quotation-files'] .shadcn-dropzone-input").EvaluateAsync<string>("element => getComputedStyle(element).opacity"));
    }

    [Fact]
    public async Task ComprehensiveWorkflowConsolesRenderAndRemainInteractive()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var consoles = page.Locator("[data-console]");
        await Assertions.Expect(consoles).ToHaveCountAsync(3);
        await Assertions.Expect(consoles.Locator(".shadcn-tabs")).ToHaveCountAsync(3);
        await Assertions.Expect(consoles.Locator(".shadcn-accordion")).ToHaveCountAsync(3);
        var production = page.Locator("[data-console='production']");
        var quality = page.Locator("[data-console='quality']");
        var handoff = page.Locator("[data-console='handoff']");
        await Assertions.Expect(production.Locator("[data-overview='production'] .shadcn-chart")).ToHaveCountAsync(1);
        await Assertions.Expect(production.Locator("[data-overview='production'] .shadcn-table")).ToHaveCountAsync(1);
        await Assertions.Expect(quality.Locator("[data-overview='quality'] [aria-label='Inspection evidence']")).ToHaveCountAsync(1);
        await Assertions.Expect(handoff.Locator("[data-overview='handoff'] [aria-label='Delivery route']")).ToHaveCountAsync(1);
        await production.GetByRole(AriaRole.Tab, new() { Name = "Support", Exact = true }).ClickAsync();
        var note = production.GetByPlaceholder("Add a production note");
        await note.FillAsync("Customer requested inspection photos");
        await production.GetByRole(AriaRole.Button, new() { Name = "Add", Exact = true }).ClickAsync();
        await Assertions.Expect(production.GetByText("Production note added to WO-2418", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Theory]
    [MemberData(nameof(ReleaseViewports))]
    public async Task BentoIsResponsiveWithoutDocumentOverflow(int width, int height)
    {
        var errors = new List<string>();
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce, true);
        var page = await context.NewPageAsync();
        page.Console += (_, message) => { if (message.Type == "error") errors.Add(message.Text); };
        page.PageError += (_, error) => errors.Add(error);
        await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString());
        await page.GetByTestId("theme-bento").WaitForAsync();
        var overflow = await page.EvaluateAsync<double>("Math.max(document.documentElement.scrollWidth-document.documentElement.clientWidth,document.body.scrollWidth-document.body.clientWidth)");
        Assert.InRange(overflow, 0, 1);
        Assert.Equal(45, await page.Locator(".theme-bento__grid [data-use-case-id]").CountAsync());
        Assert.Equal(0, await page.Locator(".theme-bento__grid [data-component-slug]").CountAsync());
        if (width <= 640) { await OpenSettingsAsync(page); await Assertions.Expect(page.Locator(".theme-device-options")).ToBeVisibleAsync(); }
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Theory]
    [InlineData(1264, 1032, 64, 84)]
    [InlineData(390, 844, 40, 48)]
    public async Task ThemePreviewClearsTheAppBarAndSponsorActionStaysCompact(int width, int height, double minimumSponsorWidth, double maximumSponsorWidth)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var headerBox = await page.Locator(".documentation-header").BoundingBoxAsync();
        var headingBox = await page.GetByRole(AriaRole.Heading, new() { Name = "Workflow examples", Level = 1 }).BoundingBoxAsync();
        var sponsor = page.GetByTestId("documentation-kofi-link");
        var sponsorBox = await sponsor.BoundingBoxAsync();

        Assert.NotNull(headerBox);
        Assert.NotNull(headingBox);
        Assert.NotNull(sponsorBox);
        Assert.InRange(headingBox.Y - (headerBox.Y + headerBox.Height), 16, 32);
        Assert.InRange(sponsorBox.Width, minimumSponsorWidth, maximumSponsorWidth);
        Assert.InRange(sponsorBox.Height, 40, 48);
        await Assertions.Expect(sponsor).ToHaveAccessibleNameAsync("Support my work on Ko-fi (opens in a new tab)");
        await Assertions.Expect(sponsor.Locator(".documentation-kofi__cup")).ToBeVisibleAsync();
        await Assertions.Expect(sponsor.Locator(".documentation-kofi__badge")).ToHaveCountAsync(0);
        if (width > 768)
        {
            await Assertions.Expect(sponsor.Locator(".documentation-kofi__label")).ToBeVisibleAsync();
        }
        else
        {
            await Assertions.Expect(sponsor.Locator(".documentation-kofi__label")).ToBeHiddenAsync();
        }

        var headerAccessibility = await page.Locator(".documentation-header").RunAxe();
        Assert.DoesNotContain(headerAccessibility.Violations, violation => violation.Impact is "serious" or "critical");
    }

    [Theory]
    [InlineData(1440, 900)]
    [InlineData(390, 844)]
    public async Task ThemeStudioFiltersTheEvaluationRunwayWithoutNavigation(int width, int height)
    {
        await using var context = await NewContextAsync(width, height, ReducedMotion.Reduce);
        var page = await OpenAsync(context);

        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Workflow examples", Level = 1 })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Group, new() { Name = "Filter workflow examples by category" })).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Theme Studio", Level = 1 })).ToHaveCountAsync(0);
        Assert.Equal(45, await page.Locator("[data-use-case-id]").CountAsync());

        var urlBeforeFiltering = page.Url;
        var overlaysFilter = page.GetByTestId("theme-category-filter-overlays");
        await overlaysFilter.ClickAsync();

        await Assertions.Expect(overlaysFilter).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(page.Locator("[data-use-case-id]")).ToHaveCountAsync(9);
        await Assertions.Expect(page.Locator("[data-use-case-id='dispatch-confirmation']")).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[data-use-case-id='production-capacity']")).ToHaveCountAsync(0);
        Assert.Equal(urlBeforeFiltering, page.Url);

        await overlaysFilter.ClickAsync();

        await Assertions.Expect(overlaysFilter).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(page.Locator("[data-use-case-id]")).ToHaveCountAsync(45);
        Assert.Equal(urlBeforeFiltering, page.Url);

        if (width != 390) return;

        await OpenSettingsAsync(page);
        await Assertions.Expect(page.GetByTestId("theme-device-controls")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("locale-english")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("locale-thai")).ToBeVisibleAsync();
        Assert.True(await page.GetByTestId("theme-preset-status").EvaluateAsync<double>("element => parseFloat(getComputedStyle(element).fontSize)") >= 12);
    }

    [Fact]
    public async Task MobileSettingsEscapeClosesTheDrawerAndPreviewHasNoSeriousAxeViolations()
    {
        await using var context = await NewContextAsync(390, 844, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var toggle = page.GetByTestId("theme-controls-toggle");
        await toggle.ClickAsync();
        await Assertions.Expect(page.GetByTestId("theme-studio-sidebar")).ToHaveAttributeAsync("data-focus-return-id", "theme-settings-toggle");
        await page.Keyboard.PressAsync("Escape");
        await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "false");
        await Assertions.Expect(page.GetByTestId("theme-studio-sidebar")).ToBeHiddenAsync();
        foreach (var card in await page.Locator("[data-use-case-id]").AllAsync())
        {
            var axe = await card.RunAxe();
            Assert.DoesNotContain(axe.Violations, violation => violation.Impact is "serious" or "critical");
        }
    }

    private async Task<IBrowserContext> NewContextAsync(int width, int height, ReducedMotion motion = ReducedMotion.NoPreference, bool touch = false) => await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = width, Height = height }, ReducedMotion = motion, HasTouch = touch });
    private async Task<IPage> OpenAsync(IBrowserContext context) { var page = await context.NewPageAsync(); await page.GotoAsync(new Uri(server.BaseUri, "/theme").ToString()); await page.GetByTestId("theme-studio").WaitForAsync(); return page; }
    private static async Task OpenSettingsAsync(IPage page) { var toggle = page.GetByTestId("theme-controls-toggle"); if (string.Equals(await toggle.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal)) await toggle.ClickAsync(); }
    private static async Task OpenAdvancedAsync(IPage page, string testId) { var trigger = page.GetByTestId(testId).Locator(":scope > [data-slot='collapsible-trigger']"); if (string.Equals(await trigger.GetAttributeAsync("aria-expanded"), "false", StringComparison.Ordinal)) await trigger.ClickAsync(); }
    private static Task<string[]> CardIdsAsync(IPage page) => page.Locator(".theme-bento__grid [data-use-case-id]").EvaluateAllAsync<string[]>("nodes => nodes.map(node => node.dataset.useCaseId)");
    private static Task<int> MasonrySpanAsync(ILocator item) => item.EvaluateAsync<int>("element => Number.parseInt(element.style.getPropertyValue('--shadcn-bento-masonry-span'), 10)");

    private static async Task WaitForMasonrySpanAsync(ILocator item, Func<int, bool> predicate)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            if (predicate(await MasonrySpanAsync(item))) return;
            await Task.Delay(50);
        }

        Assert.Fail($"Masonry span did not reach the expected state. Current span: {await MasonrySpanAsync(item)}.");
    }

    private static async Task SelectOptionAsync(IPage page, string testId, string option)
    {
        await page.GetByTestId(testId).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
    }
    private static Task<string> ComputedFontAsync(ILocator locator) => locator.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");

    private static void AssertFamilyOrder(string stack, params string[] families)
    {
        var previous = -1;
        foreach (var family in families)
        {
            var current = stack.IndexOf(family, StringComparison.OrdinalIgnoreCase);
            Assert.True(current > previous, $"Expected {family} after index {previous} in {stack}.");
            previous = current;
        }
    }
}
