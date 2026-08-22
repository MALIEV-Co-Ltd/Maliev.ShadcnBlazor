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
            cut.ChangeControl(control.Id, nextValue);
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

        Assert.Equal(65, documentation.All.Count(entry => entry.Status == ComponentDocumentationStatus.Complete));
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
        Assert.Single(input.FindAll("[data-slot='card']"));
        Assert.Equal(2, input.FindAll("input[data-slot='input']").Count);
        Assert.Equal("integration-key", input.Find("[data-slot='field-label']").GetAttribute("for"));
        Assert.Equal("integration-key-help", input.Find("[data-testid='forms-dossier-input']").GetAttribute("aria-describedby"));
        Assert.Contains("<ShadcnCard>", input.Instance.Example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Type=\"file\"", input.Instance.Example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("HandleCredentialFile", input.Instance.Example.RazorSource, StringComparison.Ordinal);
        Change(input, "input-invalid", true);
        Assert.Equal("true", input.Find("[data-testid='forms-dossier-input']").GetAttribute("aria-invalid"));
        Assert.Equal("integration-key-help integration-key-error", input.Find("[data-testid='forms-dossier-input']").GetAttribute("aria-describedby"));
        Assert.Single(input.FindAll("[data-slot='field-error'][role='alert']"));
        Assert.Contains("Invalid=\"true\"", input.Instance.Example.RazorSource, StringComparison.Ordinal);
        Change(input, "input-masked", false);
        Assert.Equal("text", input.Find("[data-testid='forms-dossier-input']").GetAttribute("type"));
        Assert.Contains("Type=\"text\"", input.Instance.Example.RazorSource, StringComparison.Ordinal);
        Change(input, "input-readonly", true);
        Assert.True(input.Find("[data-testid='forms-dossier-input']").HasAttribute("readonly"));
        Change(input, "input-disabled", true);
        Assert.True(input.Find("[data-testid='forms-dossier-input']").HasAttribute("disabled"));
        Assert.True(input.Find("[data-testid='forms-dossier-file']").HasAttribute("disabled"));
        Assert.True(input.Find("[data-testid='forms-dossier-save']").HasAttribute("disabled"));

        var textarea = RenderExample(registry, "textarea");
        Change(textarea, "textarea-invalid", true);
        Assert.Equal("true", textarea.Find("[data-testid='forms-dossier-textarea']").GetAttribute("aria-invalid"));
        Change(textarea, "textarea-rows", "5");
        Assert.Equal("5", textarea.Find("[data-testid='forms-dossier-textarea']").GetAttribute("rows"));

        var nativeSelect = RenderExample(registry, "native-select");
        Assert.Single(nativeSelect.FindAll("[data-testid='native-select-dossier-preview']"));
        Assert.Equal(2, nativeSelect.FindAll("[data-testid='forms-dossier-native-select'] optgroup").Count);
        Assert.Single(nativeSelect.FindAll("[data-testid='forms-dossier-native-select'] option:disabled"));
        Assert.Contains("5–7 business days", nativeSelect.Find("[data-testid='native-select-lead-time']").TextContent, StringComparison.Ordinal);
        nativeSelect.Find("[data-testid='forms-dossier-native-select']").Change("urgent");
        Assert.Contains("2–3 business days", nativeSelect.Find("[data-testid='native-select-lead-time']").TextContent, StringComparison.Ordinal);
        Change(nativeSelect, "native-select-invalid", true);
        Assert.Equal("true", nativeSelect.Find("[data-testid='forms-dossier-native-select']").GetAttribute("aria-invalid"));
        Change(nativeSelect, "native-select-readonly", true);
        Assert.Equal("true", nativeSelect.Find("[data-testid='forms-dossier-native-select']").GetAttribute("aria-readonly"));
        Change(nativeSelect, "native-select-compact", true);
        Assert.Equal("sm", nativeSelect.Find("[data-testid='forms-dossier-native-select']").GetAttribute("data-size"));

        var inputGroupExample = registry.GetBySlug("input-group").Single();
        var inputGroup = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, inputGroupExample));
        Assert.Contains("Part estimate", inputGroup.Markup, StringComparison.Ordinal);
        Assert.Contains("12 machined parts", inputGroup.Markup, StringComparison.Ordinal);
        Assert.Equal("ghost", inputGroup.Find("[data-testid='input-group-reset']").GetAttribute("data-variant"));
        inputGroup.Find("[data-slot='input-group-control']").Input("1000");
        Assert.Contains("12,000", inputGroup.Find("[data-testid='input-group-subtotal']").TextContent, StringComparison.Ordinal);
        inputGroup.Find("[data-testid='input-group-reset']").Click();
        Assert.Contains("15,000", inputGroup.Find("[data-testid='input-group-subtotal']").TextContent, StringComparison.Ordinal);
        Change(inputGroup, "input-group-invalid", true);
        Assert.Equal("true", inputGroup.Find("[data-testid='forms-dossier-input-group']").GetAttribute("aria-invalid"));
        Change(inputGroup, "input-group-alignment", "BlockEnd");
        Assert.Equal("block-end", inputGroup.Find("[data-slot='input-group-addon']").GetAttribute("data-align"));
        Assert.Contains("ShadcnInputGroupAlignment.BlockEnd", inputGroupExample.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Invalid=\"true\"", inputGroupExample.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private void ResetUnitPrice()", inputGroupExample.RazorSource, StringComparison.Ordinal);
        Assert.Contains("@using System.Globalization", inputGroupExample.RazorSource, StringComparison.Ordinal);

        var otp = RenderExample(registry, "input-otp");
        Change(otp, "input-otp-invalid", true);
        Assert.Equal("true", otp.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("aria-invalid"));
        Change(otp, "input-otp-numeric", false);
        Assert.Equal("text", otp.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("inputmode"));
        Assert.Null(otp.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("data-pattern"));

        var selectExample = registry.GetBySlug("select").Single();
        var select = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, selectExample));
        select.Find("[data-testid='forms-dossier-select']").Click();
        Assert.NotEmpty(select.FindAll("[role='listbox']"));
        select.Find("[role='option'][data-value='slm']").Click();
        Assert.Equal("Metal 3D printing", select.Find("[data-slot='select-value']").TextContent);
        Change(select, "select-invalid", true);
        Assert.Equal("true", select.Find("[data-testid='forms-dossier-select']").GetAttribute("aria-invalid"));
        Assert.Empty(select.FindAll("#select-open"));
        var source = selectExample.RazorSourceProvider!();
        Assert.Contains("Invalid=\"true\"", source, StringComparison.Ordinal);
        Assert.Contains("ProcessOptions", source, StringComparison.Ordinal);
        Assert.DoesNotContain("@bind-Open", source, StringComparison.Ordinal);

        var combobox = RenderExample(registry, "combobox");
        Change(combobox, "combobox-invalid", true);
        Assert.Equal("true", combobox.Find("[data-testid='forms-dossier-combobox']").GetAttribute("aria-invalid"));
        Change(combobox, "combobox-multiple", true);
        combobox.Find("[data-slot='combobox-input']").TriggerEvent("onfocus", new Microsoft.AspNetCore.Components.Web.FocusEventArgs());
        Assert.Equal("true", combobox.Find("[data-slot='combobox-list']").GetAttribute("aria-multiselectable"));
        Assert.Equal(2, combobox.FindAll("[data-slot='combobox-chip']").Count);

        var calendar = RenderExample(registry, "calendar");
        Change(calendar, "calendar-invalid", true);
        Assert.Equal("true", calendar.Find("[data-testid='forms-dossier-calendar']").GetAttribute("aria-invalid"));
        Change(calendar, "calendar-mode", "Range");
        Assert.NotEmpty(calendar.FindAll("[data-range-start='true']"));
        Assert.NotEmpty(calendar.FindAll("[data-range-end='true']"));

        var datePickerExample = registry.GetBySlug("date-picker").Single();
        var datePicker = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, datePickerExample));
        Assert.Empty(datePicker.FindAll("[data-slot='date-picker-content']"));
        datePicker.Find("[data-testid='forms-dossier-date-picker']").Click();
        Assert.Single(datePicker.FindAll("[data-slot='date-picker-content']"));
        Change(datePicker, "date-picker-invalid", true);
        Assert.Equal("true", datePicker.Find("[data-testid='forms-dossier-date-picker']").GetAttribute("aria-invalid"));
        Assert.Single(datePicker.FindAll("[data-slot='date-picker-clear']"));
        Change(datePicker, "date-picker-clearable", false);
        Assert.Empty(datePicker.FindAll("[data-slot='date-picker-clear']"));
        Change(datePicker, "date-picker-mode", "Single");
        Assert.NotEmpty(datePicker.FindAll("[data-selected-single='true']"));
        Assert.Contains("@bind-Value=\"SelectedDate\"", datePickerExample.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void InputOtpDossierDemonstratesACompleteInteractiveVerificationFlow()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var example = registry.GetBySlug("input-otp").Single();
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        Assert.Single(cut.FindAll(".showcase-otp-card[data-testid='input-otp-dossier-preview']"));
        Assert.Equal(2, cut.FindAll("[data-slot='input-otp-group']").Count);
        Assert.Equal(6, cut.FindAll("[data-slot='input-otp-slot']").Count);
        Assert.Single(cut.FindAll("[data-slot='input-otp-separator']"));
        Assert.Equal("polite", cut.Find("[data-testid='input-otp-status']").GetAttribute("aria-live"));
        Assert.Equal(string.Empty, cut.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("value"));

        var verify = cut.Find("[data-testid='input-otp-verify']");
        Assert.True(verify.HasAttribute("disabled"));
        cut.Find("[data-testid='forms-dossier-input-otp']").Input("246810");
        verify = cut.Find("[data-testid='input-otp-verify']");
        Assert.False(verify.HasAttribute("disabled"));
        verify.Click();
        Assert.Contains("verified", cut.Find("[data-testid='input-otp-status']").TextContent, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("<ShadcnInputOtpSeparator />", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("private Task VerifyCode", example.RazorSource, StringComparison.Ordinal);
        Change(cut, "input-otp-invalid", true);
        Assert.Contains("Invalid=\"true\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Equal("true", cut.Find("[data-testid='forms-dossier-input-otp']").GetAttribute("aria-invalid"));
        Change(cut, "input-otp-numeric", false);
        Assert.DoesNotContain("Pattern=", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("InputMode=\"text\"", example.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TextareaDossierKeepsRowsValidationAndCopyableSourceInSync()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var definition = registry.GetBySlug("textarea").Single();
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, definition));

        var dossier = cut.Find("[data-testid='textarea-dossier-preview']");
        Assert.Equal("manufacturing-notes", dossier.QuerySelector("[data-slot='field-label']")!.GetAttribute("for"));
        Assert.Contains("drawing", dossier.QuerySelector("[data-slot='field-description']")!.TextContent, StringComparison.OrdinalIgnoreCase);

        Change(cut, "textarea-rows", "5");
        var textarea = dossier.QuerySelector("textarea[data-slot='textarea']")!;
        Assert.Equal("5", textarea.GetAttribute("rows"));
        Assert.Contains("Rows=\"5\"", definition.RazorSource, StringComparison.Ordinal);
        Assert.Contains("<ShadcnField", definition.RazorSource, StringComparison.Ordinal);

        Change(cut, "textarea-invalid", true);
        textarea = cut.Find("textarea[data-slot='textarea']");
        Assert.Equal("true", textarea.GetAttribute("aria-invalid"));
        Assert.Contains("manufacturing-notes-error", textarea.GetAttribute("aria-describedby"), StringComparison.Ordinal);
        Assert.Contains("Add the critical", dossier.QuerySelector("[data-slot='field-error']")!.TextContent, StringComparison.Ordinal);
        Assert.Contains("Invalid=\"true\"", definition.RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanFourCoveredStateTagsOnlyNameRenderedOrOperableDossierStates()
    {
        var registry = new ComponentExampleRegistry(new ComponentDocumentationCatalog());
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["input"] = ["typed-binding", "required", "file", "invalid", "disabled", "read-only"],
            ["textarea"] = ["typed-binding", "rows", "invalid"],
            ["native-select"] = ["selected", "groups", "disabled", "read-only", "invalid", "sm"],
            ["input-group"] = ["addons", "inline", "block", "button", "invalid", "rtl"],
            ["input-otp"] = ["one-input", "paste", "keyboard", "status", "graphemes", "numeric", "invalid"],
            ["select"] = ["selected", "groups", "clearable", "open", "invalid"],
            ["combobox"] = ["selected", "multiple", "chips", "open", "invalid"],
            ["calendar"] = ["single", "range", "culture", "keyboard", "week-numbers", "invalid"],
            ["date-picker"] = ["single", "range", "calendar", "culture", "clearable", "invalid"]
        };

        foreach (var (slug, tags) in expected)
            Assert.Equal(tags, registry.GetBySlug(slug).Single().StateTags);
    }

    private IRenderedComponent<ComponentPreview> RenderExample(ComponentExampleRegistry registry, string slug) =>
        Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, registry.GetBySlug(slug).Single()));

    private static void Change(IRenderedComponent<ComponentPreview> cut, string controlId, object value) =>
        cut.ChangeControl(controlId, value);

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx"))) directory = directory.Parent;
        return directory!.FullName;
    }
}
