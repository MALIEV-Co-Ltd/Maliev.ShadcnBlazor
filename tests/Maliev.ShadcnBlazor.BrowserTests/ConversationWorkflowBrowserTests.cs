using Maliev.ShadcnBlazor.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace Maliev.ShadcnBlazor.BrowserTests;

[Collection(BrowserCollection.Name)]
public sealed class ConversationWorkflowBrowserTests(ShowcaseServerFixture server, PlaywrightFixture playwright)
{
    [Fact]
    public async Task PresentationDossiersMutateRealLogicalAndLiveState()
    {
        await using var context = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 390, Height = 844 }, ReducedMotion = ReducedMotion.Reduce });
        var page = await context.NewPageAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/attachment").ToString());
        await Assertions.Expect(page.Locator("[data-slot='attachment-group']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-slot='attachment']")).ToHaveCountAsync(5);
        await Assertions.Expect(page.Locator("[data-slot='attachment-media'][data-variant='image']")).ToHaveCountAsync(3);
        await page.GetByTestId("control-attachment-state").SelectOptionAsync("Error");
        var primaryAttachment = page.Locator(".showcase-attachment-file").First;
        await Assertions.Expect(primaryAttachment).ToHaveAttributeAsync("data-state", "error");
        await primaryAttachment.Locator("[data-slot='attachment-action']").FocusAsync();
        await Assertions.Expect(primaryAttachment.Locator("[data-slot='attachment-action']")).ToBeFocusedAsync();

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/bubble").ToString());
        await Assertions.Expect(page.Locator("[data-slot='bubble-group']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-slot='bubble']")).ToHaveCountAsync(4);
        await Assertions.Expect(page.Locator("[data-slot='bubble-reactions']")).ToHaveCountAsync(2);
        await page.GetByTestId("control-bubble-end").CheckAsync();
        await Assertions.Expect(page.Locator("[data-slot='bubble'][data-variant='secondary']")).ToHaveAttributeAsync("data-align", "end");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        await page.GetByTestId("control-marker-streaming").CheckAsync();
        await Assertions.Expect(page.Locator("[data-slot='marker'][role='status']")).ToHaveCountAsync(1);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        await page.GetByTestId("control-message-end").CheckAsync();
        await page.Locator("[data-testid='component-preview-canvas']").EvaluateAsync("el => el.dir='rtl'");
        await Assertions.Expect(page.Locator("[data-slot='message'][data-align='end']")).ToHaveCountAsync(2);
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
