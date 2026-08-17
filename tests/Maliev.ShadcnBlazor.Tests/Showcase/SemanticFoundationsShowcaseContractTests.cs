namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class SemanticFoundationsShowcaseContractTests
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
    }

    [Fact]
    public void ShowcaseLoadsSemanticStylesAndExposesComponentRoute()
    {
        var root = FindRoot();
        var index = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "index.html"));
        var layout = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Layout", "MainLayout.razor"));
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "SemanticFoundations.razor"));

        Assert.Contains("_content/Maliev.ShadcnBlazor/css/shadcn-semantic-foundations.css", index, StringComparison.Ordinal);
        Assert.Contains("components/semantic-foundations", layout, StringComparison.Ordinal);
        Assert.Contains("@page \"/components/semantic-foundations\"", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnAspectRatio", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnField", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnItem", page, StringComparison.Ordinal);
        Assert.DoesNotContain(">M</span></ShadcnItemMedia>", page, StringComparison.Ordinal);
        Assert.Contains("<svg aria-hidden=\"true\" viewBox=\"0 0 24 24\"", page, StringComparison.Ordinal);
        Assert.Contains("<ShadcnEmpty", page, StringComparison.Ordinal);
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

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException();
    }
}
