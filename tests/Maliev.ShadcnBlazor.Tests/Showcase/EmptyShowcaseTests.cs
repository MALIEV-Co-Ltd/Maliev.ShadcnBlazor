using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class EmptyShowcaseTests : BunitContext
{
    public EmptyShowcaseTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void EmptyDossierUsesPackageButtonsAndAnnouncesBothRecoveryActions()
    {
        var example = GetExample();
        var cut = Render(example.Preview);

        Assert.NotNull(cut.Find(".showcase-empty-dossier [data-slot='empty']"));
        Assert.Equal(2, cut.FindAll(".showcase-empty-actions [data-slot='button']").Count);
        Assert.Equal("default", cut.Find("[data-empty-action='create']").GetAttribute("data-variant"));
        Assert.Equal("outline", cut.Find("[data-empty-action='import']").GetAttribute("data-variant"));

        var status = cut.Find("[role='status'][aria-live='polite']");
        Assert.Equal("Choose how you want to start.", status.TextContent.Trim());

        cut.Find("[data-empty-action='create']").Click();
        cut.WaitForAssertion(() => Assert.Equal("A new project workspace is ready.", status.TextContent.Trim()));

        cut.Find("[data-empty-action='import']").Click();
        cut.WaitForAssertion(() => Assert.Equal("Project import opened. Select a project archive to continue.", status.TextContent.Trim()));
    }

    [Fact]
    public void EmptyDossierSourceTracksMediaVariantAndContainsCompleteInteractionCode()
    {
        var example = GetExample();

        Assert.Contains("@using Maliev.ShadcnBlazor.Components.Actions", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnButton", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("StartProject", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ImportProject", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("role=\"status\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnEmptyMediaVariant.Icon\"", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "empty-media-variant").Apply("Default");

        Assert.Contains("Variant=\"ShadcnEmptyMediaVariant.Default\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Equal("default", Render(example.Preview).Find("[data-slot='empty-icon']").GetAttribute("data-variant"));
    }

    private static ComponentExampleDefinition GetExample()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        return Assert.Single(registry.GetBySlug("empty"));
    }
}
