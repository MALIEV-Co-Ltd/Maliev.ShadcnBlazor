using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ConversationWorkflowBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task AttachmentUsesBundledRasterArtAndDefaultTypeFaces()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 900 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/attachment").ToString());
        await page.EvaluateAsync("document.fonts.ready");

        await Assertions.Expect(page.Locator("img.showcase-attachment-artwork")).ToHaveCountAsync(3);
        var imageSrc = await page.Locator("img.showcase-attachment-artwork").First.GetAttributeAsync("src");
        Assert.NotNull(imageSrc);
        Assert.Contains("images/attachments/", imageSrc, StringComparison.Ordinal);
        Assert.EndsWith(".png", imageSrc, StringComparison.OrdinalIgnoreCase);
        var firstImage = page.Locator("img.showcase-attachment-artwork").First;
        await firstImage.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(firstImage).ToHaveJSPropertyAsync("complete", true);
        Assert.True(await firstImage.EvaluateAsync<bool>("image => image.naturalWidth > 0"));
        await Assertions.Expect(page.Locator("svg.showcase-attachment-artwork")).ToHaveCountAsync(0);
        var sans = await page.Locator("[data-testid='component-preview-canvas']").EvaluateAsync<string>("element => getComputedStyle(element).fontFamily");
        Assert.Contains("Geist", sans, StringComparison.OrdinalIgnoreCase);
        Assert.True(await page.EvaluateAsync<bool>("document.fonts.check('16px \\\"Geist\\\"')"));
        Assert.True(await page.EvaluateAsync<bool>("document.fonts.check('16px \\\"JetBrains Mono\\\"')"));
        await Assertions.Expect(page.Locator(".showcase-attachment-file").First).ToHaveAttributeAsync("data-state", "uploading");
        var titleAnimation = await page.Locator(".showcase-attachment-file").First.Locator("[data-slot='attachment-title']").EvaluateAsync<string>("element => getComputedStyle(element).animationName");
        Assert.Contains("shadcn-attachment-title-pulse", titleAnimation, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PresentationDossiersMutateRealLogicalAndLiveState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            Permissions = ["clipboard-read", "clipboard-write"]
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/attachment").ToString());
        await Assertions.Expect(page.Locator("[data-slot='attachment-group']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-slot='attachment']")).ToHaveCountAsync(5);
        await Assertions.Expect(page.Locator("[data-slot='attachment-media'][data-variant='image']")).ToHaveCountAsync(3);
        var primaryAttachment = page.Locator(".showcase-attachment-file").First;
        await Assertions.Expect(primaryAttachment).ToHaveAttributeAsync("data-state", "uploading");
        await primaryAttachment.Locator("[data-slot='attachment-action']").FocusAsync();
        await Assertions.Expect(primaryAttachment.Locator("[data-slot='attachment-action']")).ToBeFocusedAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/bubble").ToString());
        await Assertions.Expect(page.Locator("[data-slot='bubble-group']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-slot='bubble']")).ToHaveCountAsync(5);
        await Assertions.Expect(page.Locator("[data-slot='bubble-reactions']")).ToHaveCountAsync(2);
        var incomingBubbles = page.Locator("[data-bubble-role='incoming']");
        await Assertions.Expect(incomingBubbles).ToHaveCountAsync(3);
        await Assertions.Expect(incomingBubbles.First).ToHaveAttributeAsync("data-variant", "secondary");
        await Assertions.Expect(incomingBubbles.First).ToHaveAttributeAsync("data-align", "start");
        await page.GetByTestId("control-bubble-variant").SelectOptionAsync("Tinted");
        for (var index = 0; index < await incomingBubbles.CountAsync(); index++)
            await Assertions.Expect(incomingBubbles.Nth(index)).ToHaveAttributeAsync("data-variant", "tinted");
        await page.GetByTestId("control-bubble-end").CheckAsync();
        var selectedIncomingBubble = page.Locator("[data-bubble-role='incoming']").Filter(new() { HasTextString = "I can group messages, switch sides, and keep the whole thread easy to scan." });
        await Assertions.Expect(selectedIncomingBubble).ToHaveAttributeAsync("data-align", "end");
        await page.Locator("#preview .component-code").HoverAsync();
        var copy = page.Locator("#preview .component-code").GetByTestId("copy-source");
        await copy.ClickAsync();
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copied", "true");
        await Assertions.Expect(page.Locator("#preview .component-code .code-token-tag")).Not.ToHaveCountAsync(0);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        await page.GetByTestId("control-marker-streaming").CheckAsync();
        var streamingMarker = page.Locator("[data-slot='marker'][role='status']");
        await Assertions.Expect(streamingMarker).ToHaveCountAsync(1);
        await Assertions.Expect(streamingMarker).ToHaveAttributeAsync("data-live", "true");
        await Assertions.Expect(streamingMarker.Locator("[data-slot='marker-icon'][data-streaming='true'] svg.showcase-marker-loader")).ToHaveCountAsync(1);
        await Assertions.Expect(streamingMarker.Locator("[data-slot='marker-content'][data-streaming='true']")).ToHaveCountAsync(1);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        await page.GetByTestId("control-message-end").CheckAsync();
        await page.Locator("[data-testid='component-preview-canvas']").EvaluateAsync("el => el.dir='rtl'");
        await Assertions.Expect(page.Locator("[data-slot='message'][data-align='end']")).ToHaveCountAsync(2);
    }

    [Fact]
    public async Task StreamingMarkerUsesLoaderAndShimmerButHonorsReducedMotion()
    {
        await using var animatedContext = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 640, Height = 700 }, ReducedMotion = ReducedMotion.NoPreference });
        var animatedPage = await animatedContext.NewPageAsync();
        await animatedPage.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        var animatedMarker = animatedPage.Locator("[data-slot='marker'][role='status']");
        await Assertions.Expect(animatedMarker.Locator("svg.showcase-marker-loader")).ToHaveCountAsync(1);
        Assert.Equal("shadcn-marker-spin", await animatedMarker.Locator("[data-slot='marker-icon']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        Assert.Equal("shadcn-marker-pulse", await animatedMarker.Locator("[data-slot='marker-content']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        await using var reducedContext = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 640, Height = 700 }, ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await reducedPage.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        var reducedMarker = reducedPage.Locator("[data-slot='marker'][role='status']");
        Assert.Equal("none", await reducedMarker.Locator("[data-slot='marker-icon']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        Assert.Equal("none", await reducedMarker.Locator("[data-slot='marker-content']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
    }

    [Fact]
    public async Task ScrollerTracksAppendUserIntentUnreadJumpAndPrepend()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 } });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/conversation-workflows").ToString());
        var scroller = page.GetByTestId("workflow-scroller");
        await scroller.WaitForAsync();
        await page.GetByTestId("scroll-message").ClickAsync();
        await Assertions.Expect(scroller.Locator("[data-slot='message-scroller-viewport']")).ToBeFocusedAsync();
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-autoscrolling", "");
        await Assertions.Expect(scroller.Locator("[data-slot='message-scroller-viewport']")).ToHaveAttributeAsync("data-autoscrolling", "");
        await Assertions.Expect(page.Locator("[data-message-id='turn-3']")).ToBeInViewportAsync();
        await page.GetByTestId("queue-missing-message").ClickAsync();
        await Assertions.Expect(page.GetByTestId("queued-message-result")).ToHaveTextAsync("missed");
        await page.GetByTestId("queue-before-first-message").ClickAsync();
        await Assertions.Expect(page.GetByTestId("first-message-result")).ToHaveTextAsync("handled");
        await Assertions.Expect(page.Locator("[data-testid='empty-workflow-scroller'] [data-message-id='turn-delayed']")).ToBeInViewportAsync();
        await page.GetByTestId("scroll-start").ClickAsync();
        await scroller.HoverAsync();
        await page.Mouse.WheelAsync(0, -500);
        await page.GetByTestId("append-message").ClickAsync();
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-unread", "true");
        await page.GetByTestId("jump-latest").FocusAsync();
        await Assertions.Expect(page.GetByTestId("jump-latest")).ToBeFocusedAsync();
        await page.Keyboard.PressAsync("Enter");
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-scrollable-end", "false");
        await page.GetByTestId("focus-end").ClickAsync();
        await Assertions.Expect(scroller.Locator("[data-slot='message-scroller-viewport']")).ToBeFocusedAsync();
        await Assertions.Expect(page.GetByTestId("scroller-state")).ToContainTextAsync("following:True");
        var held = await page.Locator("[data-message-id='turn-1']").BoundingBoxAsync();
        await page.GetByTestId("prepend-message").ClickAsync();
        var after = await page.Locator("[data-message-id='turn-1']").BoundingBoxAsync();
        Assert.NotNull(held); Assert.NotNull(after); Assert.InRange(Math.Abs(after.Y - held.Y), 0, 2);
    }

    [Fact]
    public async Task QuestionnaireValidatesBranchesResumesAndSubmitsThaiState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 375, Height = 667 }, ForcedColors = ForcedColors.Active });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/conversation-workflows").ToString());
        await page.GetByTestId("questionnaire-next").ClickAsync();
        await Assertions.Expect(page.Locator("[data-slot='questionnaire-error']:visible")).ToContainTextAsync("กรุณาเลือก");
        await page.WaitForTimeoutAsync(150);
        Assert.Null(await page.Locator("form[data-slot='questionnaire']").GetAttributeAsync("aria-busy"));
        Assert.Equal("numbers", await page.Locator("form[data-slot='questionnaire']").GetAttributeAsync("data-shortcuts"));
        var feature = page.Locator("input[value='feature']");
        await feature.FocusAsync();
        await feature.DispatchEventAsync("keydown", new { key = "1", isComposing = true });
        await Assertions.Expect(feature).Not.ToBeCheckedAsync();
        await page.Keyboard.PressAsync("2");
        await Assertions.Expect(feature).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByTestId("questionnaire-next")).ToBeEnabledAsync();
        await page.GetByTestId("questionnaire-next").EvaluateAsync("element => { element.click(); element.click(); }");
        await Assertions.Expect(page.Locator("fieldset[name='notes']")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("questionnaire-validation-count")).ToHaveTextAsync("2");
        var notes = page.Locator("[data-slot='questionnaire-input']");
        await Assertions.Expect(notes).ToBeFocusedAsync();
        await notes.EvaluateAsync("element => { element.value = 'first'; element.dispatchEvent(new Event('input', { bubbles: true })); element.value = 'blocked'; element.dispatchEvent(new Event('input', { bubbles: true })); }");
        await Assertions.Expect(page.GetByTestId("questionnaire-answer")).ToHaveTextAsync("blocked");
        await Assertions.Expect(page.GetByTestId("questionnaire-submit")).ToBeEnabledAsync();
        await page.GetByTestId("questionnaire-submit").ClickAsync();
        await Assertions.Expect(page.Locator("[data-slot='questionnaire-error']:visible")).ToContainTextAsync("ใช้ไม่ได้");
        await notes.FillAsync("ทดสอบ RTL");
        await Assertions.Expect(page.GetByTestId("questionnaire-answer")).ToHaveTextAsync("ทดสอบ RTL");
        await Assertions.Expect(page.GetByTestId("questionnaire-submit")).ToBeEnabledAsync();
        await page.GetByTestId("questionnaire-submit").EvaluateAsync("element => { element.click(); element.click(); }");
        await Assertions.Expect(page.GetByTestId("questionnaire-submit")).ToBeDisabledAsync();
        await Assertions.Expect(page.GetByTestId("questionnaire-result")).ToContainTextAsync("ทดสอบ RTL ครั้ง:1");
        await page.GetByTestId("resume-questionnaire").ClickAsync();
        await Assertions.Expect(page.Locator("fieldset[name='notes']")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByTestId("questionnaire-result")).ToContainTextAsync("ทำต่อจากรายละเอียด");
        Assert.Null(await page.Locator("form[data-slot='questionnaire']").GetAttributeAsync("aria-busy"));
        await page.GetByTestId("reset-questionnaire").ClickAsync();
        await Assertions.Expect(page.Locator("fieldset[name='scope']")).ToBeVisibleAsync();
    }
}
