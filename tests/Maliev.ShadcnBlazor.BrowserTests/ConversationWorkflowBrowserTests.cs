using System.Globalization;
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
        var idleAttachmentShadow = await primaryAttachment.EvaluateAsync<string>("element => getComputedStyle(element).boxShadow");
        Assert.DoesNotContain("3px", idleAttachmentShadow, StringComparison.OrdinalIgnoreCase);
        await primaryAttachment.Locator("[data-slot='attachment-action']").FocusAsync();
        await Assertions.Expect(primaryAttachment.Locator("[data-slot='attachment-action']")).ToBeFocusedAsync();
        // The preview card intentionally stays visually quiet at rest and when a
        // child receives focus. The actionable control owns the keyboard ring.
        var focusedAttachmentOutline = await primaryAttachment.Locator("[data-slot='attachment-action']")
            .EvaluateAsync<string>("element => `${getComputedStyle(element).outlineWidth} ${getComputedStyle(element).outlineStyle}`");
        Assert.DoesNotContain("0px", focusedAttachmentOutline, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("solid", focusedAttachmentOutline, StringComparison.OrdinalIgnoreCase);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/bubble").ToString());
        await Assertions.Expect(page.Locator("[data-slot='bubble-group']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-slot='bubble']")).ToHaveCountAsync(5);
        await Assertions.Expect(page.Locator("[data-slot='bubble-reactions']")).ToHaveCountAsync(2);
        var incomingBubbles = page.Locator("[data-bubble-role='incoming']");
        await Assertions.Expect(incomingBubbles).ToHaveCountAsync(3);
        await Assertions.Expect(incomingBubbles.First).ToHaveAttributeAsync("data-variant", "ghost");
        await Assertions.Expect(incomingBubbles.First).ToHaveAttributeAsync("data-align", "start");
        var outgoingBubbles = page.Locator("[data-bubble-role='outgoing']");
        await Assertions.Expect(outgoingBubbles).ToHaveCountAsync(2);
        await Assertions.Expect(outgoingBubbles.First).ToHaveAttributeAsync("data-variant", "default");
        await page.ChooseOptionAsync("control-bubble-variant", "Tinted");
        for (var index = 0; index < await outgoingBubbles.CountAsync(); index++)
            await Assertions.Expect(outgoingBubbles.Nth(index)).ToHaveAttributeAsync("data-variant", "tinted");
        for (var index = 0; index < await incomingBubbles.CountAsync(); index++)
            await Assertions.Expect(incomingBubbles.Nth(index)).ToHaveAttributeAsync("data-variant", "ghost");
        await page.GetByTestId("control-bubble-end").CheckAsync();
        var selectedIncomingBubble = page.Locator("[data-bubble-role='incoming']").Filter(new() { HasTextString = "I can group messages, switch sides, and keep the whole thread easy to scan." });
        await Assertions.Expect(selectedIncomingBubble).ToHaveAttributeAsync("data-align", "end");
        var bubblePreview = page.GetByTestId("component-preview").First;
        var bubbleSourceDisclosure = bubblePreview.Locator("details[data-testid='example-source']");
        await Assertions.Expect(bubbleSourceDisclosure).Not.ToHaveAttributeAsync("open", "");
        await bubbleSourceDisclosure.Locator("summary").ClickAsync();
        await Assertions.Expect(bubbleSourceDisclosure).ToHaveAttributeAsync("open", "");
        var bubbleSource = bubbleSourceDisclosure.Locator("[data-slot='code-block']");
        await Assertions.Expect(bubbleSource).ToBeVisibleAsync();
        await bubbleSource.HoverAsync();
        var copy = bubbleSource.GetByTestId("copy-source");
        await copy.ClickAsync();
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copied", "true");
        await Assertions.Expect(bubbleSource.Locator(".code-token-tag")).Not.ToHaveCountAsync(0);
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copied", "false", new() { Timeout = 3000 });
        await copy.ClickAsync();
        await Assertions.Expect(copy).ToHaveAttributeAsync("data-copied", "true");

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        await page.GetByTestId("control-marker-streaming").CheckAsync();
        var streamingMarker = page.Locator("[data-slot='marker'][role='status']");
        await Assertions.Expect(streamingMarker).ToHaveCountAsync(1);
        await Assertions.Expect(streamingMarker).ToHaveAttributeAsync("data-live", "true");
        await Assertions.Expect(streamingMarker.Locator("[data-slot='marker-icon'][data-streaming='true'] .showcase-marker-loader")).ToHaveCountAsync(1);
        await Assertions.Expect(streamingMarker.Locator("[data-slot='marker-content'][data-streaming='true']")).ToHaveCountAsync(1);

        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        await page.Locator("[data-testid='component-preview-canvas']").EvaluateAsync("el => el.dir='rtl'");
        await Assertions.Expect(page.GetByTestId("control-message-end")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("[data-slot='message'][data-align='start']")).ToHaveCountAsync(3);
        await Assertions.Expect(page.Locator("[data-slot='message'][data-align='end']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator("[data-slot='message']").Nth(2).Locator("[data-slot='message-footer']")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task MessagePackageActionsCopyRepeatQuoteAndKeepStructuredGeometry()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce,
            Permissions = ["clipboard-read", "clipboard-write"]
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());
        await page.GetByTestId("control-message-footer-always").CheckAsync();

        var firstCopy = page.GetByTestId("message-copy").First;
        await Assertions.Expect(firstCopy).ToHaveAttributeAsync("data-copy-state", "idle");
        await firstCopy.ClickAsync();
        await Assertions.Expect(firstCopy).ToHaveAttributeAsync("data-copy-state", "copied");
        await Assertions.Expect(firstCopy).ToHaveAttributeAsync("aria-label", "Copied");
        await Assertions.Expect(firstCopy).ToHaveAttributeAsync("data-copy-state", "idle", new() { Timeout = 3000 });
        await page.EvaluateAsync("navigator.clipboard.writeText('before-repeat')");
        await firstCopy.ClickAsync();
        await page.WaitForFunctionAsync("expected => navigator.clipboard.readText().then(value => value === expected)", "ตรวจสอบไฟล์แล้ว 3 รายการ");

        await page.GetByTestId("message-reply").First.ClickAsync();
        var quote = page.GetByTestId("message-reply-quote");
        await Assertions.Expect(quote).ToContainTextAsync("ตรวจสอบไฟล์แล้ว 3 รายการ");
        await quote.GetByRole(AriaRole.Button, new() { Name = "Cancel reply" }).ClickAsync();
        await Assertions.Expect(quote).ToHaveCountAsync(0);

        var firstMessage = page.Locator("[data-slot='message']").First;
        var body = firstMessage.Locator("[data-slot='message-body']");
        var avatar = firstMessage.Locator("[data-slot='message-avatar']");
        var avatarImage = avatar.Locator("[data-slot='avatar-image']");
        var geometry = await firstMessage.EvaluateAsync<double[]>("element => { const body = element.querySelector('[data-slot=message-body]').getBoundingClientRect(); const avatar = element.querySelector('[data-slot=message-avatar]').getBoundingClientRect(); const image = element.querySelector('[data-slot=avatar-image]').getBoundingClientRect(); return [body.bottom, avatar.bottom, avatar.width, avatar.height, image.width, image.height]; }");
        Assert.InRange(Math.Abs(geometry[0] - geometry[1]), 0, 1);
        Assert.InRange(Math.Abs((geometry[2] - 2) - geometry[4]), 0, 1);
        Assert.InRange(Math.Abs((geometry[3] - 2) - geometry[5]), 0, 1);
        await Assertions.Expect(body).ToBeVisibleAsync();
        await Assertions.Expect(avatarImage).ToBeVisibleAsync();

        var outgoing = page.Locator("[data-slot='message'][data-align='end']").Last;
        var footerGeometry = await outgoing.EvaluateAsync<double[]>("element => { const actions = element.querySelector('[data-slot=message-actions]').getBoundingClientRect(); const status = element.querySelector('[data-slot=message-status]').getBoundingClientRect(); const button = element.querySelector('[data-slot=message-reply-action]').getBoundingClientRect(); return [actions.left, actions.right, status.left, button.width]; }");
        Assert.True(footerGeometry[0] < footerGeometry[1]);
        Assert.InRange(footerGeometry[2] - footerGeometry[1], 0, 8);
        Assert.InRange(footerGeometry[3], 24, 32);
    }

    [Fact]
    public async Task ConsecutiveMessagesFromOneSenderUseCompactGroupedSpacing()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message").ToString());

        var engineer = page.Locator("[data-slot='message'][data-sender='engineer']");
        await Assertions.Expect(engineer).ToHaveCountAsync(2);
        await Assertions.Expect(engineer.Nth(0).Locator("[data-testid='engineer-message-1']")).ToHaveCountAsync(1);
        await Assertions.Expect(engineer.Nth(1).Locator("[data-testid='engineer-message-2']")).ToHaveCountAsync(1);
        await Assertions.Expect(engineer.Nth(0).Locator("[data-slot='message-avatar']")).ToHaveCountAsync(1);
        await Assertions.Expect(engineer.Nth(0).Locator("[data-slot='message-header']")).ToHaveCountAsync(1);
        await Assertions.Expect(engineer.Nth(1)).ToHaveAttributeAsync("data-continuation", "true");
        await Assertions.Expect(engineer.Nth(1).Locator("[data-slot='message-avatar']")).ToHaveCountAsync(0);
        await Assertions.Expect(engineer.Nth(1).Locator("[data-slot='message-header']")).ToHaveCountAsync(0);
        await Assertions.Expect(engineer.Locator("[data-testid='message-reply']")).ToHaveCountAsync(2);

        await page.GetByTestId("control-message-footer-always").CheckAsync();
        await engineer.Nth(0).GetByTestId("message-reply").ClickAsync();
        await Assertions.Expect(page.GetByTestId("message-reply-quote")).ToContainTextAsync("ตรวจสอบไฟล์แล้ว 3 รายการ");
        await page.GetByTestId("message-reply-quote").GetByRole(AriaRole.Button, new() { Name = "Cancel reply" }).ClickAsync();
        await engineer.Nth(1).GetByTestId("message-reply").ClickAsync();
        await Assertions.Expect(page.GetByTestId("message-reply-quote")).ToContainTextAsync("กำลังตรวจสอบค่าความคลาดเคลื่อนต่อ");

        var spacing = await page.Locator(".showcase-message-thread").EvaluateAsync<double[]>("element => { const messages = element.querySelectorAll('[data-slot=message]'); const firstBubble = messages[0].querySelector('[data-slot=bubble]').getBoundingClientRect(); const secondBubble = messages[1].querySelector('[data-slot=bubble]').getBoundingClientRect(); const coordinatorBubble = messages[2].querySelector('[data-slot=bubble]').getBoundingClientRect(); const firstFooter = messages[0].querySelector('[data-slot=message-footer]').getBoundingClientRect(); const secondFooter = messages[1].querySelector('[data-slot=message-footer]').getBoundingClientRect(); return [secondBubble.top - firstBubble.bottom, coordinatorBubble.top - secondBubble.bottom, Math.abs(firstFooter.bottom - firstBubble.bottom), firstFooter.left - firstBubble.right, secondFooter.top - secondBubble.bottom, Math.abs(secondFooter.left - secondBubble.left), Math.abs(secondBubble.left - firstBubble.left)]; }");
        Assert.InRange(spacing[0], 3, 8);
        Assert.InRange(spacing[1], 68, 104);
        Assert.True(spacing[1] > spacing[0] * 4);
        Assert.InRange(spacing[2], 0, 1);
        Assert.InRange(spacing[3], 6, 16);
        Assert.InRange(spacing[4], 8, 12);
        Assert.InRange(spacing[5], 0, 1);
        Assert.InRange(spacing[6], 0, 1);
        Assert.False(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task BubblePreviewUsesDarkOutgoingGhostIncomingAndExpandableEmojiReactions()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/bubble").ToString());

        var outgoing = page.Locator("[data-bubble-role='outgoing']");
        await Assertions.Expect(outgoing).ToHaveCountAsync(2);
        await Assertions.Expect(outgoing.First).ToHaveAttributeAsync("data-variant", "default");
        var bubblePositionBeforeHover = await outgoing.First.BoundingBoxAsync();
        Assert.NotNull(bubblePositionBeforeHover);
        await outgoing.First.HoverAsync();
        var bubblePositionAfterHover = await outgoing.First.BoundingBoxAsync();
        Assert.NotNull(bubblePositionAfterHover);
        Assert.InRange(Math.Abs(bubblePositionAfterHover.Y - bubblePositionBeforeHover.Y), 0, 0.1);
        var defaultBubble = outgoing.First.Locator("[data-slot='bubble-content']");
        var defaultBackground = await defaultBubble.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");

        var incoming = page.Locator("[data-bubble-role='incoming']").First;
        await Assertions.Expect(incoming).ToHaveAttributeAsync("data-variant", "ghost");
        await Assertions.Expect(incoming).ToHaveAttributeAsync("data-align", "start");
        var incomingContent = incoming.Locator("[data-slot='bubble-content']");
        var ghostBackground = await incomingContent.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.NotEqual(defaultBackground, ghostBackground);

        await page.ChooseOptionAsync("control-bubble-variant", "Tinted");
        await Assertions.Expect(outgoing.First).ToHaveAttributeAsync("data-variant", "tinted");
        await Assertions.Expect(incoming).ToHaveAttributeAsync("data-variant", "ghost");
        await page.ChooseOptionAsync("control-bubble-received-variant", "Muted");
        await Assertions.Expect(incoming).ToHaveAttributeAsync("data-variant", "muted");
        await Assertions.Expect(outgoing.First).ToHaveAttributeAsync("data-variant", "tinted");
        var sentBackground = await defaultBubble.EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
        Assert.NotEqual(defaultBackground, sentBackground);

        var incomingTail = await incomingContent.EvaluateAsync<string>("element => getComputedStyle(element).borderEndStartRadius");
        var incomingTop = await incomingContent.EvaluateAsync<string>("element => getComputedStyle(element).borderStartStartRadius");
        Assert.True(ParseCssPixels(incomingTail) < ParseCssPixels(incomingTop));

        await page.GetByTestId("control-bubble-end").CheckAsync();
        await Assertions.Expect(incoming).ToHaveAttributeAsync("data-align", "end");
        var endRadii = await incomingContent.EvaluateAsync<string>("element => { const style = getComputedStyle(element); return `${style.borderBottomLeftRadius}|${style.borderBottomRightRadius}|${style.borderTopLeftRadius}|${style.borderTopRightRadius}`; }");
        var endRadiusValues = endRadii.Split('|', StringSplitOptions.TrimEntries);
        Assert.Equal(4, endRadiusValues.Length);
        var endTail = Math.Min(ParseCssPixels(endRadiusValues[0]), ParseCssPixels(endRadiusValues[1]));
        var endTop = Math.Max(ParseCssPixels(endRadiusValues[2]), ParseCssPixels(endRadiusValues[3]));
        Assert.True(endTail < endTop, $"end tail={endTail}px, top={endTop}px");

        await Assertions.Expect(page.Locator("[data-slot='bubble-reactions'] [data-slot='avatar']")).ToHaveCountAsync(0);
        var emojiReactions = page.Locator("[data-slot='bubble-reaction-value']");
        await Assertions.Expect(emojiReactions).ToHaveCountAsync(3);
        await Assertions.Expect(emojiReactions.First).ToHaveTextAsync("👍");

        var heartReaction = page.GetByTestId("bubble-reaction-heart");
        await Assertions.Expect(heartReaction).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(heartReaction.Locator("[data-slot='bubble-reaction-count']")).ToHaveTextAsync("2");
        Assert.Equal("pointer", await heartReaction.EvaluateAsync<string>("element => getComputedStyle(element).cursor"));
        await heartReaction.ClickAsync();
        await Assertions.Expect(heartReaction).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(heartReaction.Locator("[data-slot='bubble-reaction-count']")).ToHaveTextAsync("3");
        await heartReaction.ClickAsync();
        await Assertions.Expect(heartReaction).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(heartReaction.Locator("[data-slot='bubble-reaction-count']")).ToHaveTextAsync("2");
        await heartReaction.FocusAsync();
        await heartReaction.PressAsync("Space");
        await Assertions.Expect(heartReaction).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(heartReaction.Locator("[data-slot='bubble-reaction-count']")).ToHaveTextAsync("3");

        var overflow = page.Locator("[data-slot='bubble-reaction-overflow']");
        var overflowTrigger = overflow.Locator("[data-slot='bubble-reaction-overflow-trigger']");
        await Assertions.Expect(overflowTrigger).ToHaveTextAsync("+2");
        await Assertions.Expect(overflowTrigger).ToHaveAttributeAsync("aria-expanded", "false");
        await overflowTrigger.ClickAsync();
        await Assertions.Expect(overflowTrigger).ToHaveCountAsync(0);
        await Assertions.Expect(overflow.Locator("[data-slot='bubble-reaction-overflow-content'] [data-slot='bubble-reaction']")).ToHaveCountAsync(2);
        await Assertions.Expect(emojiReactions).ToHaveCountAsync(5);
        var fireReaction = page.GetByTestId("bubble-reaction-fire");
        await Assertions.Expect(fireReaction).ToHaveAttributeAsync("aria-pressed", "false");
        await Assertions.Expect(fireReaction.Locator("[data-slot='bubble-reaction-count']")).ToHaveTextAsync("1");
        await fireReaction.ClickAsync();
        await Assertions.Expect(fireReaction).ToHaveAttributeAsync("aria-pressed", "true");
        await Assertions.Expect(fireReaction.Locator("[data-slot='bubble-reaction-count']")).ToHaveTextAsync("2");
    }

    [Fact]
    public async Task CodeBlockUsesCanonicalEditorPaletteInLightAndDarkModes()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1280, Height = 900 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();

        // The canonical Bubble preview expresses its numeric count as a quoted Razor
        // attribute, so the source legitimately has no standalone number token.
        foreach (var (theme, expectedColors) in new[]
        {
            ("light", new Dictionary<string, string>
            {
                ["tag"] = "rgb(128, 0, 0)",
                ["string"] = "rgb(163, 21, 21)",
                ["type"] = "rgb(38, 127, 153)",
                ["directive"] = "rgb(175, 0, 219)"
            }),
            ("dark", new Dictionary<string, string>
            {
                ["tag"] = "rgb(86, 156, 214)",
                ["string"] = "rgb(206, 145, 120)",
                ["type"] = "rgb(78, 201, 176)",
                ["directive"] = "rgb(197, 134, 192)"
            })
        })
        {
            await page.GotoAsync(new Uri(server.BaseUri, $"/docs/components/bubble?theme={theme}").ToString());
            var preview = page.GetByTestId("component-preview").First;
            var sourceDisclosure = preview.Locator("details[data-testid='example-source']");
            await Assertions.Expect(sourceDisclosure).Not.ToHaveAttributeAsync("open", "");
            await sourceDisclosure.Locator("summary").ClickAsync();
            await Assertions.Expect(sourceDisclosure).ToHaveAttributeAsync("open", "");
            var code = sourceDisclosure.Locator("[data-slot='code-block']");
            await Assertions.Expect(code).ToBeVisibleAsync();
            if (theme == "dark")
            {
                await page.GetByTestId("documentation-theme-toggle").ClickAsync();
                await Assertions.Expect(page.Locator("[data-shadcn-scope]").First).ToHaveAttributeAsync("data-shadcn-theme", "dark");
            }
            var canonicalTokens = code.Locator("[class*='shadcn-code-token-']");
            await Assertions.Expect(canonicalTokens).Not.ToHaveCountAsync(0);

            var distinctColors = await canonicalTokens.EvaluateAllAsync<string[]>("elements => [...new Set(elements.map(element => getComputedStyle(element).color))]");
            Assert.True(distinctColors.Length >= 5, $"Expected a multi-token editor palette in {theme}; got {string.Join(", ", distinctColors)}");

            foreach (var (token, expectedColor) in expectedColors)
            {
                var tokenLocator = code.Locator($".shadcn-code-token-{token}").First;
                await Assertions.Expect(tokenLocator).ToBeVisibleAsync();
                Assert.Equal(expectedColor, await tokenLocator.EvaluateAsync<string>("element => getComputedStyle(element).color"));
            }

            var typeTokenText = await code.Locator(".shadcn-code-token-type").AllTextContentsAsync();
            Assert.DoesNotContain("Hey", typeTokenText);
            Assert.DoesNotContain("I", typeTokenText);
        }
    }

    [Fact]
    public async Task StreamingMarkerUsesLoaderAndShimmerButHonorsReducedMotion()
    {
        await using var animatedContext = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 640, Height = 700 }, ReducedMotion = ReducedMotion.NoPreference });
        var animatedPage = await animatedContext.NewPageAsync();
        await animatedPage.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        var animatedMarker = animatedPage.Locator("[data-slot='marker'][role='status']");
        var animatedLoader = animatedMarker.Locator(".showcase-marker-loader");
        await Assertions.Expect(animatedLoader).ToHaveCountAsync(1);
        var loaderBackground = await animatedLoader.EvaluateAsync<string>("element => getComputedStyle(element).backgroundImage");
        Assert.Contains("radial-gradient", loaderBackground, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("shadcn-marker-dots", await animatedLoader.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        Assert.Equal("none", await animatedMarker.Locator("[data-slot='marker-icon']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        var animatedContent = animatedMarker.Locator("[data-slot='marker-content']");
        Assert.Equal("shadcn-marker-wave", await animatedContent.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        var contentColor = await animatedContent.EvaluateAsync<string>("element => getComputedStyle(element).color");
        Assert.NotEqual("transparent", contentColor, StringComparer.OrdinalIgnoreCase);
        Assert.NotEqual("rgba(0, 0, 0, 0)", contentColor, StringComparer.OrdinalIgnoreCase);
        var textFillColor = await animatedContent.EvaluateAsync<string>("element => getComputedStyle(element).webkitTextFillColor");
        Assert.NotEqual("transparent", textFillColor, StringComparer.OrdinalIgnoreCase);
        var maskImage = await animatedContent.EvaluateAsync<string>("element => getComputedStyle(element).maskImage || getComputedStyle(element).webkitMaskImage");
        Assert.Contains("linear-gradient", maskImage, StringComparison.OrdinalIgnoreCase);
        var shimmerKeyframes = await animatedContent.EvaluateAsync<string>("element => [...document.styleSheets].flatMap(sheet => { try { return [...sheet.cssRules]; } catch { return []; } }).filter(rule => rule.name === 'shadcn-marker-wave').flatMap(rule => [...rule.cssRules]).map(rule => rule.cssText).join(' ')");
        Assert.Contains("mask-position: right center", shimmerKeyframes, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mask-position: left center", shimmerKeyframes, StringComparison.OrdinalIgnoreCase);

        await using var reducedContext = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 640, Height = 700 }, ReducedMotion = ReducedMotion.Reduce });
        var reducedPage = await reducedContext.NewPageAsync();
        await reducedPage.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        var reducedMarker = reducedPage.Locator("[data-slot='marker'][role='status']");
        Assert.Equal("none", await reducedMarker.Locator("[data-slot='marker-icon']").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        var reducedContent = reducedMarker.Locator("[data-slot='marker-content']");
        Assert.Equal("none", await reducedContent.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        var reducedContentColor = await reducedContent.EvaluateAsync<string>("element => getComputedStyle(element).color");
        Assert.NotEqual("transparent", reducedContentColor, StringComparer.OrdinalIgnoreCase);
        Assert.NotEqual("rgba(0, 0, 0, 0)", reducedContentColor, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("none", await reducedContent.EvaluateAsync<string>("element => getComputedStyle(element).maskImage || getComputedStyle(element).webkitMaskImage"));
        Assert.Equal("none", await reducedMarker.Locator(".showcase-marker-loader").EvaluateAsync<string>("element => getComputedStyle(element).animationName"));

        await using var forcedColorsContext = await playwright.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 640, Height = 700 }, ForcedColors = ForcedColors.Active });
        var forcedColorsPage = await forcedColorsContext.NewPageAsync();
        await forcedColorsPage.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());
        var forcedColorsMarker = forcedColorsPage.Locator("[data-slot='marker'][role='status']");
        var forcedColorsContent = forcedColorsMarker.Locator("[data-slot='marker-content']");
        var forcedColorsContentColor = await forcedColorsContent.EvaluateAsync<string>("element => getComputedStyle(element).color");
        Assert.NotEqual("transparent", forcedColorsContentColor, StringComparer.OrdinalIgnoreCase);
        Assert.NotEqual("rgba(0, 0, 0, 0)", forcedColorsContentColor, StringComparer.OrdinalIgnoreCase);
        Assert.Equal("none", await forcedColorsContent.EvaluateAsync<string>("element => getComputedStyle(element).animationName"));
        var forcedColorsLoaderColor = await forcedColorsMarker.Locator(".showcase-marker-loader").EvaluateAsync<string>("element => getComputedStyle(element).color");
        Assert.NotEqual("transparent", forcedColorsLoaderColor, StringComparer.OrdinalIgnoreCase);
        Assert.NotEqual("rgba(0, 0, 0, 0)", forcedColorsLoaderColor, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarkerConversationLeavesReadableSpaceBetweenMessageBubbles()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 640, Height = 700 },
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/marker").ToString());

        var bubbles = page.Locator(".showcase-marker-thread [data-slot='bubble']");
        await Assertions.Expect(bubbles).ToHaveCountAsync(3);
        var first = await bubbles.Nth(0).BoundingBoxAsync();
        var second = await bubbles.Nth(1).BoundingBoxAsync();
        var third = await bubbles.Nth(2).BoundingBoxAsync();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotNull(third);
        Assert.True(second!.Y - (first!.Y + first.Height) >= 12);
        Assert.True(third!.Y - (second.Y + second.Height) >= 12);
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
    public async Task ScrollerFollowsStreamingUntilMeasuredDepartureAndResumesAtTheNativeEdge()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ColorScheme = ColorScheme.Dark
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/message-scroller").ToString());
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var scroller = page.Locator("#preview [data-slot='message-scroller']");
        var viewport = scroller.Locator("[data-slot='message-scroller-viewport']");
        await Assertions.Expect(page.GetByTestId("control-scroller-auto")).ToBeCheckedAsync();
        await Assertions.Expect(page.GetByTestId("scroller-demo")).ToHaveAttributeAsync("data-preview-auto", "true");
        await page.GetByTestId("scroller-send").ClickAsync();
        await Assertions.Expect(page.GetByTestId("scroller-streaming")).ToBeVisibleAsync();
        await page.WaitForFunctionAsync("element => element.scrollHeight - element.clientHeight > 80", await viewport.ElementHandleAsync());
        await page.WaitForFunctionAsync("element => element.scrollHeight - element.clientHeight - element.scrollTop <= 8", await viewport.ElementHandleAsync());

        var last = scroller.Locator("[data-slot='message-scroller-item']").Last;
        var assistant = last.Locator("[data-slot='message']");
        var aligned = await assistant.EvaluateAsync<double[]>("element => { const body = element.querySelector('[data-slot=message-body]').getBoundingClientRect(); const avatar = element.querySelector('[data-slot=message-avatar]').getBoundingClientRect(); return [body.bottom, avatar.bottom]; }");
        Assert.InRange(Math.Abs(aligned[0] - aligned[1]), 0, 1);

        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-autoscrolling", "");
        await viewport.EvaluateAsync("element => { element.scrollTop = 0; element.dispatchEvent(new PointerEvent('pointerup', { bubbles: true })); }");
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-unread", "true");
        await page.WaitForTimeoutAsync(180);
        Assert.InRange(await viewport.EvaluateAsync<double>("element => element.scrollTop"), 0, 8);

        await viewport.EvaluateAsync("element => { element.scrollTop = element.scrollHeight; element.dispatchEvent(new PointerEvent('pointerup', { bubbles: true })); }");
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-scrollable-end", "false");
        await Assertions.Expect(scroller).ToHaveAttributeAsync("data-following", "true");
        await Assertions.Expect(page.GetByTestId("scroller-streaming")).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await page.WaitForFunctionAsync("element => element.scrollHeight - element.clientHeight - element.scrollTop <= 8", await viewport.ElementHandleAsync());

        var safeGeometry = await scroller.EvaluateAsync<double[]>("element => { const last = element.querySelector('[data-slot=message-scroller-item]:last-child').getBoundingClientRect(); const transcript = element.querySelector('.showcase-scroller-transcript'); const transcriptBox = transcript.getBoundingClientRect(); const viewport = transcript.querySelector('[data-slot=message-scroller-viewport]').getBoundingClientRect(); const composer = element.querySelector('.showcase-scroller-composer').getBoundingClientRect(); const fade = getComputedStyle(transcript, '::after'); return [last.bottom, composer.top, transcriptBox.bottom, viewport.bottom, fade.pointerEvents === 'none' ? 1 : 0, fade.backgroundImage.includes('gradient') ? 1 : 0, parseFloat(fade.width), transcriptBox.width]; }");
        Assert.True(safeGeometry[0] < safeGeometry[1]);
        Assert.InRange(Math.Abs(safeGeometry[2] - safeGeometry[3]), 0, 1);
        Assert.True(safeGeometry[2] <= safeGeometry[1], "The transcript must end above the composer.");
        Assert.Equal(1, safeGeometry[4]);
        Assert.Equal(1, safeGeometry[5]);
        Assert.True(safeGeometry[6] <= safeGeometry[7] - 12, "The message fade must stop before the vertical scrollbar gutter.");
    }

    [Fact]
    public async Task QuestionnaireDossierSupportsBilingualKeyboardCustomAnswersAndExactSource()
    {
        await using var context = await playwright.Browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 390, Height = 844 },
            ReducedMotion = ReducedMotion.Reduce,
            ForcedColors = ForcedColors.Active
        });
        var page = await context.NewPageAsync();
        await page.GotoAsync(new Uri(server.BaseUri, "/docs/components/questionnaire").ToString());
        await page.GetByTestId("documentation-direction-toggle").ClickAsync();

        var other = page.Locator("#preview input[value='other']");
        await other.FocusAsync();
        await page.Keyboard.PressAsync("3");
        await Assertions.Expect(other).ToBeCheckedAsync();
        await Assertions.Expect(other).ToHaveAttributeAsync("aria-expanded", "true");
        var custom = page.Locator("#preview [data-slot='questionnaire-input'][data-custom='true']");
        await Assertions.Expect(custom).ToBeVisibleAsync();
        Assert.Equal(await custom.GetAttributeAsync("id"), await other.GetAttributeAsync("aria-controls"));

        await page.Locator("#preview [data-slot='questionnaire-next']").ClickAsync();
        await Assertions.Expect(page.Locator("#preview [data-slot='questionnaire-error']:visible")).ToContainTextAsync("A custom answer is required.");
        await custom.FillAsync("ชิ้นงานเฉพาะ · Custom part");
        await page.Locator("#preview [data-slot='questionnaire-next']").ClickAsync();
        await Assertions.Expect(page.Locator("#preview fieldset[name='notes']")).ToBeVisibleAsync();

        var source = page.Locator("#preview .component-code");
        await Assertions.Expect(source).ToContainTextAsync("Custom: true");
        await Assertions.Expect(source).ToContainTextAsync("อื่น ๆ · Other");
        await Assertions.Expect(source).ToContainTextAsync("ShadcnQuestionnaireInput");
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

    private static double ParseCssPixels(string value) =>
        double.Parse(value.Replace("px", string.Empty, StringComparison.OrdinalIgnoreCase), CultureInfo.InvariantCulture);
}
