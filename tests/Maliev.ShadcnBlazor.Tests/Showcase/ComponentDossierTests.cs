using System.Reflection;
using Bunit;
using Maliev.ShadcnBlazor.Components.Primitives;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation;
using Maliev.ShadcnBlazor.Showcase.Documentation.Api;
using Maliev.ShadcnBlazor.Showcase.Documentation.Examples;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Maliev.ShadcnBlazor.Tests.Showcase;

public sealed class ComponentDossierTests : BunitContext
{
    private readonly IComponentDocumentationCatalog _documentation = new ComponentDocumentationCatalog();

    public ComponentDossierTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMalievShadcn();
    }

    [Fact]
    public void RegistryGivesCertifiedEntriesAndImplementedActionCandidatesOneRealPrimaryExample()
    {
        var registry = new ComponentExampleRegistry(_documentation);
        var complete = _documentation.All.Where(entry => entry.Status == ComponentDocumentationStatus.Complete).ToArray();
        var planned = _documentation.All.Where(entry => entry.Status == ComponentDocumentationStatus.Planned).ToArray();

        Assert.NotEmpty(complete);
        foreach (var entry in complete)
        {
            var example = Assert.Single(registry.GetBySlug(entry.Slug));
            Assert.Equal($"{entry.Slug}-primary", example.Id);
            Assert.False(string.IsNullOrWhiteSpace(example.RazorSource));
            Assert.Contains(entry.PrimaryType!.Split('`')[0], example.RazorSource, StringComparison.Ordinal);
            Assert.NotEmpty(example.StateTags);

            var preview = Render(example.Preview);
            Assert.NotEmpty(preview.FindAll("[data-slot]"));
        }

        var actionSlugs = new HashSet<string>(["button", "button-group", "checkbox", "radio-group", "slider", "switch", "toggle", "toggle-group"], StringComparer.Ordinal);
        Assert.All(actionSlugs, slug => Assert.Equal(ComponentDocumentationStatus.Complete, _documentation.FindBySlug(slug)!.Status));
        Assert.DoesNotContain(planned, entry => actionSlugs.Contains(entry.Slug));
        Assert.All(planned.Where(entry => entry.PrimaryType is null), entry => Assert.Empty(registry.GetBySlug(entry.Slug)));
    }

    [Fact]
    public void EveryCatalogPreviewRendersItsPrimaryRclComponent()
    {
        var registry = new ComponentExampleRegistry(_documentation);
        var expectedSlots = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // These components expose a semantic host slot rather than repeating the catalog slug.
            ["resizable"] = "resizable-group",
            ["toast"] = "toast-viewport"
        };

        foreach (var entry in _documentation.All.Where(entry => entry.Status == ComponentDocumentationStatus.Complete))
        {
            var example = Assert.Single(registry.GetBySlug(entry.Slug));
            var markup = Render(example.Preview).Markup;
            var expectedSlot = expectedSlots.GetValueOrDefault(entry.Slug, entry.Slug);

            Assert.Contains(
                $"data-slot=\"{expectedSlot}\"",
                markup,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ExampleIdsAreUniqueAcrossTheAuthoritativeCatalog()
    {
        var registry = new ComponentExampleRegistry(_documentation);
        var ids = _documentation.All.SelectMany(entry => registry.GetBySlug(entry.Slug)).Select(example => example.Id).ToArray();

        Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ActionSelectionFixturesOnlyClaimStatesRenderedByTheirDeterministicPreview()
    {
        var examplesType = typeof(ComponentExampleRegistry).Assembly.GetType(
            "Maliev.ShadcnBlazor.Showcase.Documentation.Examples.ActionSelectionExamples",
            throwOnError: true)!;
        var create = examplesType.GetMethod("Create", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!;
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["button"] = ["variants", "sizes", "disabled"],
            ["button-group"] = ["horizontal", "vertical", "separator", "nested", "text"],
            ["checkbox"] = ["unchecked", "checked", "indeterminate", "disabled", "read-only", "invalid", "form"],
            ["radio-group"] = ["selected", "unselected", "disabled-item", "horizontal", "vertical", "roving-focus", "read-only", "invalid", "form"],
            ["slider"] = ["single", "range", "multiple", "horizontal", "vertical", "keyboard", "pointer", "disabled", "read-only", "invalid", "form"],
            ["switch"] = ["checked", "unchecked", "default", "sm", "disabled", "read-only", "invalid", "form"],
            ["toggle"] = ["on", "off", "default", "outline", "sm", "lg", "disabled", "invalid"],
            ["toggle-group"] = ["single", "multiple", "spacing", "connected", "horizontal", "vertical", "roving-focus", "disabled-item", "outline", "sizes"]
        };

        foreach (var (slug, states) in expected)
        {
            var example = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<ComponentExampleDefinition>>(create.Invoke(null, [slug])));
            Assert.Equal(states, example.StateTags);
            Assert.NotEmpty(Render(example.Preview).FindAll("[data-slot]"));
        }
    }

    [Fact]
    public void ApiCatalogCoversEveryTypeAndPublicParameterInTheApprovedSnapshot()
    {
        var descriptors = new ComponentApiCatalog().All.ToDictionary(descriptor => descriptor.FullTypeName, StringComparer.Ordinal);
        string? currentType = null;

        foreach (var line in File.ReadLines(FindSnapshot()))
        {
            if (line.StartsWith("type ", StringComparison.Ordinal))
            {
                currentType = line[5..];
                if (!currentType.StartsWith("Maliev.ShadcnBlazor.Components.Actions.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Content.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Direction.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Forms.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Layout.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Primitives.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Selection.", StringComparison.Ordinal) &&
                    !currentType.StartsWith("Maliev.ShadcnBlazor.Components.Typography.", StringComparison.Ordinal))
                {
                    currentType = null;
                    continue;
                }

                Assert.True(descriptors.ContainsKey(currentType), $"Missing API descriptor for {currentType}.");
                continue;
            }

            if (!line.StartsWith("  ", StringComparison.Ordinal))
                continue;

            if (currentType is null)
                continue;

            var declaration = line.Trim().Replace(" [CaptureUnmatchedValues]", string.Empty, StringComparison.Ordinal);
            var separator = declaration.LastIndexOf(' ');
            var expectedType = declaration[..separator];
            var expectedName = declaration[(separator + 1)..];
            var parameter = Assert.Single(descriptors[currentType].Parameters, candidate => candidate.Name == expectedName);
            Assert.Equal(expectedType, parameter.FriendlyType);
        }
    }

    [Fact]
    public void ApiMetadataAddsDescriptionsWithoutHidingReflectedParameters()
    {
        var catalog = new ComponentApiCatalog();
        var assembly = typeof(ShadcnComponentBase).Assembly;

        foreach (var descriptor in catalog.All)
        {
            var type = assembly.GetType(descriptor.FullTypeName);
            Assert.NotNull(type);
            var expected = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(property => property.GetCustomAttribute<ParameterAttribute>() is not null || type.IsValueType || type.Name == "ShadcnSliderThumbAttributes")
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal)
                .ToArray();
            var actual = descriptor.Parameters.Select(parameter => parameter.Name).Order(StringComparer.Ordinal).ToArray();

            Assert.Equal(expected, actual);
            Assert.All(descriptor.Parameters, parameter => Assert.False(string.IsNullOrWhiteSpace(parameter.Description)));
        }

        var ratio = Assert.Single(
            catalog.All.Single(descriptor => descriptor.FullTypeName.EndsWith(".ShadcnAspectRatio", StringComparison.Ordinal)).Parameters,
            parameter => parameter.Name == "Ratio");
        Assert.True(ratio.Required);
        Assert.Equal("0", ratio.DefaultValue);
        Assert.Contains("positive", ratio.Constraints, StringComparison.OrdinalIgnoreCase);

        var checkbox = catalog.All.Single(descriptor => descriptor.FullTypeName.EndsWith(".ShadcnCheckbox", StringComparison.Ordinal));
        Assert.Equal("ValueChanged", checkbox.Parameters.Single(parameter => parameter.Name == "Value").BindingPair);
        Assert.Equal("Value", checkbox.Parameters.Single(parameter => parameter.Name == "ValueChanged").BindingPair);
    }

    [Fact]
    public void EditableExampleControlReRendersTheRealRclPreview()
    {
        var example = Assert.Single(new ComponentExampleRegistry(_documentation).GetBySlug("aspect-ratio"));
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        cut.Find("[data-testid='control-aspect-ratio']").Change("1");

        cut.WaitForAssertion(() => Assert.Contains("aspect-ratio: 1", cut.Find("[data-slot='aspect-ratio']").GetAttribute("style"), StringComparison.Ordinal));
    }

    [Fact]
    public void AttachmentPreviewKeepsLifecycleStateInTheDemoAndExposesOnlyMeaningfulControls()
    {
        var cut = RenderPreview("attachment");

        Assert.Empty(cut.FindAll("[data-testid='control-attachment-state']"));
        Assert.Equal(["Vertical", "Image media"], cut.FindAll(".component-preview__control").Select(control => control.TextContent.Trim()));
        Assert.Equal("uploading", cut.Find(".showcase-attachment-file").GetAttribute("data-state"));
    }

    [Fact]
    public void CodeExampleUsesAnAccessibleIconCopyAction()
    {
        var cut = Render<ComponentCodeExample>(parameters => parameters
            .Add(component => component.Title, "Keyboard shortcut")
            .Add(component => component.Source, "<ShadcnKbd>Ctrl</ShadcnKbd>"));

        var copy = cut.Find("[data-testid='copy-source']");
        Assert.Equal("Copy source", copy.GetAttribute("aria-label"));
        Assert.NotNull(copy.QuerySelector("svg"));
        Assert.Contains("Copy source", copy.TextContent, StringComparison.Ordinal);
        Assert.NotNull(cut.Find(".component-code__surface [data-testid='copy-source']"));
        Assert.NotEmpty(cut.FindAll(".component-code__surface .code-token-tag"));
    }

    [Fact]
    public void CodeExampleShowsCopiedStateAfterTheClipboardWriteSucceeds()
    {
        const string source = "<ShadcnKbd>Ctrl</ShadcnKbd>";
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-code-block.js");
        module.SetupVoid("copyText", source).SetVoidResult();
        var cut = Render<ComponentCodeExample>(parameters => parameters
            .Add(component => component.Title, "Keyboard shortcut")
            .Add(component => component.Source, source));

        cut.Find("[data-testid='copy-source']").Click();

        cut.WaitForAssertion(() =>
        {
            var copy = cut.Find("[data-testid='copy-source']");
            Assert.Equal("true", copy.GetAttribute("data-copied"));
            Assert.Equal("Copied", copy.GetAttribute("aria-label"));
            Assert.Contains("Copied", copy.TextContent, StringComparison.Ordinal);
            Assert.NotNull(copy.QuerySelector("svg"));
        });
    }

    [Fact]
    public void DirectionControlRendersInheritedAndExplicitDirections()
    {
        var cut = RenderPreview("direction");

        Assert.Equal(["Inherited", "LeftToRight", "RightToLeft"], OptionValues(cut, "control-direction"));
        Assert.Equal("rtl", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));

        cut.Find("[data-testid='control-direction']").Change("LeftToRight");
        Assert.Equal("ltr", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));

        cut.Find("[data-testid='control-direction']").Change("RightToLeft");
        Assert.Equal("rtl", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));

        cut.Find("[data-testid='control-direction']").Change("Inherited");
        Assert.Equal("rtl", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));
    }

    [Fact]
    public void LabelDisabledControlChangesTheAssociatedNativeInput()
    {
        var cut = RenderPreview("label");
        var input = cut.Find("#dossier-label-input");

        Assert.Equal("dossier-label-input", cut.Find("[data-slot='label']").GetAttribute("for"));
        Assert.False(input.HasAttribute("disabled"));

        cut.Find("[data-testid='control-label-disabled']").Change(true);

        Assert.True(cut.Find("#dossier-label-input").HasAttribute("disabled"));
    }

    [Fact]
    public void FieldControlsPropagateStateAndEveryGroupedVariantToRealDom()
    {
        var cut = RenderPreview("field");

        Assert.Equal(["Vertical", "Horizontal", "Responsive"], OptionValues(cut, "control-field-orientation"));
        Assert.Equal(["Legend", "Label"], OptionValues(cut, "control-field-legend-variant"));
        var input = cut.Find("#dossier-field-input");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("dossier-field-help dossier-field-error", input.GetAttribute("aria-describedby"));
        Assert.Equal("dossier-field-help", cut.Find("[data-slot='field-description']").GetAttribute("id"));
        Assert.Equal("dossier-field-error", cut.Find("[data-slot='field-error']").GetAttribute("id"));
        Assert.False(input.HasAttribute("disabled"));

        cut.Find("[data-testid='control-field-invalid']").Change(false);
        input = cut.Find("#dossier-field-input");
        Assert.False(input.HasAttribute("aria-invalid"));
        Assert.Equal("dossier-field-help", input.GetAttribute("aria-describedby"));
        Assert.Empty(cut.FindAll("#dossier-field-error"));

        cut.Find("[data-testid='control-field-disabled']").Change(true);
        Assert.True(cut.Find("#dossier-field-input").HasAttribute("disabled"));
        Assert.True(cut.Find("[data-slot='field-set']").HasAttribute("disabled"));
        Assert.Equal("true", cut.Find("[data-slot='field']").GetAttribute("data-disabled"));

        cut.Find("[data-testid='control-field-orientation']").Change("Horizontal");
        Assert.Equal("horizontal", cut.Find("[data-slot='field']").GetAttribute("data-orientation"));

        cut.Find("[data-testid='control-field-legend-variant']").Change("Label");
        Assert.Equal("label", cut.Find("[data-slot='field-legend']").GetAttribute("data-variant"));
    }

    [Fact]
    public void ItemControlsCoverRootVariantsSizesMediaVariantsAndLinkOutput()
    {
        var cut = RenderPreview("item");

        Assert.Equal(["Default", "Outline", "Muted"], OptionValues(cut, "control-item-variant"));
        Assert.Equal(["Default", "Small"], OptionValues(cut, "control-item-size"));
        Assert.Equal(["Default", "Icon", "Image"], OptionValues(cut, "control-item-media-variant"));

        cut.Find("[data-testid='control-item-variant']").Change("Muted");
        cut.Find("[data-testid='control-item-size']").Change("Small");
        cut.Find("[data-testid='control-item-media-variant']").Change("Image");
        cut.Find("[data-testid='control-item-link']").Change(true);

        var item = cut.Find("a[data-slot='item']");
        Assert.Equal("muted", item.GetAttribute("data-variant"));
        Assert.Equal("sm", item.GetAttribute("data-size"));
        Assert.Equal("image", cut.Find("[data-slot='item-media']").GetAttribute("data-variant"));
    }

    [Fact]
    public void EmptyControlCoversMediaVariantsAndRendersARealAction()
    {
        var cut = RenderPreview("empty");

        Assert.Equal(["Default", "Icon"], OptionValues(cut, "control-empty-media-variant"));
        Assert.Equal(2, cut.FindAll("[data-slot='empty-content'] button[type='button']").Count);
        Assert.Equal("icon", cut.Find("[data-slot='empty-icon']").GetAttribute("data-variant"));

        cut.Find("[data-testid='control-empty-media-variant']").Change("Default");

        Assert.Equal("default", cut.Find("[data-slot='empty-icon']").GetAttribute("data-variant"));
    }

    [Fact]
    public void TypographyControlsCoverTypographyAndTypesetPublicVariants()
    {
        var cut = RenderPreview("typography");

        Assert.Equal(
            ["H1", "H2", "H3", "H4", "Paragraph", "Blockquote", "InlineCode", "Lead", "Large", "Small", "Muted", "UnorderedList", "OrderedList"],
            OptionValues(cut, "control-typography-variant"));
        Assert.Equal(["div", "article", "section"], OptionValues(cut, "control-typeset-tag"));
        Assert.Equal(["0.875rem", "1rem", "1.125rem"], OptionValues(cut, "control-typeset-size"));
        Assert.Equal(["1.4", "1.6", "1.8"], OptionValues(cut, "control-typeset-leading"));
        Assert.Equal(["0.75rem", "1rem", "1.5rem"], OptionValues(cut, "control-typeset-flow"));
        Assert.Equal(["32rem", "48rem", "none"], OptionValues(cut, "control-typeset-max-width"));

        cut.Find("[data-testid='control-typography-variant']").Change("H1");
        cut.Find("[data-testid='control-typeset-tag']").Change("article");
        cut.Find("[data-testid='control-typeset-size']").Change("1.125rem");
        cut.Find("[data-testid='control-typeset-leading']").Change("1.8");
        cut.Find("[data-testid='control-typeset-flow']").Change("1.5rem");
        cut.Find("[data-testid='control-typeset-max-width']").Change("48rem");

        Assert.Equal(2, cut.FindAll("article[data-slot='typeset'] h1[data-slot='typography']").Count);
        var style = cut.Find("article[data-slot='typeset']").GetAttribute("style");
        Assert.Contains("--shadcn-typeset-size: 1.125rem", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-leading: 1.8", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-flow: 1.5rem", style, StringComparison.Ordinal);
        Assert.Contains("max-width: 48rem", style, StringComparison.Ordinal);
    }

    [Fact]
    public void CopyFailureAnnouncesTheFallbackAndPreservesTheExactSource()
    {
        const string source = "<ShadcnKbd>Ctrl</ShadcnKbd>";
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-code-block.js");
        module.SetupVoid("copyText", source).SetException(new JSException("Clipboard denied."));
        var cut = Render<ComponentCodeExample>(parameters => parameters
            .Add(component => component.Title, "Keyboard shortcut")
            .Add(component => component.Source, source));

        cut.Find("[data-testid='copy-source']").Click();

        cut.WaitForAssertion(() => Assert.Equal("Copy failed. Select the source below and copy it manually.", cut.Find("[aria-live='polite']").TextContent.Trim()));
        var fallback = cut.Find("textarea[readonly]");
        Assert.Equal(source, fallback.GetAttribute("value") ?? fallback.TextContent);
    }

    [Fact]
    public void StatusEvidenceRendersEveryCompleteLedgerDimensionExactly()
    {
        AssertEvidence(
            "aspect-ratio",
            [true, true, true, true, true, true, false]);
    }

    [Fact]
    public void StatusEvidenceRendersEveryPlanSixLedgerDimensionExactly()
    {
        AssertEvidence(
            "accordion",
            [true, true, true, true, true, true, true]);
    }

    private void AssertEvidence(string slug, IReadOnlyList<bool> expected)
    {
        var entry = Assert.IsType<ComponentDocumentationEntry>(_documentation.FindBySlug(slug));
        var cut = Render<ComponentStatusEvidence>(parameters => parameters.Add(component => component.Entry, entry));

        var rows = cut.FindAll("[data-testid='evidence-row']");
        Assert.Equal(7, rows.Count);
        string[] expectedNames = ["api", "component-tests", "accessibility", "interaction", "computed-style", "visual", "integration"];
        var actualNames = rows.Select(row => row.GetAttribute("data-evidence")!).ToArray();
        Assert.Equal(expectedNames, actualNames);
        for (var index = 0; index < rows.Count; index++)
        {
            Assert.Equal(expected[index].ToString().ToLowerInvariant(), rows[index].GetAttribute("data-complete"));
            Assert.Contains(expected[index] ? "Certified" : "Not certified", rows[index].TextContent, StringComparison.Ordinal);
        }
    }

    private static string FindSnapshot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.ShadcnBlazor.slnx")))
            directory = directory.Parent;
        return Path.Combine(directory!.FullName, "tests", "Maliev.ShadcnBlazor.Tests", "Contracts", "public-api.txt");
    }

    private IRenderedComponent<ComponentPreview> RenderPreview(string slug)
    {
        var example = Assert.Single(new ComponentExampleRegistry(_documentation).GetBySlug(slug));
        return Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));
    }

    private static string[] OptionValues(IRenderedComponent<ComponentPreview> cut, string testId) =>
        cut.Find($"[data-testid='{testId}']").QuerySelectorAll("option").Select(option => option.GetAttribute("value")!).ToArray();
}
