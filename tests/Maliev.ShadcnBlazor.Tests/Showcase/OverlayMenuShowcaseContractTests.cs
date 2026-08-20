using Bunit;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class OverlayMenuShowcaseContractTests : BunitContext
{
    private static readonly string[] Slugs = ["alert-dialog", "command", "context-menu", "dialog", "drawer", "dropdown-menu", "hover-card", "menubar", "popover", "sheet", "tooltip"];

    public OverlayMenuShowcaseContractTests() => JSInterop.Mode = JSRuntimeMode.Loose;

    [Fact]
    public void ShowcaseRegistersExampleRegistryWithNonCachingLifetime()
    {
        var source = File.ReadAllText(Path.Combine(FindRoot(), "samples", "Maliev.ShadcnBlazor.Showcase", "Program.cs"));
        Assert.Contains("AddTransient<IComponentExampleRegistry, ComponentExampleRegistry>()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AddSingleton<IComponentExampleRegistry, ComponentExampleRegistry>()", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryPlanSevenComponentHasARealCompleteDossierAndAuthoritativeApi()
    {
        var catalog = new ComponentDocumentationCatalog(); var api = new ComponentApiCatalog(); var registry = new ComponentExampleRegistry(catalog);
        foreach (var slug in Slugs)
        {
            var entry = Assert.IsType<ComponentDocumentationEntry>(catalog.FindBySlug(slug));
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
            Assert.All(new[] { entry.Evidence.Api, entry.Evidence.ComponentTests, entry.Evidence.Accessibility, entry.Evidence.Interaction, entry.Evidence.ComputedStyle, entry.Evidence.Visual, entry.Evidence.Integration }, Assert.True);
            Assert.Equal("Maliev.ShadcnBlazor.Components.Overlays", entry.Namespace);
            Assert.NotNull(entry.PrimaryType);
            Assert.True(api.GetByEntry(entry).Count >= 3, $"{slug} API ownership is incomplete.");
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.Equal($"{slug}-primary", example.Id);
            Assert.NotEmpty(example.Controls); Assert.NotEmpty(example.StateTags);
            Assert.Contains("<Shadcn", example.RazorSource, StringComparison.Ordinal);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
    }

    [Fact]
    public void EveryPlanSevenDossierControlMutatesItsRenderedCanvas()
    {
        foreach (var slug in Slugs)
            foreach (var controlId in new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single().Controls.Select(control => control.Id).ToArray())
            {
                var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug(slug).Single();
                var control = example.Controls.Single(candidate => candidate.Id == controlId);
                var before = Render(example.Preview).Markup;
                var alternate = control.Kind switch
                {
                    ComponentParameterControlKind.Toggle => bool.Parse(control.Value) ? "false" : "true",
                    ComponentParameterControlKind.Select => control.Options.First(option => !string.Equals(option, control.Value, StringComparison.Ordinal)),
                    ComponentParameterControlKind.Number => (int.Parse(control.Value, System.Globalization.CultureInfo.InvariantCulture) + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    _ => throw new ArgumentOutOfRangeException(nameof(control.Kind), control.Kind, "Unknown dossier control kind.")
                };
                control.Apply(alternate);
                Assert.NotEqual(before, Render(example.Preview).Markup);
            }
    }

    [Fact]
    public void InteractiveOverlaysStartClosedAndWaitForAUserTrigger()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        foreach (var slug in new[] { "alert-dialog", "dialog", "drawer", "dropdown-menu", "hover-card", "popover", "sheet", "tooltip" })
        {
            var markup = Render(registry.GetBySlug(slug).Single().Preview).Markup;
            Assert.DoesNotContain("data-state=\"open\"", markup, StringComparison.Ordinal);
        }

        var menubar = Render(registry.GetBySlug("menubar").Single().Preview).Markup;
        Assert.DoesNotContain("data-state=\"open\"", menubar, StringComparison.Ordinal);
    }

    [Fact]
    public void MenubarDossierUsesTheCompleteLibraryCompositionAndKeepsSourceInSync()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("menubar").Single();
        var rendered = Render(example.Preview);
        var markup = rendered.Markup;

        Assert.Contains("data-testid=\"menubar-dossier-preview\"", markup, StringComparison.Ordinal);
        Assert.Equal(4, rendered.FindAll("[data-slot='menubar-trigger']").Count);
        Assert.Contains("<ShadcnMenubarCheckboxItem", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMenubarRadioItem", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMenubarSubTrigger>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnMenubarContent>", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Loop=\"true\"", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "menubar-loop").Apply("false");
        example.Controls.Single(control => control.Id == "menubar-status").Apply("false");

        Assert.Contains("Loop=\"false\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Checked=\"false\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Equal("false", Render(example.Preview).Find("[data-slot='menubar']").GetAttribute("data-loop"));
    }

    [Fact]
    public void PopoverDossierUsesARealClosedByDefaultCompositionAndStateAwareSource()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("popover").Single();
        var markup = Render(example.Preview).Markup;

        Assert.Contains("showcase-popover-dossier", markup, StringComparison.Ordinal);
        Assert.Contains("Edit part dimensions", markup, StringComparison.Ordinal);
        Assert.DoesNotContain("data-state=\"open\"", markup, StringComparison.Ordinal);
        Assert.DoesNotContain(example.Controls, control => control.Id == "popover-open");
        Assert.Contains("@bind-Open=\"Open\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnPopoverHeader", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShadcnInput", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "popover-top").Apply("true");
        Assert.Contains("Side=\"ShadcnOverlaySide.Top\"", example.RazorSource, StringComparison.Ordinal);
        example.Controls.Single(control => control.Id == "popover-outside").Apply("false");
        Assert.Contains("CloseOnOutsidePress=\"false\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void DialogDossierShowsAnEditableProfileAndKeepsItsSourceInSync()
    {
        var dialog = Assert.Single(new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("dialog"));

        Assert.Equal("Editable profile dialog", dialog.Title);
        Assert.Contains("<ShadcnDialogTrigger", dialog.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnInput", dialog.RazorSource, StringComparison.Ordinal);
        Assert.Contains("id=\"dialog-profile-name\"", dialog.RazorSource, StringComparison.Ordinal);
        Assert.Contains("id=\"dialog-profile-username\"", dialog.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowCloseButton=\"true\"", dialog.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Modal=\"true\"", dialog.RazorSource, StringComparison.Ordinal);

        var initial = Render(dialog.Preview);
        Assert.Empty(initial.FindAll("[data-slot='dialog-content']"));
        Assert.Contains("Edit profile", initial.Find("[data-slot='dialog-trigger']").TextContent, StringComparison.Ordinal);

        dialog.Controls.Single(control => control.Id == "dialog-open").Apply("true");
        var opened = Render(dialog.Preview);
        Assert.NotEmpty(opened.FindAll("[data-slot='dialog-content']"));
        Assert.Equal(2, opened.FindAll("[data-slot='input']").Count);
        Assert.Contains("Open=\"true\"", dialog.RazorSource, StringComparison.Ordinal);

        dialog.Controls.Single(control => control.Id == "dialog-variant").Apply("true");
        Assert.Contains("Modal=\"false\"", dialog.RazorSource, StringComparison.Ordinal);
        Assert.Null(Render(dialog.Preview).Find("[data-slot='dialog-content']").GetAttribute("aria-modal"));
    }

    [Fact]
    public void CommandDossierUsesACompleteStateAwareRealWorldComposition()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("command").Single();
        var markup = Render(example.Preview).Markup;

        Assert.Contains("showcase-command-dossier", markup, StringComparison.Ordinal);
        Assert.Contains("Workspace command palette", markup, StringComparison.Ordinal);
        Assert.Contains("Create quotation", markup, StringComparison.Ordinal);
        Assert.Contains("Command selected", markup, StringComparison.Ordinal);
        Assert.Contains("<ShadcnCommandInput", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnCommandGroup Heading=\"Workspace\">", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnCommandShortcut>", example.RazorSource, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "command-disabled").Apply("true");
        Assert.Contains("Disabled=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("data-disabled=\"true\"", Render(example.Preview).Markup, StringComparison.Ordinal);

        example.Controls.Single(control => control.Id == "command-empty").Apply("true");
        Assert.Contains("Value=\"no matching command\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("value=\"no matching command\"", Render(example.Preview).Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void SheetDossierUsesTheLiveTriggerAndKeepsItsSourceInSyncWithEveryEdge()
    {
        var example = new ComponentExampleRegistry(new ComponentDocumentationCatalog()).GetBySlug("sheet").Single();
        var cut = Render(example.Preview);

        Assert.Empty(cut.FindAll("[data-slot='sheet-content']"));
        cut.Find("[data-slot='sheet-trigger']").Click();
        Assert.Equal("right", cut.Find("[data-slot='sheet-content']").GetAttribute("data-side"));
        Assert.Contains("@bind-Open=\"open\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Side=\"ShadcnSheetSide.Right\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnInput", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnSwitch", example.RazorSource, StringComparison.Ordinal);

        cut.Find("[data-slot='sheet-close']").Click();
        Assert.Empty(cut.FindAll("[data-slot='sheet-content']"));

        var sideControl = example.Controls.Single(control => control.Id == "sheet-side");
        foreach (var side in Enum.GetNames<Maliev.ShadcnBlazor.Components.Overlays.ShadcnSheetSide>())
        {
            sideControl.Apply(side);
            var rerendered = Render(example.Preview);
            rerendered.Find("[data-slot='sheet-trigger']").Click();
            Assert.Equal(side.ToLowerInvariant(), rerendered.Find("[data-slot='sheet-content']").GetAttribute("data-side"));
            Assert.Contains($"Side=\"ShadcnSheetSide.{side}\"", example.RazorSource, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DocumentationRouteLinksEveryPlanSevenPinnedAndCurrentReference()
    {
        var root = FindRoot();
        var route = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        foreach (var slug in Slugs)
        {
            Assert.Contains($"\"{slug}\" =>", route, StringComparison.Ordinal);
            Assert.Contains($"ui/{slug}.tsx", route, StringComparison.Ordinal);
        }
        Assert.Contains("6261bd89f72d794aea491482cc2acfd8dc3d63e2", route, StringComparison.Ordinal);
        Assert.Contains("https://ui.shadcn.com/docs/components/base/", route, StringComparison.Ordinal);
    }

    private static string FindRoot() { var directory = new DirectoryInfo(AppContext.BaseDirectory); while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent; return directory?.FullName ?? throw new DirectoryNotFoundException(); }
}
