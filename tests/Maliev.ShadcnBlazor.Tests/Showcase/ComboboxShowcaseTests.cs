using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ComboboxShowcaseTests : BunitContext
{
    public ComboboxShowcaseTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-forms.js");
        module.SetupVoid("observePopupDismissal", _ => true);
        module.SetupVoid("disconnectPopupDismissal", _ => true);
        module.SetupVoid("focusElement", _ => true);
    }

    [Fact]
    public void ComboboxDossierSupportsDirectTriggerSelectionClearAndDocumentedStates()
    {
        Services.AddMalievShadcn();
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var example = registry.GetBySlug("combobox").Single();

        Assert.Contains("@bind-Value=\"SelectedMaterial\"", example.RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("@bind-Values=\"SelectedMaterials\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind-Open=\"IsOpen\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@bind-Query=\"Query\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowClear=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("ShowTrigger=\"true\"", example.RazorSource, StringComparison.Ordinal);

        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));
        var input = cut.Find("[data-testid='forms-dossier-combobox']");
        var root = cut.Find("[data-slot='combobox']");
        var trigger = root.QuerySelector("[data-slot='combobox-trigger']")!;

        Assert.Equal("false", input.GetAttribute("aria-expanded"));
        trigger.Click();
        Assert.Equal("true", input.GetAttribute("aria-expanded"));

        cut.Find("[data-slot='combobox-item'][data-value='peek']").Click();
        Assert.Equal("PEEK", input.GetAttribute("value"));
        Assert.Equal("false", input.GetAttribute("aria-expanded"));

        input.Focus();
        Assert.Equal("true", input.GetAttribute("aria-expanded"));
        cut.Find("[data-slot='combobox-clear']").Click();
        Assert.Equal(string.Empty, input.GetAttribute("value"));

        Change(cut, "combobox-invalid", true);
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Change(cut, "combobox-multiple", true);
        Assert.Equal("true", root.QuerySelector("[data-slot='combobox-list']")!.GetAttribute("aria-multiselectable"));
        Assert.Equal(2, root.QuerySelectorAll("[data-slot='combobox-chip']").Length);
        Assert.Contains("@bind-Values=\"SelectedMaterials\"", example.RazorSourceProvider!(), StringComparison.Ordinal);
        Assert.DoesNotContain("@bind-Value=\"SelectedMaterial\"", example.RazorSourceProvider!(), StringComparison.Ordinal);
    }

    [Fact]
    public void ComboboxStylesKeepClearActionInFieldAndInvalidStateVisible()
    {
        var root = FindRoot();
        var css = File.ReadAllText(Path.Combine(root, "src", "Maliev.ShadcnBlazor", "wwwroot", "css", "shadcn-forms.css"));

        Assert.Contains(".shadcn-combobox-input:has(.shadcn-combobox-control[aria-invalid=\"true\"])", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-combobox-input [data-slot=\"combobox-clear\"] { position: static", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-combobox-input [data-slot=\"combobox-trigger\"] { position: static", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-combobox-input [data-slot=\"combobox-clear\"]:focus-visible { background: transparent", css, StringComparison.Ordinal);
        Assert.Contains(".shadcn-combobox-content { inset-inline-end: auto", css, StringComparison.Ordinal);
    }

    private static void Change(IRenderedComponent<ComponentPreview> cut, string controlId, object value) =>
        cut.ChangeControl(controlId, value);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory!.FullName;
    }
}
