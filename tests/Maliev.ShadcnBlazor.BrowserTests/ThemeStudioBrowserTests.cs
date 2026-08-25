using Deque.AxeCore.Playwright;
using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ThemeStudioBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    public static TheoryData<int, int> ReleaseViewports => new() { { 1440, 900 }, { 1024, 768 }, { 768, 1024 }, { 390, 844 }, { 320, 568 } };

    [Fact]
    public async Task BentoUsesOneOrderedScrollableCanvasWithoutMirrorsOrClippedBorders()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var bento = page.GetByTestId("theme-bento");
        var cards = bento.Locator("[data-use-case-id]");
        await Assertions.Expect(bento).ToBeVisibleAsync();
        Assert.Equal(37, await cards.CountAsync());
        Assert.Equal(37, (await CardIdsAsync(page)).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(0, await bento.Locator("[data-component-slug]").CountAsync());
        await Assertions.Expect(page.Locator("[data-mirror], [data-runway-track]")).ToHaveCountAsync(0);
        var first = cards.First;
        Assert.Equal("1px", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderLeftWidth"));
        Assert.Equal("1px", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderRightWidth"));
        Assert.NotEqual("rgba(0, 0, 0, 0)", await first.EvaluateAsync<string>("element => getComputedStyle(element).borderLeftColor"));
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

        var profileName = page.Locator("[data-use-case-id='operator-profile'] input").First;
        await profileName.FillAsync("Kanda T.");
        await page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("^Icon library") }).ClickAsync();
        await page.GetByTestId("theme-icon-library-phosphor").ClickAsync();

        await Assertions.Expect(icons.First).ToHaveAttributeAsync("data-library", "phosphor");
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
        await save.EvaluateAsync("element => element.click()");
        await Assertions.Expect(save).ToContainTextAsync("Saving");
        await Assertions.Expect(save).ToContainTextAsync("Saved", new() { Timeout = 2_000 });
        await Assertions.Expect(save).ToContainTextAsync("Save profile", new() { Timeout = 2_000 });

        var handoff = page.Locator("[data-use-case-id='shipping-handoff']");
        var confirm = handoff.Locator("button[data-operation-state]");
        await Assertions.Expect(confirm).ToHaveTextAsync("Confirm address");
        await confirm.EvaluateAsync("element => element.click()");
        await Assertions.Expect(confirm).ToContainTextAsync("Confirming");
        await Assertions.Expect(confirm).ToContainTextAsync("Confirmed", new() { Timeout = 2_000 });
        await Assertions.Expect(confirm).ToContainTextAsync("Confirm address", new() { Timeout = 2_000 });

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
    public async Task AnimatedIssueFieldsAndSidebarTypographyControlsRemainReadable()
    {
        await using var context = await NewContextAsync(1569, 1032);
        var page = await OpenAsync(context);

        var issue = page.Locator("[data-use-case-id='issue-report']");
        await Assertions.Expect(issue.Locator("[data-animated-input]")).ToHaveCountAsync(1);
        await Assertions.Expect(issue.Locator("[data-animated-textarea]")).ToHaveCountAsync(1);

        await OpenAdvancedAsync(page, "theme-typography-section");
        await OpenAdvancedAsync(page, "theme-advanced-typography");
        var weight = page.GetByTestId("theme-role-body-weight");
        Assert.True((await weight.BoundingBoxAsync())!.Width >= 96);
    }

    [Fact]
    public async Task ConversationAcceptsUserMessagesAndRevealsEachAssistantReplyForwardOnce()
    {
        await using var context = await NewContextAsync(1440, 900);
        var page = await OpenAsync(context);
        var conversation = page.Locator("[data-use-case-id='assistant-conversation']");
        var turns = conversation.Locator("[data-slot='message-scroller-item']");
        await Assertions.Expect(turns).ToHaveCountAsync(2);
        await conversation.Locator(".theme-runway-composer input").FillAsync("สถานะการตรวจสอบล่าสุดเป็นอย่างไร");
        await conversation.GetByRole(AriaRole.Button, new() { Name = "Send message" }).ClickAsync();
        await Assertions.Expect(turns).ToHaveCountAsync(4);
        await Assertions.Expect(conversation.GetByText("สถานะการตรวจสอบล่าสุดเป็นอย่างไร", new() { Exact = true })).ToBeVisibleAsync();
        var typingText = conversation.Locator(".theme-runway-typing-text").Last;
        Assert.Equal("1", await typingText.EvaluateAsync<string>("element => getComputedStyle(element).animationIterationCount"));
        Assert.DoesNotContain('\uFFFD', await typingText.InnerTextAsync());
        await Assertions.Expect(typingText).ToHaveCSSAsync("clip-path", "inset(0px 0% 0px 0px)", new() { Timeout = 8000 });
    }

    [Fact]
    public async Task DarkGhostMessagesAndInputGroupsKeepReadableSingleBoundaries()
    {
        await using var context = await NewContextAsync(1440, 900, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        await page.GetByTestId("documentation-theme-toggle").ClickAsync();

        var conversation = page.Locator("[data-use-case-id='assistant-conversation']");
        var assistantMessage = conversation.Locator(".shadcn-message").Filter(new() { HasText = "MALIEV Assistant" }).First;
        Assert.True(await assistantMessage.EvaluateAsync<bool>("element => { const bubble = element.querySelector('.shadcn-bubble[data-variant=ghost]'); const header = element.querySelector('.shadcn-message-header'); const content = element.querySelector('.shadcn-bubble-content'); if (!bubble || !header || !content) return false; const style = getComputedStyle(bubble); const headerBox = header.getBoundingClientRect(); const contentBox = content.getBoundingClientRect(); return style.borderTopStyle !== 'none' && style.borderTopColor !== 'rgba(0, 0, 0, 0)' && Math.abs(headerBox.left - contentBox.left) <= 1; }"));

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
        await preview.EvaluateAsync("element => element.scrollTop = 420");
        var beforeScroll = await preview.EvaluateAsync<double>("element => element.scrollTop");
        var beforeCards = await CardIdsAsync(page);
        var beforePreset = await page.GetByTestId("theme-preset").InnerTextAsync();
        await page.GetByTestId("theme-preset-shuffle").ClickAsync();
        Assert.Equal(beforeCards, await CardIdsAsync(page));
        Assert.NotEqual(beforePreset, await page.GetByTestId("theme-preset").InnerTextAsync());
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
        Assert.Equal(37, await reveals.CountAsync());
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
        await OpenAdvancedAsync(page, "theme-advanced-accessibility");
        await page.GetByTestId("preview-animation-pause").ClickAsync();
        var bento = page.GetByTestId("theme-bento");
        await Assertions.Expect(bento).ToHaveAttributeAsync("data-animation-paused", "true");
        var renderCycle = await bento.GetAttributeAsync("data-render-cycle");
        var profile = page.Locator("[data-use-case-id='operator-profile']");
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
        await Assertions.Expect(consoles.Locator(".shadcn-chart")).ToHaveCountAsync(3);
        await Assertions.Expect(consoles.Locator(".shadcn-table")).ToHaveCountAsync(3);
        await Assertions.Expect(consoles.Locator(".shadcn-accordion")).ToHaveCountAsync(3);
        var production = page.Locator("[data-console='production']");
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
        Assert.Equal(37, await page.Locator(".theme-bento__grid [data-use-case-id]").CountAsync());
        Assert.Equal(0, await page.Locator(".theme-bento__grid [data-component-slug]").CountAsync());
        if (width <= 640) { await OpenSettingsAsync(page); await Assertions.Expect(page.Locator(".theme-device-options")).ToBeHiddenAsync(); }
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public async Task MobileSettingsRestoresFocusAndPreviewHasNoSeriousAxeViolations()
    {
        await using var context = await NewContextAsync(390, 844, ReducedMotion.Reduce);
        var page = await OpenAsync(context);
        var toggle = page.GetByTestId("theme-controls-toggle");
        await toggle.ClickAsync();
        await page.GetByTestId("theme-sidebar-collapse").ClickAsync();
        await Assertions.Expect(toggle).ToBeFocusedAsync();
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

    private static async Task SelectOptionAsync(IPage page, string testId, string option)
    {
        await page.GetByTestId(testId).ClickAsync();
        await page.GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
    }
    private static Task<string> ComputedFontAsync(ILocator locator) => locator.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");
}
