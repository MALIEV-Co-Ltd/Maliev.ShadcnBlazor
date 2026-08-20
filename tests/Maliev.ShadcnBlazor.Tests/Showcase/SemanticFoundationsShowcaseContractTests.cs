using Bunit;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class SemanticFoundationsShowcaseContractTests : BunitContext
{
    [Fact]
    public void SemanticExamplesShowTheFullHierarchyAndHumanReadableRatios()
    {
        var registry = new Maliev.ShadcnBlazor.Showcase.Documentation.Examples.ComponentExampleRegistry(new Maliev.ShadcnBlazor.Showcase.Documentation.ComponentDocumentationCatalog());
        var aspectRatio = Assert.Single(registry.GetBySlug("aspect-ratio"));
        var ratioControl = Assert.Single(aspectRatio.Controls, control => control.Id == "aspect-ratio");
        Assert.Equal(["16:9", "4:3", "1:1"], ratioControl.Options);
        Assert.Contains("16:9", aspectRatio.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("1.7777777777777777", aspectRatio.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<img src=\"images/attachments/workspace-plan.png\"", aspectRatio.RazorSource, StringComparison.Ordinal);

        ratioControl.Apply("4:3");
        Assert.Contains("Ratio=\"@(4d / 3d)\"", aspectRatio.RazorSource, StringComparison.Ordinal);
        Assert.Contains("showcase-aspect-ratio-demo--4-3", aspectRatio.RazorSource, StringComparison.Ordinal);

        ratioControl.Apply("1:1");
        Assert.Contains("Ratio=\"@(1d / 1d)\"", aspectRatio.RazorSource, StringComparison.Ordinal);
        Assert.Contains("showcase-aspect-ratio-demo--1-1", aspectRatio.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<span class=\"showcase-aspect-ratio-media__ratio\">1:1</span>", aspectRatio.RazorSource, StringComparison.Ordinal);

        var typography = Assert.Single(registry.GetBySlug("typography"));
        Assert.Contains("Variant=\"ShadcnTypographyVariant.H1\"", typography.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnTypographyVariant.H3\"", typography.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnTypographyVariant.Paragraph\"", typography.RazorSource, StringComparison.Ordinal);

        var keyboard = Assert.Single(registry.GetBySlug("kbd"));
        Assert.Contains("ShadcnCard", keyboard.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Esc", keyboard.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Ctrl", keyboard.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Shift", keyboard.RazorSource, StringComparison.Ordinal);

        var item = Assert.Single(registry.GetBySlug("item"));
        Assert.Contains("<ShadcnItemGroup", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnItemActions>", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnBadge", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<svg aria-hidden=\"true\"", item.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain(">PDF<", item.RazorSource, StringComparison.OrdinalIgnoreCase);

        item.Controls.Single(control => control.Id == "item-variant").Apply("Muted");
        item.Controls.Single(control => control.Id == "item-size").Apply("Small");
        item.Controls.Single(control => control.Id == "item-media-variant").Apply("Image");
        item.Controls.Single(control => control.Id == "item-link").Apply("true");

        Assert.Contains("Variant=\"ShadcnItemVariant.Muted\"", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Size=\"ShadcnItemSize.Small\"", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Href=\"#item-workspace-plan\"", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnItemMediaVariant.Image\"", item.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<img src=\"images/attachments/workspace-plan.png\"", item.RazorSource, StringComparison.Ordinal);
        var platform = Assert.Single(keyboard.Controls, control => control.Id == "kbd-platform");
        platform.Apply("macOS");
        Assert.Contains("<ShadcnKbd>⌘</ShadcnKbd>", keyboard.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<ShadcnKbd>Ctrl</ShadcnKbd>", keyboard.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnKbd>Shift</ShadcnKbd>", keyboard.RazorSource, StringComparison.Ordinal);
        Assert.Equal("macOS", platform.Value);
    }

    [Fact]
    public void KbdStylesPreserveLiteralShortcutOrderAndForcedColorDefinition()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(
            root,
            "src",
            "Maliev.ShadcnBlazor",
            "wwwroot",
            "css",
            "shadcn-semantic-foundations.css"));

        Assert.Contains(".shadcn-kbd-group", css, StringComparison.Ordinal);
        Assert.Contains("direction: ltr", css, StringComparison.Ordinal);
        Assert.Contains("unicode-bidi: isolate", css, StringComparison.Ordinal);
        Assert.Contains("@media (forced-colors: active)", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-kbd {", css, StringComparison.Ordinal);
        Assert.Contains("border: 1px solid ButtonText", css, StringComparison.Ordinal);
    }

    [Fact]
    public void LabelExampleUsesPackageComponentsAndMatchesTheDisabledSetting()
    {
        var registry = new Maliev.ShadcnBlazor.Showcase.Documentation.Examples.ComponentExampleRegistry(new Maliev.ShadcnBlazor.Showcase.Documentation.ComponentDocumentationCatalog());
        var label = Assert.Single(registry.GetBySlug("label"));

        Assert.Contains("<ShadcnLabel For=\"project-name\">", label.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnInput TValue=\"string\"", label.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind-Value=\"ProjectName\"", label.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<input", label.RazorSource, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Disabled=\"false\"", label.RazorSource, StringComparison.Ordinal);

        label.Controls.Single(control => control.Id == "label-disabled").Apply("true");

        Assert.Contains("Disabled=\"true\"", label.RazorSource, StringComparison.Ordinal);
        Assert.Contains("data-disabled=\"true\"", label.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void FieldExampleUsesLibraryControlsAndKeepsSourceInSyncWithSettings()
    {
        var registry = new Maliev.ShadcnBlazor.Showcase.Documentation.Examples.ComponentExampleRegistry(new Maliev.ShadcnBlazor.Showcase.Documentation.ComponentDocumentationCatalog());
        var field = Assert.Single(registry.GetBySlug("field"));

        Assert.Contains("<ShadcnInput", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSelect", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnCheckbox", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnTextarea", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnButton", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@code", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("HandleSubmit", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Orientation=\"ShadcnFieldOrientation.Vertical\"", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnFieldLegendVariant.Legend\"", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Invalid=\"false\"", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"false\"", field.RazorSource, StringComparison.Ordinal);

        field.Controls.Single(control => control.Id == "field-orientation").Apply("Horizontal");
        field.Controls.Single(control => control.Id == "field-legend-variant").Apply("Label");
        field.Controls.Single(control => control.Id == "field-invalid").Apply("true");
        field.Controls.Single(control => control.Id == "field-disabled").Apply("true");

        Assert.Contains("Orientation=\"ShadcnFieldOrientation.Horizontal\"", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Variant=\"ShadcnFieldLegendVariant.Label\"", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Invalid=\"true\"", field.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Disabled=\"true\"", field.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowcaseLoadsSemanticStylesAndExposesComponentRoute()
    {
        var root = FindRoot();
        var index = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html"));
        var layout = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "MainLayout.razor"));
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "SemanticFoundations.razor"));
        var documentation = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));

        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css", index, StringComparison.Ordinal);
        Assert.Contains("components/semantic-foundations", layout, StringComparison.Ordinal);
        Assert.Contains("@page \"/components/semantic-foundations\"", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAspectRatio", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnField", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnItem", page, StringComparison.Ordinal);
        Assert.DoesNotContain(">M</span></ShadcnItemMedia>", page, StringComparison.Ordinal);
        Assert.Contains("<svg aria-hidden=\"true\" viewBox=\"0 0 24 24\"", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnEmpty", page, StringComparison.Ordinal);
        Assert.Contains("\"label\" => [\"apps/v4/registry/bases/base/ui/label.tsx\"]", documentation, StringComparison.Ordinal);
    }

    [Fact]
    public void ItemStylesKeepMediaDecorativeAndVisibleInForcedColors()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-semantic-foundations.css"));

        Assert.Contains(".shadcn-item-media svg", css, StringComparison.Ordinal);
        Assert.Contains("pointer-events: none", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-item[data-variant=\"outline\"]", css, StringComparison.Ordinal);
        Assert.Contains("border-color: CanvasText", css, StringComparison.Ordinal);
    }

    [Fact]
    public void SeparatorPreviewAndSourceStaySynchronizedAcrossMeaningfulStates()
    {
        var registry = new Maliev.ShadcnBlazor.Showcase.Documentation.Examples.ComponentExampleRegistry(new Maliev.ShadcnBlazor.Showcase.Documentation.ComponentDocumentationCatalog());
        var separator = Assert.Single(registry.GetBySlug("separator"));

        Assert.Contains("Orientation=\"ShadcnSeparatorOrientation.Horizontal\"", separator.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Decorative=\"false\"", separator.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Quotation #Q-4189", separator.RazorSource, StringComparison.Ordinal);
        var initial = Render(separator.Preview);
        Assert.Equal("horizontal", initial.Find("[data-slot='separator']").GetAttribute("data-orientation"));
        Assert.Equal("separator", initial.Find("[data-slot='separator']").GetAttribute("role"));

        separator.Controls.Single(control => control.Id == "separator-orientation").Apply("Vertical");
        separator.Controls.Single(control => control.Id == "separator-decorative").Apply("true");

        Assert.Contains("Orientation=\"ShadcnSeparatorOrientation.Vertical\"", separator.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Decorative=\"true\"", separator.RazorSource, StringComparison.Ordinal);
        var updated = Render(separator.Preview);
        Assert.Equal("vertical", updated.Find("[data-slot='separator']").GetAttribute("data-orientation"));
        Assert.Equal("none", updated.Find("[data-slot='separator']").GetAttribute("role"));
        Assert.Equal("true", updated.Find("[data-slot='separator']").GetAttribute("aria-hidden"));
        Assert.Contains("showcase-separator-demo--vertical", updated.Find(".showcase-separator-demo").ClassList);
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
