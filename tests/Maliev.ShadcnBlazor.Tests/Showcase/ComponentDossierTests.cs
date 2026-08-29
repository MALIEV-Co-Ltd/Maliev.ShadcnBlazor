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
            var examples = registry.GetBySlug(entry.Slug);
            if (entry.Slug is "bento-grid" or "visual-style-scope") Assert.Equal(3, examples.Count);
            else Assert.Equal($"{entry.Slug}-primary", Assert.Single(examples).Id);
            foreach (var example in examples)
            {
                Assert.False(string.IsNullOrWhiteSpace(example.RazorSource));
                Assert.Contains(entry.PrimaryType!.Split('`')[0], example.RazorSource, StringComparison.Ordinal);
                Assert.NotEmpty(example.StateTags);

                var preview = Render(example.Preview);
                Assert.NotEmpty(preview.FindAll("[data-slot]"));
            }
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
            var expectedSlot = expectedSlots.GetValueOrDefault(entry.Slug, entry.Slug);
            foreach (var example in registry.GetBySlug(entry.Slug))
            {
                var markup = Render(example.Preview).Markup;
                Assert.Contains(
                    $"data-slot=\"{expectedSlot}\"",
                    markup,
                    StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void CheckboxDossierShowsEveryMeaningfulStateWithoutConfigurationControls()
    {
        var example = Assert.Single(new ComponentExampleRegistry(_documentation).GetBySlug("checkbox"));
        var preview = Render(example.Preview);
        var checkboxes = preview.FindAll("input[data-slot='checkbox']");

        Assert.Equal("Notification preferences", example.Title);
        Assert.Empty(example.Controls);
        Assert.Equal(6, checkboxes.Count);
        Assert.Contains(checkboxes, checkbox => checkbox.GetAttribute("aria-checked") == "true");
        Assert.Contains(checkboxes, checkbox => checkbox.GetAttribute("aria-checked") == "false");
        Assert.Contains(checkboxes, checkbox => checkbox.GetAttribute("aria-checked") == "mixed");
        Assert.Contains(checkboxes, checkbox => checkbox.HasAttribute("disabled"));
        Assert.Contains(checkboxes, checkbox => checkbox.GetAttribute("aria-readonly") == "true");
        Assert.Contains(checkboxes, checkbox => checkbox.GetAttribute("aria-invalid") == "true");
        Assert.Contains("@bind-Value=\"AcceptTerms\"", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Required before production files can be released.", example.RazorSource, StringComparison.Ordinal);
        Assert.Contains("Name=\"legacy-alerts\" Disabled=\"true\"", example.RazorSource, StringComparison.Ordinal);
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
            ["button"] = ["variants", "sizes", "icons", "link", "disabled", "programmatic focus"],
            ["button-group"] = ["horizontal", "vertical", "separator", "nested", "text"],
            ["checkbox"] = ["unchecked", "checked", "indeterminate", "disabled", "read-only", "invalid", "form"],
            ["radio-group"] = ["selected", "unselected", "disabled-item", "horizontal", "vertical", "roving-focus", "read-only", "invalid", "form"],
            ["slider"] = ["single", "range", "multiple", "horizontal", "vertical", "keyboard", "pointer", "disabled", "read-only", "invalid", "form"],
            ["switch"] = ["checked", "unchecked", "default", "sm", "disabled", "read-only", "invalid", "form"],
            ["toggle"] = ["on", "off", "default", "outline", "sm", "lg", "disabled", "invalid"],
            ["toggle-group"] = ["single", "multiple", "spacing", "connected", "horizontal", "vertical", "roving-focus", "disabled-item", "outline", "sizes", "disabled", "invalid"]
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

        cut.SelectControl("aspect-ratio", "1:1");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("aspect-ratio: 1", cut.Find("[data-slot='aspect-ratio']").GetAttribute("style"), StringComparison.Ordinal);
            Assert.Contains("showcase-aspect-ratio-demo--1-1", cut.Find(".showcase-aspect-ratio-demo").ClassList);
            Assert.Equal("Engineering workspace reference", cut.Find(".showcase-aspect-ratio-media img").GetAttribute("alt"));
        });
    }

    [Fact]
    public void KbdPreviewShowsContextualOneTwoAndThreeKeyShortcutsAndSynchronizesPlatform()
    {
        var example = Assert.Single(new ComponentExampleRegistry(_documentation).GetBySlug("kbd"));
        var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));

        Assert.Single(cut.FindAll("[data-slot='card']"));
        Assert.Equal(3, cut.FindAll("[data-slot='item']").Count);
        Assert.Equal(
            [1, 2, 3],
            cut.FindAll("[data-slot='kbd-group']")
                .Select(group => group.QuerySelectorAll("[data-slot='kbd']").Length));

        cut.SelectControl("kbd-platform", "macOS");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("⌘", cut.Find(".component-preview__canvas").TextContent, StringComparison.Ordinal);
            Assert.DoesNotContain("Ctrl", cut.Find(".component-preview__canvas").TextContent, StringComparison.Ordinal);
            Assert.Contains("<ShadcnKbd>⌘</ShadcnKbd>", example.RazorSource, StringComparison.Ordinal);
            Assert.Equal("macOS", example.Controls.Single(control => control.Id == "kbd-platform").Value);
        });
    }

    [Fact]
    public void CatalogSelectControlsUseThePackageSelectAndSynchronizePreviewAndSource()
    {
        var registry = new ComponentExampleRegistry(_documentation);
        var examples = _documentation.All
            .Where(entry => entry.Status == ComponentDocumentationStatus.Complete && entry.Slug != "native-select")
            .SelectMany(entry => registry.GetBySlug(entry.Slug))
            .Where(example => example.Controls.Any(control => control.Kind == ComponentParameterControlKind.Select))
            .ToArray();

        Assert.NotEmpty(examples);
        foreach (var example in examples)
        {
            var cut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));
            var selectControls = example.Controls.Where(control => control.Kind == ComponentParameterControlKind.Select).ToArray();

            Assert.Empty(cut.FindAll(".component-preview__controls select"));
            Assert.Equal(selectControls.Length, cut.FindAll(".component-preview__controls [data-slot='select']").Count);
            Assert.All(selectControls, control =>
            {
                var trigger = cut.Find($"[data-testid='control-{control.Id}']");
                Assert.Equal("select-trigger", trigger.GetAttribute("data-slot"));
                Assert.Equal(control.Label, trigger.GetAttribute("aria-label"));
            });
        }

        var card = registry.GetBySlug("card").Single();
        var cardCut = Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, card));
        cardCut.Find("[data-testid='control-card-size']").Click();
        cardCut.Find("[role='option'][data-value='Small']").Click();

        Assert.Equal("sm", cardCut.Find("[data-slot='card']").GetAttribute("data-size"));
        Assert.Contains("Size=\"ShadcnCardSize.Small\"", card.RazorSource, StringComparison.Ordinal);
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
    public void CodeExampleRendersCompleteSourceInAClosedNativeDisclosureWhenCollapsible()
    {
        Assert.NotNull(typeof(ComponentCodeExample).GetProperty("Collapsible"));

        var cut = Render(builder =>
        {
            builder.OpenComponent<ComponentCodeExample>(0);
            builder.AddAttribute(1, "Title", "Keyboard shortcut");
            builder.AddAttribute(2, "Source", "<ShadcnKbd>Ctrl</ShadcnKbd>");
            builder.AddAttribute(3, "Collapsible", true);
            builder.AddAttribute(4, "Summary", "View complete source");
            builder.AddAttribute(5, "TestId", "example-source");
            builder.CloseComponent();
        });

        var disclosure = cut.Find("details[data-testid='example-source']");
        Assert.False(disclosure.HasAttribute("open"));
        Assert.Equal("View complete source", disclosure.QuerySelector("summary")?.TextContent.Trim());
        Assert.NotNull(disclosure.QuerySelector(".component-code__surface [data-testid='copy-source']"));
        Assert.Empty(cut.FindAll("section.component-code"));
    }

    [Fact]
    public void CodeExampleRemainsAnAlwaysVisibleSectionWhenNotCollapsible()
    {
        var cut = Render<ComponentCodeExample>(parameters => parameters
            .Add(component => component.Title, "Keyboard shortcut")
            .Add(component => component.Heading, "Razor example")
            .Add(component => component.Source, "<ShadcnKbd>Ctrl</ShadcnKbd>"));

        Assert.NotNull(cut.Find("section.component-code .component-code__surface"));
        Assert.Equal("Razor example", cut.Find("section.component-code h2").TextContent.Trim());
        Assert.Empty(cut.FindAll("details"));
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

        Assert.Equal(["Inherited (RTL)", "Left to right (LTR)", "Right to left (RTL)"], OptionValues(cut, "control-direction"));
        Assert.Equal("rtl", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));

        cut.SelectControl("direction", "Left to right (LTR)");
        Assert.Equal("ltr", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));

        cut.SelectControl("direction", "Right to left (RTL)");
        Assert.Equal("rtl", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));

        cut.SelectControl("direction", "Inherited (RTL)");
        Assert.Equal("rtl", cut.Find("[data-testid='direction-example']").GetAttribute("dir"));
    }

    [Fact]
    public void LabelDossierUsesThePackageInputAndKeepsItsStateSynchronized()
    {
        var cut = RenderPreview("label");
        var input = cut.Find("#dossier-label-input");

        Assert.Equal("dossier-label-input", cut.Find("[data-slot='label']").GetAttribute("for"));
        Assert.Equal("input", input.GetAttribute("data-slot"));
        Assert.Contains("shadcn-input", input.ClassList);
        Assert.Equal("Project name", input.GetAttribute("aria-label"));
        Assert.Equal("dossier-label-help", input.GetAttribute("aria-describedby"));
        Assert.Equal("false", cut.Find("[data-testid='label-dossier']").GetAttribute("data-disabled"));
        Assert.False(input.HasAttribute("disabled"));

        input.Input("Fixture inspection · Revision C");
        Assert.Contains("Fixture inspection · Revision C", cut.Find("[data-testid='label-project-preview']").TextContent, StringComparison.Ordinal);

        cut.Find("[data-testid='control-label-disabled']").Change(true);

        input = cut.Find("#dossier-label-input");
        Assert.True(input.HasAttribute("disabled"));
        Assert.Equal("true", cut.Find("[data-testid='label-dossier']").GetAttribute("data-disabled"));
    }

    [Fact]
    public void FieldControlsPropagateStateAndEveryGroupedVariantToRealDom()
    {
        var cut = RenderPreview("field");

        Assert.Equal(["Vertical", "Horizontal", "Responsive"], OptionValues(cut, "control-field-orientation"));
        Assert.Equal(["Legend", "Label"], OptionValues(cut, "control-field-legend-variant"));
        var input = cut.Find("#field-card-number");
        Assert.False(input.HasAttribute("aria-invalid"));
        Assert.Equal("field-card-number-help", input.GetAttribute("aria-describedby"));
        Assert.Equal("field-card-number-help", cut.Find("#field-card-number-help").GetAttribute("id"));
        Assert.Empty(cut.FindAll("#field-card-number-error"));

        cut.Find("[data-testid='control-field-invalid']").Change(true);
        input = cut.Find("#field-card-number");
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("field-card-number-help field-card-number-error", input.GetAttribute("aria-describedby"));
        Assert.Equal("alert", cut.Find("#field-card-number-error").GetAttribute("role"));

        cut.Find("[data-testid='control-field-disabled']").Change(true);
        Assert.True(cut.Find("[data-slot='field-set']").HasAttribute("disabled"));
        Assert.Equal("true", cut.Find("#field-card-number").ParentElement?.GetAttribute("data-disabled"));

        cut.SelectControl("field-orientation", "Horizontal");
        Assert.Equal("horizontal", cut.Find("[data-slot='field']").GetAttribute("data-orientation"));

        cut.SelectControl("field-legend-variant", "Label");
        Assert.Equal("label", cut.Find("[data-slot='field-legend']").GetAttribute("data-variant"));
    }

    [Fact]
    public void FieldAndActionExamplesUseCompactRealisticCompositions()
    {
        var field = RenderPreview("field");
        Assert.Equal(2, field.FindAll(".showcase-field-dossier__card-row > [data-slot='field']").Count);
        Assert.Contains("class=\"payment-card-row\"", GetExample("field").RazorSource, StringComparison.Ordinal);

        var slider = RenderPreview("slider");
        Assert.All(slider.FindAll("input[data-slot='slider-thumb']"), thumb => Assert.Equal("1", thumb.GetAttribute("step")));
        Assert.Contains("Step=\"1\"", GetExample("slider").RazorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Step=\"5\"", GetExample("slider").RazorSource, StringComparison.Ordinal);

        var toggle = RenderPreview("toggle");
        var tools = toggle.FindAll("[role='toolbar'] button[data-slot='toggle']");
        Assert.Equal(3, tools.Count);
        Assert.Equal(["Toggle bold emphasis", "Toggle italic emphasis", "Toggle underline emphasis"], tools.Select(tool => tool.GetAttribute("aria-label")));
        Assert.All(tools, tool => Assert.NotNull(tool.QuerySelector("[data-slot='icon']")));
        Assert.Contains("LucideIconCatalog.Instance.Get", GetExample("toggle").RazorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void SinglePreviewControlAndThemeProfileUseIntrinsicStartAlignedLayout()
    {
        var css = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "samples", "Maliev.ShadcnBlazor.Showcase", "wwwroot", "css", "showcase.css"));
        Assert.Contains(".component-preview__controls:has(> .component-preview__control:only-child)", css, StringComparison.Ordinal);
        Assert.Contains(".theme-runway-profile { display: flex; min-inline-size: 0; align-items: center; justify-content: flex-start", css, StringComparison.Ordinal);
    }

    [Fact]
    public void AvatarGroupOverflowExpandsAndCollapsesWithTheCountTrigger()
    {
        var source = GetExample("avatar");
        source.Controls.Single(control => control.Id == "avatar-group").Apply("true");
        Assert.Contains("Expanded=\"groupExpanded\"", source.RazorSource, StringComparison.Ordinal);
        Assert.Contains("OnClick=\"_ => groupExpanded = !groupExpanded\"", source.RazorSource, StringComparison.Ordinal);

        var avatar = RenderPreview("avatar");
        avatar.Find("[data-testid='control-avatar-group']").Change(true);
        var group = avatar.Find("[data-testid='avatar-group-preview']");
        Assert.Equal("false", group.GetAttribute("data-expanded"));
        Assert.True(group.QuerySelector(".showcase-avatar-group-preview__overflow")?.HasAttribute("aria-hidden"));

        group.QuerySelector("button[data-slot='avatar-group-count']")!.Click();

        avatar.WaitForAssertion(() =>
        {
            group = avatar.Find("[data-testid='avatar-group-preview']");
            Assert.Equal("true", group.GetAttribute("data-expanded"));
            Assert.False(group.QuerySelector(".showcase-avatar-group-preview__overflow")?.HasAttribute("aria-hidden"));
        });
    }

    [Fact]
    public void ItemControlsCoverRootVariantsSizesMediaVariantsAndLinkOutput()
    {
        var cut = RenderPreview("item");

        Assert.NotNull(cut.Find(".showcase-item-dossier"));
        Assert.Equal(3, cut.FindAll("[data-slot='item-group'] > [role='listitem']").Count);
        Assert.Equal(3, cut.FindAll("[data-slot='item-media'] svg[aria-hidden='true']").Count);
        Assert.Empty(cut.FindAll("[data-slot='item-media'] img"));
        Assert.Equal(3, cut.FindAll("[data-slot='item-actions'] [data-slot='badge']").Count);
        Assert.DoesNotContain(">PDF<", cut.Markup, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(["Default", "Outline", "Muted"], OptionValues(cut, "control-item-variant"));
        Assert.Equal(["Default", "Small"], OptionValues(cut, "control-item-size"));
        Assert.Equal(["Default", "Icon", "Image"], OptionValues(cut, "control-item-media-variant"));

        cut.SelectControl("item-variant", "Muted");
        cut.SelectControl("item-size", "Small");
        cut.SelectControl("item-media-variant", "Image");
        cut.Find("[data-testid='control-item-link']").Change(true);

        var item = cut.Find("a[data-slot='item']");
        Assert.Equal("muted", item.GetAttribute("data-variant"));
        Assert.Equal("sm", item.GetAttribute("data-size"));
        Assert.Equal(3, cut.FindAll("[data-slot='item-media'][data-variant='image'] img[alt]").Count);
    }

    [Fact]
    public void EmptyControlCoversMediaVariantsAndRendersARealAction()
    {
        var cut = RenderPreview("empty");

        Assert.Equal(["Default", "Icon"], OptionValues(cut, "control-empty-media-variant"));
        Assert.Equal(2, cut.FindAll("[data-slot='empty-content'] button[type='button']").Count);
        Assert.Equal("icon", cut.Find("[data-slot='empty-icon']").GetAttribute("data-variant"));

        cut.SelectControl("empty-media-variant", "Default");

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

        cut.SelectControl("typography-variant", "H1");
        cut.SelectControl("typeset-tag", "article");
        cut.SelectControl("typeset-size", "1.125rem");
        cut.SelectControl("typeset-leading", "1.8");
        cut.SelectControl("typeset-flow", "1.5rem");
        cut.SelectControl("typeset-max-width", "48rem");

        Assert.Equal(2, cut.FindAll("article[data-slot='typeset'] h1[data-slot='typography']").Count);
        var style = cut.Find("article[data-slot='typeset']").GetAttribute("style");
        Assert.Contains("--shadcn-typeset-size: 1.125rem", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-leading: 1.8", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-flow: 1.5rem", style, StringComparison.Ordinal);
        Assert.Contains("--shadcn-typeset-measure: 48rem", style, StringComparison.Ordinal);
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

    [Fact]
    public void StatusEvidenceNamesTheIntegrationDimensionWithoutDuplicatingTheLabel()
    {
        var entry = Assert.IsType<ComponentDocumentationEntry>(_documentation.FindBySlug("accordion"));
        var cut = Render<ComponentStatusEvidence>(parameters => parameters.Add(component => component.Entry, entry));

        var integration = cut.Find("[data-evidence='integration']");

        Assert.Equal("Integration", integration.QuerySelector("th")?.TextContent.Trim());
    }

    [Fact]
    public void StatusEvidenceExplainsWhatCertificationMeansInThisRepository()
    {
        var entry = Assert.IsType<ComponentDocumentationEntry>(_documentation.FindBySlug("accordion"));
        var cut = Render<ComponentStatusEvidence>(parameters => parameters.Add(component => component.Entry, entry));

        Assert.Equal(
            "Certification is the reviewed evidence status recorded by this repository for each area below.",
            cut.Find("[data-testid='certification-explanation']").TextContent.Trim());
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
        var example = GetExample(slug);
        return Render<ComponentPreview>(parameters => parameters.Add(component => component.Example, example));
    }

    private ComponentExampleDefinition GetExample(string slug) =>
        Assert.Single(new ComponentExampleRegistry(_documentation).GetBySlug(slug));

    private static string[] OptionValues(IRenderedComponent<ComponentPreview> cut, string testId) =>
        cut.SelectControlOptions(testId["control-".Length..]);
}
