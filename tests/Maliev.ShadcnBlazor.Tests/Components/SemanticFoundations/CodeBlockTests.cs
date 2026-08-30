using Bunit;
using Maliev.ShadcnBlazor.Components.Typography;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Components.SemanticFoundations;

public sealed class CodeBlockTests : BunitContext
{
    public CodeBlockTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void RendersTheSelectedLanguageAndSyntaxTokens()
    {
        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, "<ShadcnButton Variant=\"Outline\">Save</ShadcnButton>")
            .Add(component => component.Language, "razor")
            .Add(component => component.Sources, new Dictionary<string, string>
            {
                ["razor"] = "<ShadcnButton>Save</ShadcnButton>",
                ["csharp"] = "var enabled = true;"
            }));

        Assert.Equal("razor", cut.Find("[data-slot='code-block']").GetAttribute("data-language"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-tag"));
        var trigger = cut.Find("button[data-slot='select-trigger']");
        Assert.Equal("razor", cut.Find("[data-slot='select-value']").TextContent);
        trigger.Click();
        Assert.Equal(2, cut.FindAll("[role='option']").Count);

        cut.Find("[role='option'][data-value='csharp']").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("csharp", cut.Find("[data-slot='code-block']").GetAttribute("data-language"));
            Assert.NotEmpty(cut.FindAll(".shadcn-code-token-keyword"));
        });
    }

    [Fact]
    public void CopyActionUsesTheRclModuleAndAnnouncesSuccess()
    {
        const string source = "<ShadcnKbd>Ctrl</ShadcnKbd>";
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-code-block.js");
        module.SetupVoid("copyText", source).SetVoidResult();
        var cut = Render<ShadcnCodeBlock>(parameters => parameters.Add(component => component.Source, source));

        cut.Find("[data-testid='copy-source']").Click();

        cut.WaitForAssertion(() =>
        {
            var copy = cut.Find("[data-testid='copy-source']");
            Assert.Equal("true", copy.GetAttribute("data-copied"));
            Assert.Equal("Copied", copy.GetAttribute("aria-label"));
            Assert.Null(copy.QuerySelector(".shadcn-code-block-copy-status"));
            Assert.NotNull(copy.QuerySelector("svg"));
            Assert.Equal("Source copied to clipboard.", cut.Find("[aria-live='polite']").TextContent.Trim());
        });
    }

    [Fact]
    public void ToolbarUsesAStaticLanguageLabelWhenThereIsOnlyOneSource()
    {
        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, "dotnet add package Maliev.ShadcnBlazor")
            .Add(component => component.Language, "bash"));

        var toolbar = cut.Find("[data-slot='code-block-toolbar']");
        var label = toolbar.QuerySelector(".shadcn-code-block-language");
        Assert.NotNull(label);
        Assert.Equal("copy-source", label.NextElementSibling?.GetAttribute("data-testid"));
        Assert.Equal("bash", label.TextContent.Trim());
        Assert.Null(toolbar.QuerySelector("[data-slot='select-trigger']"));
    }

    [Fact]
    public void CopyModuleFallsBackWhenClipboardPermissionIsUnavailable()
    {
        var root = FindRoot();
        var module = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "js", "shadcn-code-block.js"));

        Assert.Contains("navigator.clipboard?.writeText", module, StringComparison.Ordinal);
        Assert.Contains("document.execCommand(\"copy\")", module, StringComparison.Ordinal);
        Assert.Contains("activeElement.focus({ preventScroll: true })", module, StringComparison.Ordinal);
    }

    [Fact]
    public void HighlightsCSharpAndRazorSyntaxWithEditorTokens()
    {
        const string source = "@using Maliev.ShadcnBlazor.Components.Feedback\n<ShadcnAttachment State=\"ShadcnAttachmentState.Done\" />\nvar progress = 64;\nvar ready = true;";

        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, source)
            .Add(component => component.Language, "razor"));

        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-directive"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-tag"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-attribute"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-string"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-type"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-literal"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-number"));

        var csharp = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, "public async Task<string> ResolveAsync() { var result = true; return result; }")
            .Add(component => component.Language, "csharp"));

        Assert.Empty(csharp.FindAll(".shadcn-code-token-tag"));
        Assert.NotEmpty(csharp.FindAll(".shadcn-code-token-keyword"));
        Assert.NotEmpty(csharp.FindAll(".shadcn-code-token-type"));
        Assert.NotEmpty(csharp.FindAll(".shadcn-code-token-method"));
        Assert.NotEmpty(csharp.FindAll(".shadcn-code-token-literal"));
    }

    [Fact]
    public void KeepsApostrophesInRazorTextOutOfFollowingMarkup()
    {
        const string source = "<ShadcnBubbleContent>Hey there! what's up?</ShadcnBubbleContent>\n<ShadcnBubbleContent>Still markup.</ShadcnBubbleContent>";

        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, source)
            .Add(component => component.Language, "razor"));

        var strings = cut.FindAll(".shadcn-code-token-string").Select(element => element.TextContent).ToArray();
        Assert.DoesNotContain(strings, value => value.Contains("what's", StringComparison.Ordinal));
        Assert.Equal(4, cut.FindAll(".shadcn-code-token-tag").Count);

        var razorCode = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, "@code { var label = \"value\"; }")
            .Add(component => component.Language, "razor"));

        Assert.Contains(razorCode.FindAll(".shadcn-code-token-string"), element => element.TextContent == "\"value\"");
    }

    [Fact]
    public void LeavesRazorMarkupTextInTheEditorForegroundPalette()
    {
        const string source = "<ShadcnBubbleContent>Hey there! I can ship it.</ShadcnBubbleContent>";

        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, source)
            .Add(component => component.Language, "razor"));

        var typeTokens = cut.FindAll(".shadcn-code-token-type").Select(element => element.TextContent).ToArray();
        Assert.DoesNotContain("Hey", typeTokens);
        Assert.DoesNotContain("I", typeTokens);
        Assert.Contains(cut.FindAll(".shadcn-code-token-tag"), element => element.TextContent == "ShadcnBubbleContent");
    }

    [Fact]
    public void KeepsTheEditorPaletteAcrossACompleteRazorExample()
    {
        const string source = """
            @using Maliev.ShadcnBlazor.Components.Conversation

            <ShadcnBubbleGroup data-reveal="true">
                <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
                    <ShadcnBubbleContent>Hey there! what's up?</ShadcnBubbleContent>
                </ShadcnBubble>

                <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Tinted">
                    <ShadcnBubbleContent>Hey! Want to see chat bubbles?</ShadcnBubbleContent>
                    <ShadcnBubbleReactions Side="ShadcnReactionSide.Bottom" Align="ShadcnLogicalAlign.Start" AccessibleName="Reactions">
                        <span aria-hidden="true">👍</span>
                    </ShadcnBubbleReactions>
                </ShadcnBubble>

                <ShadcnBubble Align="ShadcnLogicalAlign.Start" Variant="ShadcnBubbleVariant.Muted">
                    <ShadcnBubbleContent>I can group messages, switch sides, and keep the whole thread easy to scan.</ShadcnBubbleContent>
                </ShadcnBubble>

                <ShadcnBubble Align="ShadcnLogicalAlign.End" Variant="ShadcnBubbleVariant.Default">
                    <ShadcnBubbleContent Href="/docs/components/bubble">Sure. Hit me with your best demo.</ShadcnBubbleContent>
                </ShadcnBubble>
            </ShadcnBubbleGroup>
            """;

        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, source)
            .Add(component => component.Language, "razor"));

        var tags = cut.FindAll(".shadcn-code-token-tag").Select(element => element.TextContent).ToArray();
        Assert.Contains("ShadcnBubbleGroup", tags);
        Assert.Contains("ShadcnBubbleReactions", tags);
        Assert.Contains("ShadcnBubbleContent", tags);
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-directive"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-attribute"));
        Assert.NotEmpty(cut.FindAll(".shadcn-code-token-string"));
        Assert.DoesNotContain("Hey", cut.FindAll(".shadcn-code-token-type").Select(element => element.TextContent));
        Assert.DoesNotContain("I", cut.FindAll(".shadcn-code-token-type").Select(element => element.TextContent));
    }

    [Fact]
    public void HighlightsRazorExpressionsInsideMarkupAttributesWithoutLosingThaiContent()
    {
        const string source = "<ShadcnButton Disabled=\"@isBusy\" aria-label=\"บันทึก @item.Name\">บันทึกงาน</ShadcnButton>";

        var cut = Render<ShadcnCodeBlock>(parameters => parameters
            .Add(component => component.Source, source)
            .Add(component => component.Language, "razor"));

        Assert.Contains(cut.FindAll(".shadcn-code-token-directive"), token => token.TextContent == "@");
        Assert.Contains(cut.FindAll(".shadcn-code-token-property"), token => token.TextContent == "Name");
        Assert.Contains("บันทึกงาน", cut.Find("code").TextContent, StringComparison.Ordinal);
        Assert.Contains("บันทึก ", cut.Find("code").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void UsesOneEditorPaletteAcrossPackageAndShowcaseStyles()
    {
        var root = FindRoot();
        var baseCss = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-base.css"));
        var showcaseCss = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);
        var darkStart = baseCss.IndexOf("\n[data-shadcn-theme=\"dark\"],", StringComparison.Ordinal);
        if (darkStart >= 0) darkStart++;

        Assert.True(darkStart > 0, "The dark theme token scope is required.");
        foreach (var token in new[] { "foreground", "comment", "tag", "string", "keyword", "type", "number", "literal", "attribute", "method", "property", "directive", "operator", "punctuation" })
        {
            var declaration = $"--shadcn-code-token-{token}:";
            Assert.Contains(declaration, baseCss[..darkStart], StringComparison.Ordinal);
            Assert.Contains(declaration, baseCss[darkStart..], StringComparison.Ordinal);
            Assert.DoesNotContain($"--shadcn-code-token-{token}:", showcaseCss, StringComparison.Ordinal);
        }

        // The RCL emits and styles the canonical token classes. The Showcase
        // must not redefine package-owned editor internals.
        foreach (var token in new[] { "comment", "tag", "string", "keyword", "type", "number", "literal", "attribute", "method", "property", "directive", "operator", "punctuation" })
        {
            Assert.DoesNotContain($".component-code pre code .shadcn-code-token-{token}", showcaseCss, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("#8250df", showcaseCss, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#cf222e", showcaseCss, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#0a6e3d", showcaseCss, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UsesQuietCopyControlAndTransientCopiedFeedbackStyles()
    {
        var root = FindRoot();
        var baseCss = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-base.css"));
        var showcaseCss = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"))
            .Replace("\r\n", "\n", StringComparison.Ordinal);

        Assert.Contains(".shadcn-code-block-copy {", baseCss, StringComparison.Ordinal);
        Assert.Contains(".shadcn-code-block-copy { display: inline-flex", baseCss, StringComparison.Ordinal);
        Assert.Contains("opacity: 1", baseCss, StringComparison.Ordinal);
        Assert.Contains(".shadcn-code-block-copy[data-copied=\"true\"]", baseCss, StringComparison.Ordinal);
        Assert.Contains("@keyframes shadcn-code-block-copy-feedback", baseCss, StringComparison.Ordinal);
        Assert.DoesNotContain("shadcn-code-block-copy-fade", baseCss, StringComparison.Ordinal);

        Assert.Contains("margin-inline-start: auto", baseCss, StringComparison.Ordinal);
        Assert.Contains("inline-size: 2rem", baseCss, StringComparison.Ordinal);
        Assert.Contains("min-inline-size: 0", baseCss, StringComparison.Ordinal);
        Assert.Contains("direction: ltr", baseCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".component-code__surface .shadcn-code-block-copy {\n    position: absolute", showcaseCss, StringComparison.Ordinal);
        Assert.DoesNotContain("@keyframes component-code-copied-feedback", showcaseCss, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowcaseStylesheetsCarryARevisionQueryForPaletteRefreshes()
    {
        var root = FindRoot();
        var index = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html"));

        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-base.css?v=2.0.0", index, StringComparison.Ordinal);
        Assert.Contains("css/showcase.css?v=2.0.0", index, StringComparison.Ordinal);
        Assert.Contains("js/mock-site-overlay.js?v=2.0.0", index, StringComparison.Ordinal);
        Assert.Contains("_framework/blazor.webassembly.js?v=2.0.0", index, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CopyFeedbackReturnsToTheCopyIconForSubsequentCopies()
    {
        const string source = "<ShadcnKbd>Ctrl</ShadcnKbd>";
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-code-block.js");
        module.SetupVoid("copyText", source).SetVoidResult();
        var cut = Render<ShadcnCodeBlock>(parameters => parameters.Add(component => component.Source, source));
        var copy = cut.Find("[data-testid='copy-source']");

        Assert.Equal("idle", copy.GetAttribute("data-copy-state"));
        Assert.Null(copy.QuerySelector(".shadcn-code-block-copy-feedback"));
        copy.Click();
        cut.WaitForAssertion(() =>
        {
            var copied = cut.Find("[data-testid='copy-source']");
            Assert.Equal("true", copied.GetAttribute("data-copied"));
            Assert.Equal("copied", copied.GetAttribute("data-copy-state"));
            Assert.NotNull(copied.QuerySelector(".shadcn-code-block-copy-feedback"));
        });

        await Task.Delay(2100);
        cut.WaitForAssertion(() =>
        {
            var reset = cut.Find("[data-testid='copy-source']");
            Assert.Equal("false", reset.GetAttribute("data-copied"));
            Assert.Equal("idle", reset.GetAttribute("data-copy-state"));
            Assert.Null(reset.QuerySelector(".shadcn-code-block-copy-status"));
            Assert.NotNull(reset.QuerySelector("svg rect"));
        });

        cut.Find("[data-testid='copy-source']").Click();
        cut.WaitForAssertion(() =>
        {
            var copied = cut.Find("[data-testid='copy-source']");
            Assert.Equal("true", copied.GetAttribute("data-copied"));
            Assert.Equal("copied", copied.GetAttribute("data-copy-state"));
        });
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
