using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class DrawerShowcaseTests : BunitContext
{
    public DrawerShowcaseTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void DrawerDossierUsesDirectInteractionAndACompleteResponsiveComposition()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("drawer").Single();
        var rendered = Render(example.Preview);

        Assert.Contains("showcase-drawer-dossier", rendered.Markup, StringComparison.Ordinal);
        Assert.Contains("Dispatch review", rendered.Markup, StringComparison.Ordinal);
        Assert.Contains("Bangkok production hub", rendered.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain(example.Controls, control => control.Id == "drawer-open");
        Assert.Contains(example.Controls, control => control.Id == "drawer-direction" && control.Kind == ComponentParameterControlKind.Select);
        Assert.Contains(example.Controls, control => control.Id == "drawer-modal-mode" && control.Kind == ComponentParameterControlKind.Select);

        var trigger = rendered.Find("[data-slot='drawer-trigger']");
        Assert.Equal("false", trigger.GetAttribute("aria-expanded"));
        trigger.Click();

        Assert.Equal("true", rendered.Find("[data-slot='drawer-trigger']").GetAttribute("aria-expanded"));
        Assert.Contains("Confirm dispatch", rendered.Find("[data-slot='drawer-content']").TextContent, StringComparison.Ordinal);
        Assert.Equal(2, rendered.FindAll("[data-slot='drawer-close']").Count);
    }

    [Fact]
    public void DrawerSourceTracksDirectionModalModeHandleAndSnapSettingsExactly()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("drawer").Single();

        Assert.Contains("@bind-Open=\"Open\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("SwipeDirection=\"ShadcnDrawerSwipeDirection.Down\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ModalMode=\"ShadcnDrawerModalMode.Modal\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowSwipeHandle=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("SnapPoints=\"SnapPoints\"", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "drawer-direction").Apply("Right");
        example.Controls.Single(control => control.Id == "drawer-modal-mode").Apply("TrapFocus");
        example.Controls.Single(control => control.Id == "drawer-handle").Apply("false");
        example.Controls.Single(control => control.Id == "drawer-snap-points").Apply("false");

        Assert.Contains("SwipeDirection=\"ShadcnDrawerSwipeDirection.Right\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ModalMode=\"ShadcnDrawerModalMode.TrapFocus\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowSwipeHandle=\"false\"", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SnapPoints=\"SnapPoints\"", example.RazorSource, StringComparison.Ordinal);

        var rendered = Render(example.Preview);
        Assert.Equal("right", rendered.Find("[data-slot='drawer']").GetAttribute("data-edge"));
        rendered.Find("[data-slot='drawer-trigger']").Click();
        Assert.Empty(rendered.FindAll("[data-slot='drawer-overlay']"));
    }
}
