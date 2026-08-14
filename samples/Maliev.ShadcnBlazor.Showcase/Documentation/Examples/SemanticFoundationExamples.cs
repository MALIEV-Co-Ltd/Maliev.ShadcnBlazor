using System.Globalization;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Direction;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Components.Typography;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class SemanticFoundationExamples
{
    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "direction" => [Direction()],
        "aspect-ratio" => [AspectRatio()],
        "typography" => [Typography()],
        "label" => [Label()],
        "field" => [Field()],
        "item" => [Item()],
        "kbd" => [Kbd()],
        "separator" => [Separator()],
        "empty" => [Empty()],
        _ => []
    };

    private static ComponentExampleDefinition Direction()
    {
        ShadcnDirection? direction = null;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnDirectionProvider>(0);
            builder.AddAttribute(1, nameof(ShadcnDirectionProvider.Direction), ShadcnDirection.RightToLeft);
            builder.AddAttribute(2, nameof(ShadcnDirectionProvider.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnDirectionProvider>(0);
                content.AddAttribute(1, nameof(ShadcnDirectionProvider.Direction), direction);
                content.AddAttribute(2, nameof(ShadcnDirectionProvider.AdditionalAttributes),
                    new Dictionary<string, object> { ["data-testid"] = "direction-example" });
                content.AddAttribute(3, nameof(ShadcnDirectionProvider.ChildContent), Text("مرحبا — direction preview"));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        };
        var control = new ComponentParameterControl(
            "direction",
            "Direction",
            ComponentParameterControlKind.Select,
            "Inherited",
            ["Inherited", "LeftToRight", "RightToLeft"],
            value => direction = value == "Inherited" ? null : Enum.Parse<ShadcnDirection>(value));
        return Example(
            "direction",
            "Nested reading direction",
            "Override direction for one component subtree.",
            "<ShadcnDirectionProvider Direction=\"ShadcnDirection.RightToLeft\">\n    <ShadcnDirectionProvider Direction=\"null\">\n        مرحبا — inherited RTL preview\n    </ShadcnDirectionProvider>\n</ShadcnDirectionProvider>",
            preview,
            [control],
            ["inherited", "ltr", "rtl"]);
    }

    private static ComponentExampleDefinition AspectRatio()
    {
        var ratio = 16d / 9d;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnAspectRatio>(0);
            builder.AddAttribute(1, nameof(ShadcnAspectRatio.Ratio), ratio);
            builder.AddAttribute(2, nameof(ShadcnAspectRatio.ChildContent), Text("16:9 media frame"));
            builder.CloseComponent();
        };
        var control = new ComponentParameterControl(
            "aspect-ratio",
            "Aspect ratio",
            ComponentParameterControlKind.Select,
            "1.7777777777777777",
            ["1.7777777777777777", "1", "0.5625"],
            value => ratio = double.Parse(value, CultureInfo.InvariantCulture));
        return Example(
            "aspect-ratio",
            "Responsive media frame",
            "Choose a landscape, square, or portrait ratio without measuring in JavaScript.",
            "<ShadcnAspectRatio Ratio=\"@(16d / 9d)\">\n    16:9 media frame\n</ShadcnAspectRatio>",
            preview,
            [control],
            ["16:9", "1:1", "9:16"]);
    }

    private static ComponentExampleDefinition Typography()
    {
        var variant = ShadcnTypographyVariant.H2;
        var tag = "div";
        var size = "1rem";
        var leading = "1.6";
        var flow = "1rem";
        var maxWidth = "48rem";
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnTypeset>(0);
            builder.AddAttribute(1, nameof(ShadcnTypeset.Tag), tag);
            builder.AddAttribute(2, nameof(ShadcnTypeset.Size), size);
            builder.AddAttribute(3, nameof(ShadcnTypeset.Leading), leading);
            builder.AddAttribute(4, nameof(ShadcnTypeset.Flow), flow);
            builder.AddAttribute(5, nameof(ShadcnTypeset.MaxWidth), maxWidth);
            builder.AddAttribute(6, nameof(ShadcnTypeset.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnTypography>(0);
                content.AddAttribute(1, nameof(ShadcnTypography.Variant), variant);
                content.AddAttribute(2, nameof(ShadcnTypography.ChildContent), Text("Build calm, capable interfaces"));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        };
        var options = Enum.GetNames<ShadcnTypographyVariant>();
        ComponentParameterControl[] controls =
        [
            new(
                "typography-variant",
                "Variant",
                ComponentParameterControlKind.Select,
                variant.ToString(),
                options,
                value => variant = Enum.Parse<ShadcnTypographyVariant>(value)),
            new(
                "typeset-tag",
                "Typeset tag",
                ComponentParameterControlKind.Select,
                tag,
                ["div", "article", "section"],
                value => tag = value),
            new(
                "typeset-size",
                "Typeset size",
                ComponentParameterControlKind.Select,
                size,
                ["0.875rem", "1rem", "1.125rem"],
                value => size = value),
            new(
                "typeset-leading",
                "Typeset leading",
                ComponentParameterControlKind.Select,
                leading,
                ["1.4", "1.6", "1.8"],
                value => leading = value),
            new(
                "typeset-flow",
                "Typeset flow",
                ComponentParameterControlKind.Select,
                flow,
                ["0.75rem", "1rem", "1.5rem"],
                value => flow = value),
            new(
                "typeset-max-width",
                "Typeset max width",
                ComponentParameterControlKind.Select,
                maxWidth,
                ["32rem", "48rem", "none"],
                value => maxWidth = value)
        ];
        return Example(
            "typography",
            "Semantic type scale",
            "Select a semantic text treatment while preserving the matching HTML element.",
            "<ShadcnTypeset Tag=\"article\" Size=\"1rem\" Leading=\"1.6\" Flow=\"1rem\" MaxWidth=\"48rem\">\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.H2\">\n        Build calm, capable interfaces\n    </ShadcnTypography>\n</ShadcnTypeset>",
            preview,
            controls,
            options.Select(name => name.ToLowerInvariant())
                .Concat(["typeset-div", "typeset-article", "typeset-section", "typeset-rhythm"])
                .ToArray());
    }

    private static ComponentExampleDefinition Label()
    {
        var disabled = false;
        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "div");
            builder.OpenComponent<ShadcnLabel>(1);
            builder.AddAttribute(2, nameof(ShadcnLabel.For), "dossier-label-input");
            builder.AddAttribute(3, nameof(ShadcnLabel.ChildContent), Text("Project name"));
            builder.CloseComponent();
            builder.OpenElement(4, "input");
            builder.AddAttribute(5, "id", "dossier-label-input");
            builder.AddAttribute(6, "disabled", disabled);
            builder.CloseElement();
            builder.CloseElement();
        };
        return Example(
            "label",
            "Associated form label",
            "Connect visible text to its form control with a stable identifier.",
            "<ShadcnLabel For=\"project-name\">Project name</ShadcnLabel>\n<input id=\"project-name\" disabled />",
            preview,
            [new ComponentParameterControl(
                "label-disabled",
                "Disabled",
                ComponentParameterControlKind.Toggle,
                "false",
                [],
                value => disabled = bool.Parse(value))],
            ["associated", "enabled", "disabled"]);
    }

    private static ComponentExampleDefinition Field()
    {
        var orientation = ShadcnFieldOrientation.Vertical;
        var legendVariant = ShadcnFieldLegendVariant.Legend;
        var invalid = true;
        var disabled = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnFieldSet>(0);
            builder.AddAttribute(1, nameof(ShadcnFieldSet.Disabled), disabled);
            builder.AddAttribute(2, nameof(ShadcnFieldSet.ChildContent), FieldSetContent(orientation, legendVariant, invalid, disabled));
            builder.CloseComponent();
        };
        ComponentParameterControl[] controls =
        [
            new(
                "field-orientation",
                "Orientation",
                ComponentParameterControlKind.Select,
                orientation.ToString(),
                Enum.GetNames<ShadcnFieldOrientation>(),
                value => orientation = Enum.Parse<ShadcnFieldOrientation>(value)),
            new(
                "field-legend-variant",
                "Legend variant",
                ComponentParameterControlKind.Select,
                legendVariant.ToString(),
                Enum.GetNames<ShadcnFieldLegendVariant>(),
                value => legendVariant = Enum.Parse<ShadcnFieldLegendVariant>(value)),
            new(
                "field-invalid",
                "Invalid",
                ComponentParameterControlKind.Toggle,
                "true",
                [],
                value => invalid = bool.Parse(value)),
            new(
                "field-disabled",
                "Disabled",
                ComponentParameterControlKind.Toggle,
                "false",
                [],
                value => disabled = bool.Parse(value))
        ];
        return Example(
            "field",
            "Validation composition",
            "Group a label, control, help text, and validation message with shared state.",
            "<ShadcnFieldSet Disabled=\"false\">\n    <ShadcnFieldLegend Variant=\"ShadcnFieldLegendVariant.Legend\">Contact</ShadcnFieldLegend>\n    <ShadcnField Invalid=\"true\" DescriptionId=\"email-help\" ErrorId=\"email-error\">\n        <ShadcnFieldLabel For=\"email\">Email</ShadcnFieldLabel>\n        <input id=\"email\" aria-invalid=\"true\" aria-describedby=\"email-help email-error\" />\n        <ShadcnFieldDescription>Email used for notifications.</ShadcnFieldDescription>\n        <ShadcnFieldError>Enter a valid address.</ShadcnFieldError>\n    </ShadcnField>\n</ShadcnFieldSet>",
            preview,
            controls,
            ["vertical", "horizontal", "responsive", "valid", "invalid", "enabled", "disabled", "legend", "label"]);
    }

    private static ComponentExampleDefinition Item()
    {
        var variant = ShadcnItemVariant.Outline;
        var size = ShadcnItemSize.Default;
        var mediaVariant = ShadcnItemMediaVariant.Icon;
        var link = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnItem>(0);
            builder.AddAttribute(1, nameof(ShadcnItem.Variant), variant);
            builder.AddAttribute(2, nameof(ShadcnItem.Size), size);
            builder.AddAttribute(3, nameof(ShadcnItem.Href), link ? "#item-example" : null);
            builder.AddAttribute(4, nameof(ShadcnItem.ChildContent), ItemContent(mediaVariant));
            builder.CloseComponent();
        };
        var options = Enum.GetNames<ShadcnItemVariant>();
        ComponentParameterControl[] controls =
        [
            new(
                "item-variant",
                "Variant",
                ComponentParameterControlKind.Select,
                variant.ToString(),
                options,
                value => variant = Enum.Parse<ShadcnItemVariant>(value)),
            new(
                "item-size",
                "Size",
                ComponentParameterControlKind.Select,
                size.ToString(),
                Enum.GetNames<ShadcnItemSize>(),
                value => size = Enum.Parse<ShadcnItemSize>(value)),
            new(
                "item-media-variant",
                "Media variant",
                ComponentParameterControlKind.Select,
                mediaVariant.ToString(),
                Enum.GetNames<ShadcnItemMediaVariant>(),
                value => mediaVariant = Enum.Parse<ShadcnItemMediaVariant>(value)),
            new(
                "item-link",
                "Link",
                ComponentParameterControlKind.Toggle,
                "false",
                [],
                value => link = bool.Parse(value))
        ];
        return Example(
            "item",
            "Structured content item",
            "Compose item title and description inside a selectable visual treatment.",
            "<ShadcnItem Variant=\"ShadcnItemVariant.Outline\">\n    <ShadcnItemMedia Variant=\"ShadcnItemMediaVariant.Icon\">PDF</ShadcnItemMedia>\n    <ShadcnItemContent>\n        <ShadcnItemTitle>Production drawing</ShadcnItemTitle>\n        <ShadcnItemDescription>Revision C</ShadcnItemDescription>\n    </ShadcnItemContent>\n</ShadcnItem>",
            preview,
            controls,
            ["default", "outline", "muted", "small", "link", "media-default", "media-icon", "media-image"]);
    }

    private static ComponentExampleDefinition Kbd()
    {
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnKbdGroup>(0);
            builder.AddAttribute(1, nameof(ShadcnKbdGroup.ChildContent), KeyboardKeys());
            builder.CloseComponent();
        };
        return Example(
            "kbd",
            "Keyboard shortcut",
            "Present a compact shortcut using a keyboard-key group.",
            "<ShadcnKbdGroup>\n    <ShadcnKbd>Ctrl</ShadcnKbd>\n    <span>+</span>\n    <ShadcnKbd>K</ShadcnKbd>\n</ShadcnKbdGroup>",
            preview,
            [],
            ["single-key", "grouped"]);
    }

    private static ComponentExampleDefinition Separator()
    {
        var orientation = ShadcnSeparatorOrientation.Horizontal;
        var decorative = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnSeparator>(0);
            builder.AddAttribute(1, nameof(ShadcnSeparator.Orientation), orientation);
            builder.AddAttribute(2, nameof(ShadcnSeparator.Decorative), decorative);
            builder.CloseComponent();
        };
        ComponentParameterControl[] controls =
        [
            new(
                "separator-orientation",
                "Orientation",
                ComponentParameterControlKind.Select,
                orientation.ToString(),
                Enum.GetNames<ShadcnSeparatorOrientation>(),
                value => orientation = Enum.Parse<ShadcnSeparatorOrientation>(value)),
            new(
                "separator-decorative",
                "Decorative",
                ComponentParameterControlKind.Toggle,
                "false",
                [],
                value => decorative = bool.Parse(value))
        ];
        return Example(
            "separator",
            "Semantic section separator",
            "Switch between a meaningful separator and a decorative visual rule.",
            "<ShadcnSeparator Decorative=\"false\" Orientation=\"ShadcnSeparatorOrientation.Horizontal\" />",
            preview,
            controls,
            ["horizontal", "vertical", "decorative", "semantic"]);
    }

    private static ComponentExampleDefinition Empty()
    {
        var mediaVariant = ShadcnEmptyMediaVariant.Icon;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<ShadcnEmpty>(0);
            builder.AddAttribute(1, nameof(ShadcnEmpty.ChildContent), EmptyContent(mediaVariant));
            builder.CloseComponent();
        };
        return Example(
            "empty",
            "Empty collection",
            "Explain why content is absent and offer a clear recovery action.",
            "<ShadcnEmpty>\n    <ShadcnEmptyHeader>\n        <ShadcnEmptyMedia Variant=\"ShadcnEmptyMediaVariant.Icon\">+</ShadcnEmptyMedia>\n        <ShadcnEmptyTitle>No projects yet</ShadcnEmptyTitle>\n        <ShadcnEmptyDescription>Create a project to begin.</ShadcnEmptyDescription>\n    </ShadcnEmptyHeader>\n    <ShadcnEmptyContent><button type=\"button\">Create project</button></ShadcnEmptyContent>\n</ShadcnEmpty>",
            preview,
            [new ComponentParameterControl(
                "empty-media-variant",
                "Media variant",
                ComponentParameterControlKind.Select,
                mediaVariant.ToString(),
                Enum.GetNames<ShadcnEmptyMediaVariant>(),
                value => mediaVariant = Enum.Parse<ShadcnEmptyMediaVariant>(value))],
            ["media-default", "media-icon", "description", "action"]);
    }

    private static ComponentExampleDefinition Example(
        string slug,
        string title,
        string description,
        string source,
        RenderFragment preview,
        IReadOnlyList<ComponentParameterControl> controls,
        IReadOnlyList<string> stateTags) =>
        new($"{slug}-primary", title, description, source, preview, controls, stateTags);

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);

    private static RenderFragment FieldSetContent(
        ShadcnFieldOrientation orientation,
        ShadcnFieldLegendVariant legendVariant,
        bool invalid,
        bool disabled) => builder =>
    {
        builder.OpenComponent<ShadcnFieldLegend>(0);
        builder.AddAttribute(1, nameof(ShadcnFieldLegend.Variant), legendVariant);
        builder.AddAttribute(2, nameof(ShadcnFieldLegend.ChildContent), Text("Contact"));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnField>(3);
        builder.AddAttribute(4, nameof(ShadcnField.Orientation), orientation);
        builder.AddAttribute(5, nameof(ShadcnField.Invalid), invalid);
        builder.AddAttribute(6, nameof(ShadcnField.Disabled), disabled);
        builder.AddAttribute(7, nameof(ShadcnField.DescriptionId), "dossier-field-help");
        builder.AddAttribute(8, nameof(ShadcnField.ErrorId), "dossier-field-error");
        builder.AddAttribute(9, nameof(ShadcnField.ChildContent), FieldContent(invalid, disabled));
        builder.CloseComponent();
    };

    private static RenderFragment FieldContent(bool invalid, bool disabled) => builder =>
    {
        builder.OpenComponent<ShadcnFieldLabel>(0);
        builder.AddAttribute(1, nameof(ShadcnFieldLabel.For), "dossier-field-input");
        builder.AddAttribute(2, nameof(ShadcnFieldLabel.ChildContent), Text("Email"));
        builder.CloseComponent();
        builder.OpenElement(3, "input");
        builder.AddAttribute(4, "id", "dossier-field-input");
        builder.AddAttribute(5, "disabled", disabled);
        builder.AddAttribute(6, "aria-invalid", invalid ? "true" : null);
        builder.AddAttribute(7, "aria-describedby", invalid ? "dossier-field-help dossier-field-error" : "dossier-field-help");
        builder.CloseElement();
        builder.OpenComponent<ShadcnFieldDescription>(8);
        builder.AddAttribute(9, nameof(ShadcnFieldDescription.ChildContent), Text("Email used for notifications."));
        builder.CloseComponent();
        if (invalid)
        {
            builder.OpenComponent<ShadcnFieldError>(10);
            builder.AddAttribute(11, nameof(ShadcnFieldError.ChildContent), Text("Enter a valid address."));
            builder.CloseComponent();
        }
    };

    private static RenderFragment ItemContent(ShadcnItemMediaVariant mediaVariant) => builder =>
    {
        builder.OpenComponent<ShadcnItemMedia>(0);
        builder.AddAttribute(1, nameof(ShadcnItemMedia.Variant), mediaVariant);
        builder.AddAttribute(2, nameof(ShadcnItemMedia.ChildContent), Text("PDF"));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnItemContent>(3);
        builder.AddAttribute(4, nameof(ShadcnItemContent.ChildContent), (RenderFragment)(builder2 =>
        {
            builder2.OpenComponent<ShadcnItemTitle>(0);
            builder2.AddAttribute(1, nameof(ShadcnItemTitle.ChildContent), Text("Production drawing"));
            builder2.CloseComponent();
            builder2.OpenComponent<ShadcnItemDescription>(2);
            builder2.AddAttribute(3, nameof(ShadcnItemDescription.ChildContent), Text("Revision C"));
            builder2.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static RenderFragment KeyboardKeys() => builder =>
    {
        builder.OpenComponent<ShadcnKbd>(0);
        builder.AddAttribute(1, nameof(ShadcnKbd.ChildContent), Text("Ctrl"));
        builder.CloseComponent();
        builder.AddContent(2, "+");
        builder.OpenComponent<ShadcnKbd>(3);
        builder.AddAttribute(4, nameof(ShadcnKbd.ChildContent), Text("K"));
        builder.CloseComponent();
    };

    private static RenderFragment EmptyContent(ShadcnEmptyMediaVariant mediaVariant) => builder =>
    {
        builder.OpenComponent<ShadcnEmptyHeader>(0);
        builder.AddAttribute(1, nameof(ShadcnEmptyHeader.ChildContent), (RenderFragment)(builder2 =>
        {
            builder2.OpenComponent<ShadcnEmptyMedia>(0);
            builder2.AddAttribute(1, nameof(ShadcnEmptyMedia.Variant), mediaVariant);
            builder2.AddAttribute(2, nameof(ShadcnEmptyMedia.ChildContent), Text("+"));
            builder2.CloseComponent();
            builder2.OpenComponent<ShadcnEmptyTitle>(3);
            builder2.AddAttribute(4, nameof(ShadcnEmptyTitle.ChildContent), Text("No projects yet"));
            builder2.CloseComponent();
            builder2.OpenComponent<ShadcnEmptyDescription>(5);
            builder2.AddAttribute(6, nameof(ShadcnEmptyDescription.ChildContent), Text("Create a project to begin."));
            builder2.CloseComponent();
        }));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnEmptyContent>(2);
        builder.AddAttribute(3, nameof(ShadcnEmptyContent.ChildContent), (RenderFragment)(content =>
        {
            content.OpenElement(0, "button");
            content.AddAttribute(1, "type", "button");
            content.AddContent(2, "Create project");
            content.CloseElement();
        }));
        builder.CloseComponent();
    };
}
