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
            Assert.Equal("Copied", copy.QuerySelector(".shadcn-code-block-copy-status")?.TextContent.Trim());
            Assert.Equal("Source copied to clipboard.", cut.Find("[aria-live='polite']").TextContent.Trim());
        });
    }
}
