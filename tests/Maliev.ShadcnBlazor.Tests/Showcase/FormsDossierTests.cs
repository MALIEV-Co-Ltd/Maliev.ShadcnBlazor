using Bunit;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Microsoft.JSInterop;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class FormsDossierTests : BunitContext
{
    private static readonly string[] Slugs = ["calendar", "combobox", "date-picker", "input", "input-group", "input-otp", "native-select", "select", "textarea"];

    public FormsDossierTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void EveryCertifiedPlanFourComponentHasAnOperableRealComponentDossier()
    {
        var documentation = new ComponentDocumentationCatalog();
        var registry = new ComponentExampleRegistry(documentation);

        foreach (var slug in Slugs)
        {
            Assert.Equal(ComponentDocumentationStatus.Complete, documentation.FindBySlug(slug)!.Status);
            var example = Assert.Single(registry.GetBySlug(slug));
            Assert.Equal($"{slug}-primary", example.Id);
            Assert.NotEmpty(example.Controls);
            Assert.NotEmpty(example.StateTags);
            Assert.Contains("Shadcn", example.RazorSource, StringComparison.Ordinal);
            var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));
            Assert.NotEmpty(cut.FindAll("[data-slot]"));
            var control = example.Controls[0];
            var original = cut.Markup;
            object nextValue = control.Kind == ComponentParameterControlKind.Toggle ? !bool.Parse(control.Value) : control.Options.Last();
            cut.Find($"[data-testid='control-{control.Id}']").Change(nextValue);
            Assert.NotEqual(original, cut.Markup);
        }
    }

    [Fact]
    public void IndependentlyAcceptedDossierEvidenceCompletesAllNinePlanFourComponents()
    {
        var documentation = new ComponentDocumentationCatalog();

        foreach (var slug in Slugs)
        {
            var entry = documentation.FindBySlug(slug)!;
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
            Assert.True(entry.Evidence.Api);
            Assert.True(entry.Evidence.ComponentTests);
            Assert.True(entry.Evidence.Accessibility);
            Assert.True(entry.Evidence.Interaction);
            Assert.True(entry.Evidence.ComputedStyle);
            Assert.True(entry.Evidence.Integration);
            Assert.True(entry.Evidence.Visual);
        }

        Assert.Equal(64, documentation.All.Count(entry => entry.Status == ComponentDocumentationStatus.Complete));
        Assert.DoesNotContain(documentation.All, entry => entry.Status == ComponentDocumentationStatus.Planned);
    }

    [Fact]
    public void PlanFourDossiersNameRequiredBehaviorAndCustomizationStates()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var tags = Slugs.ToDictionary(slug => slug, slug => registry.GetBySlug(slug).Single().StateTags, StringComparer.Ordinal);

        Assert.Contains("file", tags["input"]);
        Assert.Contains("read-only", tags["native-select"]);
        Assert.Contains("graphemes", tags["input-otp"]);
        Assert.Contains("multiple", tags["combobox"]);
        Assert.Contains("open", tags["select"]);
        Assert.Contains("range", tags["calendar"]);
        Assert.Contains("culture", tags["date-picker"]);
        Assert.All(tags.Values, states => Assert.Contains("invalid", states));
    }

    [Fact]
    public void PlanFourCatalogEntriesExposeRealPrimaryTypesAliasesAndCapabilities()
    {
        var catalog = new ComponentDocumentationCatalog();
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["calendar"] = "ShadcnCalendar",
            ["combobox"] = "ShadcnCombobox`1",
            ["date-picker"] = "ShadcnDatePicker",
            ["input"] = "ShadcnInput`1",
            ["input-group"] = "ShadcnInputGroup",
            ["input-otp"] = "ShadcnInputOtp",
            ["native-select"] = "ShadcnNativeSelect`1",
            ["select"] = "ShadcnSelect`1",
            ["textarea"] = "ShadcnTextarea`1"
        };

        foreach (var (slug, primaryType) in expected)
        {
            var entry = catalog.FindBySlug(slug)!;
            Assert.Equal("Maliev.ShadcnBlazor.Components.Forms", entry.Namespace);
            Assert.Equal(primaryType, entry.PrimaryType);
            Assert.NotEmpty(entry.Aliases);
            Assert.NotEmpty(entry.Capabilities);
            Assert.Equal(ComponentDocumentationStatus.Complete, entry.Status);
        }
    }

    [Fact]
    public void PlanFourDocumentationNamesAccessibilityAndPinnedSourceContracts()
    {
        var root = FindRoot();
        var notes = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Components", "Documentation", "ComponentAccessibilityNotes.razor"));
        var page = File.ReadAllText(Path.Combine(root, "samples", "Maliev.ShadcnBlazor.Showcase", "Pages", "Docs", "ComponentDocumentation.razor"));
        foreach (var slug in Slugs) Assert.Contains($"\"{slug}\"", notes, StringComparison.Ordinal);
        foreach (var source in new[] { "calendar.tsx", "combobox.tsx", "input.tsx", "input-group.tsx", "input-otp.tsx", "native-select.tsx", "select.tsx", "textarea.tsx", "popover.tsx", "button.tsx" })
            Assert.Contains(source, page, StringComparison.Ordinal);
        Assert.Contains("foreach (var upstreamPath in UpstreamPaths)", page, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanFourApiCatalogOwnsEveryPublicCompositionTypeWithoutInternalContexts()
    {
        var documentation = new ComponentDocumentationCatalog();
        var api = new ComponentApiCatalog();
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["calendar"] = ["ShadcnCalendar", "ShadcnDateRange"],
            ["combobox"] = ["ShadcnCombobox`1", "ShadcnComboboxOption`1"],
            ["date-picker"] = ["ShadcnDatePicker", "ShadcnDateRange"],
            ["input"] = ["ShadcnInput`1"],
            ["input-group"] = ["ShadcnInputGroup", "ShadcnInputGroupAddon", "ShadcnInputGroupButton", "ShadcnInputGroupText"],
            ["input-otp"] = ["ShadcnInputOtp", "ShadcnInputOtpGroup", "ShadcnInputOtpSeparator", "ShadcnInputOtpSlot"],
            ["native-select"] = ["ShadcnNativeSelect`1", "ShadcnNativeSelectOptGroup", "ShadcnNativeSelectOption`1"],
            ["select"] = ["ShadcnSelect`1", "ShadcnSelectOption`1"],
            ["textarea"] = ["ShadcnTextarea`1"]
        };

        foreach (var (slug, types) in expected)
        {
            var descriptors = api.GetByEntry(documentation.FindBySlug(slug)!).Select(item => item.FullTypeName.Split('.').Last()).Where(name => name != "ShadcnComponentBase").Order().ToArray();
            Assert.Equal(types.Order(), descriptors);
            Assert.DoesNotContain(descriptors, name => name.EndsWith("Context", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void EveryPlanFourDossierControlChangesItsClaimedLiveState()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());

        var input = RenderExample(registry, "input");
        Change(input, "input-invalid", true);
        Assert.Equal("true", input.Find("[data-testid='forms-dossier-input']").GetAttribute("aria-invalid"));
        Change(input, "input-type", "file");
        Assert.Equal("file", input.Find("[data-testid='forms-dossier-input']").GetAttribute("type"));

        var textarea = RenderExample(registry, "textarea");
        Change(textarea, "textarea-invalid", true);
        Assert.Equal("true", textarea.Find("[data-testid='forms-dossier-textarea']").GetAttribute("aria-invalid"));
        Change(textarea, "textarea-rows", "5");
        Assert.Equal("5", textarea.Find("[data-testid='forms-dossier-textarea']").GetAttribute("rows"));

        var nativeSelect = RenderExample(registry, "native-select");
        Change(nativeSelect, "native-select-invalid", true);
        Assert.Equal("true", nativeSelect.Find("[data-testid='forms-dossier-native-select']").GetAttribute("aria-invalid"));
        Change(nativeSelect, "native-select-readonly", true);
        Assert.Equal("true", nativeSelect.Find("[data-testid='forms-dossier-native-select']").GetAttribute("aria-readonly"));

        var inputGroup = RenderExample(registry, "input-group");
        Change(inputGroup, "input-group-invalid", true);
        Assert.Equal("true", inputGroup.Find("[data-testid='forms-dossier-input-group']").GetAttribute("aria-invalid"));
        Change(inputGroup, "input-group-alignment", "BlockEnd");
        Assert.Equal("block-end", inputGroup.Find("[data-slot='input-group-addon']").GetAttribute("data-align"));

        var otp = RenderExample(registry, "input-otp");
        Change(otp, "input-otp-invalid", true);
        Assert.Equal("true", otp.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("aria-invalid"));
        Change(otp, "input-otp-numeric", false);
        Assert.Equal("text", otp.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("inputmode"));
        Assert.Null(otp.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("data-pattern"));

        var select = RenderExample(registry, "select");
        Change(select, "select-invalid", true);
        Assert.Equal("true", select.Find("[data-testid='forms-dossier-select']").GetAttribute("aria-invalid"));
        Change(select, "select-open", true);
        Assert.Equal("true", select.Find("[data-testid='forms-dossier-select']").GetAttribute("aria-expanded"));

        var combobox = RenderExample(registry, "combobox");
        Change(combobox, "combobox-invalid", true);
        Assert.Equal("true", combobox.Find("[data-testid='forms-dossier-combobox']").GetAttribute("aria-invalid"));
        Change(combobox, "combobox-multiple", true);
        Assert.Equal("true", combobox.Find("[data-slot='combobox-list']").GetAttribute("aria-multiselectable"));
        Assert.Equal(2, combobox.FindAll("[data-slot='combobox-chip']").Count);

        var calendar = RenderExample(registry, "calendar");
        Change(calendar, "calendar-invalid", true);
        Assert.Equal("true", calendar.Find("[data-testid='forms-dossier-calendar']").GetAttribute("aria-invalid"));
        Change(calendar, "calendar-mode", "Range");
        Assert.NotEmpty(calendar.FindAll("[data-range-start='true']"));
        Assert.NotEmpty(calendar.FindAll("[data-range-end='true']"));

        var datePicker = RenderExample(registry, "date-picker");
        Change(datePicker, "date-picker-invalid", true);
        Assert.Equal("true", datePicker.Find("[data-testid='forms-dossier-date-picker']").GetAttribute("aria-invalid"));
        Assert.Single(datePicker.FindAll("[data-slot='date-picker-clear']"));
        Change(datePicker, "date-picker-clearable", false);
        Assert.Empty(datePicker.FindAll("[data-slot='date-picker-clear']"));
    }

    [Fact]
    public void PlanFourCoveredStateTagsOnlyNameRenderedOrOperableDossierStates()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["input"] = ["typed-binding", "required", "file", "invalid"],
            ["textarea"] = ["typed-binding", "rows", "invalid"],
            ["native-select"] = ["selected", "read-only", "invalid"],
            ["input-group"] = ["addons", "inline", "block", "invalid"],
            ["input-otp"] = ["one-input", "graphemes", "numeric", "invalid"],
            ["select"] = ["selected", "groups", "clearable", "open", "invalid"],
            ["combobox"] = ["selected", "multiple", "chips", "open", "invalid"],
            ["calendar"] = ["single", "range", "culture", "invalid"],
            ["date-picker"] = ["single", "text-input", "culture", "clearable", "invalid"]
        };

        foreach (var (slug, tags) in expected)
            Assert.Equal(tags, registry.GetBySlug(slug).Single().StateTags);
    }

    private IRenderedComponent<ComponentPreview> RenderExample(ComponentExampleRegistry registry, string slug) =>
        Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, registry.GetBySlug(slug).Single()));

    private static void Change(IRenderedComponent<ComponentPreview> cut, string controlId, object value) =>
        cut.Find($"[data-testid='control-{controlId}']").Change(value);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory!.FullName;
    }
}
