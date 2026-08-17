using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Direction;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Components.Typography;
using Maliev.ShadcnBlazor.Theming;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;

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
            var isRightToLeft = direction is null or ShadcnDirection.RightToLeft;
            builder.OpenComponent<ShadcnDirectionProvider>(0);
            builder.AddAttribute(1, nameof(ShadcnDirectionProvider.Direction), ShadcnDirection.RightToLeft);
            builder.AddAttribute(2, nameof(ShadcnDirectionProvider.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnDirectionProvider>(0);
                content.AddAttribute(1, nameof(ShadcnDirectionProvider.Direction), direction);
                content.AddAttribute(2, nameof(ShadcnDirectionProvider.AdditionalAttributes),
                    new Dictionary<string, object>
                    {
                        ["data-testid"] = "direction-example",
                        ["lang"] = isRightToLeft ? "ar" : "en",
                        ["data-direction-mode"] = direction?.ToString() ?? "Inherited"
                    });
                content.AddAttribute(3, nameof(ShadcnDirectionProvider.ChildContent), DirectionContent(isRightToLeft));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        };
        var control = new ComponentParameterControl(
            "direction",
            "Direction",
            ComponentParameterControlKind.Select,
            "Inherited (RTL)",
            ["Inherited (RTL)", "Left to right (LTR)", "Right to left (RTL)"],
            value => direction = value switch
            {
                "Inherited (RTL)" => null,
                "Left to right (LTR)" => ShadcnDirection.LeftToRight,
                "Right to left (RTL)" => ShadcnDirection.RightToLeft,
                _ => direction
            });
        var example = Example(
            "direction",
            "Nested reading direction",
            "Switch a localized production-workspace form between inherited RTL, explicit LTR, and explicit RTL reading order.",
            DirectionSource(direction),
            preview,
            [control],
            ["inherited", "ltr", "rtl", "form", "responsive"]);
        return example with { RazorSourceProvider = () => DirectionSource(direction) };
    }

    private static ComponentExampleDefinition AspectRatio()
    {
        var ratio = 16d / 9d;
        var ratioLabel = "16:9";
        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", $"showcase-aspect-ratio-demo showcase-aspect-ratio-demo--{ratioLabel.Replace(':', '-')}");
            builder.OpenComponent<ShadcnAspectRatio>(2);
            builder.AddAttribute(3, nameof(ShadcnAspectRatio.Ratio), ratio);
            builder.AddAttribute(4, nameof(ShadcnAspectRatio.ChildContent), AspectRatioContent(ratioLabel));
            builder.CloseComponent();
            builder.CloseElement();
        };
        var control = new ComponentParameterControl(
            "aspect-ratio",
            "Aspect ratio",
            ComponentParameterControlKind.Select,
            "16:9",
            ["16:9", "4:3", "1:1"],
            value =>
            {
                ratioLabel = value;
                ratio = value switch { "16:9" => 16d / 9d, "4:3" => 4d / 3d, "1:1" => 1d, _ => ratio };
            });
        string Source() => $"""
<div class="showcase-aspect-ratio-demo showcase-aspect-ratio-demo--{ratioLabel.Replace(':', '-')}">
    <ShadcnAspectRatio Ratio="@({RatioExpression(ratioLabel)})">
        <figure class="showcase-aspect-ratio-media">
            <img src="images/attachments/workspace-plan.png"
                 alt="Engineering workspace reference" />
            <figcaption>
                <span class="showcase-aspect-ratio-media__copy">
                    <strong>Engineering workspace</strong>
                    <span>Layout reference · Revision C</span>
                </span>
                <span class="showcase-aspect-ratio-media__ratio">{ratioLabel}</span>
            </figcaption>
        </figure>
    </ShadcnAspectRatio>
</div>
""";
        return Example(
            "aspect-ratio",
            "Responsive media frame",
            "Choose a landscape, square, or portrait ratio without measuring in JavaScript.",
            Source(),
            preview,
            [control],
            [
                "16:9",
                "4:3",
                "1:1"
            ]) with
        { RazorSourceProvider = Source };
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
            builder.AddAttribute(6, nameof(ShadcnTypeset.ChildContent), TypographyContent(variant));
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
            "Select a semantic text treatment while preserving the matching HTML element. The preview shows the complete hierarchy in a realistic product brief.",
            "<ShadcnTypeset Tag=\"article\" Size=\"1rem\" Leading=\"1.6\" Flow=\"1rem\" MaxWidth=\"48rem\">\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.H1\">Production brief</ShadcnTypography>\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.Lead\">A compact hierarchy for a quotation workspace.</ShadcnTypography>\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.H2\">Build calm, capable interfaces</ShadcnTypography>\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.H3\">Make the next action obvious</ShadcnTypography>\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.H4\">Use type to organize the work</ShadcnTypography>\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.Paragraph\">Use semantic components to make dense manufacturing workflows easier to scan.</ShadcnTypography>\n    <ShadcnTypography Variant=\"ShadcnTypographyVariant.Blockquote\">Good interfaces reduce the number of decisions people must hold in their heads.</ShadcnTypography>\n</ShadcnTypeset>",
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
            "<ShadcnItem Variant=\"ShadcnItemVariant.Outline\">\n    <ShadcnItemMedia Variant=\"ShadcnItemMediaVariant.Icon\">\n        <svg aria-hidden=\"true\" viewBox=\"0 0 24 24\"><path d=\"M6 3h8l4 4v14H6z\" /><path d=\"M14 3v5h5M9 13h6M9 17h4\" /></svg>\n    </ShadcnItemMedia>\n    <ShadcnItemContent>\n        <ShadcnItemTitle>Production drawing</ShadcnItemTitle>\n        <ShadcnItemDescription>Revision C · Updated 2 minutes ago</ShadcnItemDescription>\n    </ShadcnItemContent>\n</ShadcnItem>",
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
            "Present single-key and multi-key shortcuts in a compact, readable command reference.",
            "<ShadcnKbdGroup>\n    <ShadcnKbd>Esc</ShadcnKbd>\n</ShadcnKbdGroup>\n<ShadcnKbdGroup>\n    <ShadcnKbd>Ctrl</ShadcnKbd><span>+</span><ShadcnKbd>K</ShadcnKbd>\n</ShadcnKbdGroup>\n<ShadcnKbdGroup>\n    <ShadcnKbd>Ctrl</ShadcnKbd><span>+</span><ShadcnKbd>Shift</ShadcnKbd><span>+</span><ShadcnKbd>P</ShadcnKbd>\n</ShadcnKbdGroup>",
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
            builder.OpenElement(0, "section"); builder.AddAttribute(1, "class", orientation == ShadcnSeparatorOrientation.Vertical ? "showcase-separator-demo showcase-separator-demo--vertical" : "showcase-separator-demo");
            builder.OpenElement(2, "div"); builder.AddAttribute(3, "class", "showcase-separator-demo__section"); builder.OpenElement(4, "strong"); builder.AddContent(5, "Production details"); builder.CloseElement(); builder.OpenElement(6, "span"); builder.AddContent(7, "Material and finish requirements."); builder.CloseElement(); builder.CloseElement();
            builder.OpenComponent<ShadcnSeparator>(10); builder.AddAttribute(11, nameof(ShadcnSeparator.Orientation), orientation); builder.AddAttribute(12, nameof(ShadcnSeparator.Decorative), decorative); builder.CloseComponent();
            builder.OpenElement(20, "div"); builder.AddAttribute(21, "class", "showcase-separator-demo__section"); builder.OpenElement(22, "strong"); builder.AddContent(23, "Delivery"); builder.CloseElement(); builder.OpenElement(24, "span"); builder.AddContent(25, "Dispatch estimate and shipping method."); builder.CloseElement(); builder.CloseElement();
            builder.CloseElement();
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
            "Show a meaningful boundary between quotation sections, or switch to a decorative rule when the relationship is only visual.",
            "<section>\n    <h3>Production details</h3>\n    <p>Material and finish requirements.</p>\n    <ShadcnSeparator Decorative=\"false\" />\n    <h3>Delivery</h3>\n    <p>Dispatch estimate and shipping method.</p>\n</section>",
            preview,
            controls,
            ["horizontal", "vertical", "decorative", "semantic"]);
    }

    private static ComponentExampleDefinition Empty()
    {
        var mediaVariant = ShadcnEmptyMediaVariant.Icon;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<EmptyDossierPreview>(0);
            builder.AddAttribute(1, nameof(EmptyDossierPreview.MediaVariant), mediaVariant);
            builder.CloseComponent();
        };
        string Source() => $$"""
@using Maliev.ShadcnBlazor.Components.Actions
@using Maliev.ShadcnBlazor.Components.Content

<section class="showcase-empty-dossier" aria-label="Project workspace empty state">
    <ShadcnEmpty>
        <ShadcnEmptyHeader>
            <ShadcnEmptyMedia Variant="ShadcnEmptyMediaVariant.{{mediaVariant}}">
                <svg aria-hidden="true" viewBox="0 0 24 24">
                    <path d="M3 7.5A2.5 2.5 0 0 1 5.5 5H10l2 2h6.5A2.5 2.5 0 0 1 21 9.5v7A2.5 2.5 0 0 1 18.5 19h-13A2.5 2.5 0 0 1 3 16.5z" />
                    <path d="M12 11v5M9.5 13.5h5" />
                </svg>
            </ShadcnEmptyMedia>
            <ShadcnEmptyTitle>No projects yet</ShadcnEmptyTitle>
            <ShadcnEmptyDescription>
                Create your first project or import an existing project archive.
            </ShadcnEmptyDescription>
        </ShadcnEmptyHeader>
        <ShadcnEmptyContent>
            <div class="showcase-empty-actions">
                <ShadcnButton OnClick="StartProject">Create project</ShadcnButton>
                <ShadcnButton Variant="ShadcnButtonVariant.Outline" OnClick="ImportProject">Import project</ShadcnButton>
            </div>
            <p class="showcase-empty-status" role="status" aria-live="polite">@Feedback</p>
        </ShadcnEmptyContent>
    </ShadcnEmpty>
</section>

@code {
    private string Feedback = "Choose how you want to start.";

    private void StartProject() => Feedback = "A new project workspace is ready.";

    private void ImportProject() =>
        Feedback = "Project import opened. Select a project archive to continue.";
}
""";
        return Example(
            "empty",
            "Empty collection",
            "Start a project workspace from a clear empty state, with distinct create and import paths and immediate action feedback.",
            Source(),
            preview,
            [new ComponentParameterControl(
                "empty-media-variant",
                "Media variant",
                ComponentParameterControlKind.Select,
                mediaVariant.ToString(),
                Enum.GetNames<ShadcnEmptyMediaVariant>(),
                value => mediaVariant = Enum.Parse<ShadcnEmptyMediaVariant>(value))],
            ["media-default", "media-icon", "description", "primary-action", "secondary-action", "status"]) with
        { RazorSourceProvider = Source };
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

    private static RenderFragment DirectionContent(bool isRightToLeft) => builder =>
    {
        var title = isRightToLeft ? "إنشاء مساحة عمل للإنتاج" : "Create a production workspace";
        var description = isRightToLeft ? "تابع عروض الأسعار وملفات الإنتاج مع فريقك." : "Keep quotations and production files together for your team.";
        var status = isRightToLeft ? "العربية · من اليمين إلى اليسار" : "English · Left to right";
        var emailLabel = isRightToLeft ? "البريد الإلكتروني" : "Work email";
        var emailHelp = isRightToLeft ? "سنرسل تحديثات الإنتاج إلى هذا العنوان." : "Production updates will be sent to this address.";
        var workspaceLabel = isRightToLeft ? "اسم مساحة العمل" : "Workspace name";
        var workspaceHelp = isRightToLeft ? "استخدم اسماً يسهل على فريقك التعرف عليه." : "Choose a name your team will recognize.";
        var action = isRightToLeft ? "إنشاء مساحة العمل" : "Create workspace";

        builder.OpenElement(0, "form"); builder.AddAttribute(1, "class", "showcase-direction-form"); builder.AddAttribute(2, "aria-labelledby", "direction-form-title");
        builder.OpenElement(3, "div"); builder.AddAttribute(4, "class", "showcase-direction-form__header"); builder.OpenElement(5, "div"); builder.OpenElement(6, "strong"); builder.AddAttribute(7, "id", "direction-form-title"); builder.AddContent(8, title); builder.CloseElement(); builder.OpenElement(9, "span"); builder.AddContent(10, description); builder.CloseElement(); builder.CloseElement(); builder.OpenElement(11, "span"); builder.AddContent(12, status); builder.CloseElement(); builder.CloseElement();
        AddDirectionField(builder, 20, "direction-email", emailLabel, "natee@example.com", emailHelp, "email");
        AddDirectionField(builder, 40, "direction-workspace", workspaceLabel, "Bangkok Production", workspaceHelp);
        builder.OpenElement(60, "div"); builder.AddAttribute(61, "class", "showcase-direction-form__actions");
        builder.OpenComponent<ShadcnButton>(62); builder.AddAttribute(63, nameof(ShadcnButton.ButtonType), ShadcnButtonType.Button); builder.AddAttribute(64, nameof(ShadcnButton.ChildContent), Text(action)); builder.CloseComponent();
        builder.CloseElement();
        builder.CloseElement();
    };

    private static void AddDirectionField(RenderTreeBuilder builder, int sequence, string id, string label, string value, string description, string type = "text")
    {
        var helpId = $"{id}-help";
        builder.OpenElement(sequence, "div"); builder.AddAttribute(sequence + 1, "class", "showcase-direction-field");
        builder.OpenComponent<ShadcnLabel>(sequence + 2); builder.AddAttribute(sequence + 3, nameof(ShadcnLabel.For), id); builder.AddAttribute(sequence + 4, nameof(ShadcnLabel.ChildContent), Text(label)); builder.CloseComponent();
        builder.OpenComponent<ShadcnInput<string>>(sequence + 5); builder.AddAttribute(sequence + 6, nameof(ShadcnInput<string>.AdditionalAttributes), new Dictionary<string, object> { ["id"] = id, ["aria-describedby"] = helpId }); builder.AddAttribute(sequence + 7, nameof(ShadcnInput<string>.Type), type); builder.AddAttribute(sequence + 8, nameof(ShadcnInput<string>.Value), value); builder.AddAttribute(sequence + 9, nameof(ShadcnInput<string>.AutoComplete), type == "email" ? "email" : "organization"); builder.CloseComponent();
        builder.OpenElement(sequence + 10, "small"); builder.AddAttribute(sequence + 11, "id", helpId); builder.AddContent(sequence + 12, description); builder.CloseElement();
        builder.CloseElement();
    }

    private static string DirectionSource(ShadcnDirection? direction)
    {
        var isRightToLeft = direction is null or ShadcnDirection.RightToLeft;
        var directionValue = direction is null ? "null" : $"ShadcnDirection.{direction}";
        var language = isRightToLeft ? "ar" : "en";
        var title = isRightToLeft ? "إنشاء مساحة عمل للإنتاج" : "Create a production workspace";
        var description = isRightToLeft ? "تابع عروض الأسعار وملفات الإنتاج مع فريقك." : "Keep quotations and production files together for your team.";
        var status = isRightToLeft ? "العربية · من اليمين إلى اليسار" : "English · Left to right";
        var emailLabel = isRightToLeft ? "البريد الإلكتروني" : "Work email";
        var emailHelp = isRightToLeft ? "سنرسل تحديثات الإنتاج إلى هذا العنوان." : "Production updates will be sent to this address.";
        var workspaceLabel = isRightToLeft ? "اسم مساحة العمل" : "Workspace name";
        var workspaceHelp = isRightToLeft ? "استخدم اسماً يسهل على فريقك التعرف عليه." : "Choose a name your team will recognize.";
        var action = isRightToLeft ? "إنشاء مساحة العمل" : "Create workspace";

        return $"""
@using Maliev.ShadcnBlazor.Components.Actions
@using Maliev.ShadcnBlazor.Components.Direction
@using Maliev.ShadcnBlazor.Components.Forms

<ShadcnDirectionProvider Direction="ShadcnDirection.RightToLeft">
    <ShadcnDirectionProvider Direction="{directionValue}" lang="{language}">
        <form class="showcase-direction-form" aria-labelledby="direction-form-title">
            <div class="showcase-direction-form__header">
                <div>
                    <strong id="direction-form-title">{title}</strong>
                    <span>{description}</span>
                </div>
                <span>{status}</span>
            </div>
            <div class="showcase-direction-field">
                <ShadcnLabel For="direction-email">{emailLabel}</ShadcnLabel>
                <ShadcnInput<string> Id="direction-email" Type="email" Value="natee@example.com" AutoComplete="email" aria-describedby="direction-email-help" />
                <small id="direction-email-help">{emailHelp}</small>
            </div>
            <div class="showcase-direction-field">
                <ShadcnLabel For="direction-workspace">{workspaceLabel}</ShadcnLabel>
                <ShadcnInput<string> Id="direction-workspace" Value="Bangkok Production" AutoComplete="organization" aria-describedby="direction-workspace-help" />
                <small id="direction-workspace-help">{workspaceHelp}</small>
            </div>
            <div class="showcase-direction-form__actions">
                <ShadcnButton>{action}</ShadcnButton>
            </div>
        </form>
    </ShadcnDirectionProvider>
</ShadcnDirectionProvider>
""";
    }

    private static RenderFragment AspectRatioContent(string ratioLabel) => builder =>
    {
        builder.OpenElement(0, "figure"); builder.AddAttribute(1, "class", "showcase-aspect-ratio-media");
        builder.OpenElement(2, "img"); builder.AddAttribute(3, "src", "images/attachments/workspace-plan.png"); builder.AddAttribute(4, "alt", "Engineering workspace reference"); builder.CloseElement();
        builder.OpenElement(5, "figcaption");
        builder.OpenElement(6, "span"); builder.AddAttribute(7, "class", "showcase-aspect-ratio-media__copy"); builder.OpenElement(8, "strong"); builder.AddContent(9, "Engineering workspace"); builder.CloseElement(); builder.OpenElement(10, "span"); builder.AddContent(11, "Layout reference · Revision C"); builder.CloseElement(); builder.CloseElement();
        builder.OpenElement(12, "span"); builder.AddAttribute(13, "class", "showcase-aspect-ratio-media__ratio"); builder.AddContent(14, ratioLabel); builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
    };

    private static string RatioExpression(string ratioLabel) => ratioLabel switch
    {
        "4:3" => "4d / 3d",
        "1:1" => "1d / 1d",
        _ => "16d / 9d"
    };

    private static RenderFragment TypographyContent(ShadcnTypographyVariant selected) => builder =>
    {
        builder.OpenElement(0, "div"); builder.AddAttribute(1, "class", "showcase-typography-preview");
        builder.OpenElement(2, "div"); builder.AddAttribute(3, "class", "showcase-typography-preview__selected"); builder.OpenElement(4, "span"); builder.AddContent(5, "Selected treatment"); builder.CloseElement(); builder.OpenComponent<ShadcnTypography>(6); builder.AddAttribute(7, nameof(ShadcnTypography.Variant), selected); builder.AddAttribute(8, nameof(ShadcnTypography.ChildContent), Text("Build calm, capable interfaces")); builder.CloseComponent(); builder.CloseElement();
        AddTypography(builder, 20, ShadcnTypographyVariant.H1, "Production brief");
        AddTypography(builder, 30, ShadcnTypographyVariant.Lead, "A compact hierarchy for a quotation workspace.");
        AddTypography(builder, 40, ShadcnTypographyVariant.H2, "Build calm, capable interfaces");
        AddTypography(builder, 50, ShadcnTypographyVariant.H3, "Make the next action obvious");
        AddTypography(builder, 60, ShadcnTypographyVariant.H4, "Use type to organize the work");
        AddTypography(builder, 70, ShadcnTypographyVariant.Paragraph, "Semantic typography lets a production brief stay readable when it moves from a desktop review to a compact mobile handoff.");
        AddTypography(builder, 80, ShadcnTypographyVariant.Blockquote, "Good interfaces reduce the number of decisions people must hold in their heads.");
        builder.OpenComponent<ShadcnTypography>(90); builder.AddAttribute(91, nameof(ShadcnTypography.Variant), ShadcnTypographyVariant.UnorderedList); builder.AddAttribute(92, nameof(ShadcnTypography.ChildContent), (RenderFragment)(list => { list.OpenElement(0, "li"); list.AddContent(1, "Clear status at a glance"); list.CloseElement(); list.OpenElement(2, "li"); list.AddContent(3, "Helpful context near each control"); list.CloseElement(); })); builder.CloseComponent();
        builder.CloseElement();
    };

    private static void AddTypography(RenderTreeBuilder builder, int sequence, ShadcnTypographyVariant variant, string text)
    {
        builder.OpenComponent<ShadcnTypography>(sequence); builder.AddAttribute(sequence + 1, nameof(ShadcnTypography.Variant), variant); builder.AddAttribute(sequence + 2, nameof(ShadcnTypography.ChildContent), Text(text)); builder.CloseComponent();
    }

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
        builder.AddAttribute(2, nameof(ShadcnItemMedia.ChildContent), FileIcon());
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
        builder.OpenElement(0, "div"); builder.AddAttribute(1, "class", "showcase-kbd-list");
        AddShortcut(builder, 10, "Open command palette", ["Ctrl", "K"]);
        AddShortcut(builder, 20, "Search files", ["Ctrl", "Shift", "P"]);
        AddShortcut(builder, 30, "Close dialog", ["Esc"]);
        builder.CloseElement();
    };

    private static RenderFragment FileIcon() => builder =>
    {
        builder.OpenElement(0, "svg"); builder.AddAttribute(1, "viewBox", "0 0 24 24"); builder.AddAttribute(2, "aria-hidden", "true"); builder.OpenElement(3, "path"); builder.AddAttribute(4, "d", "M6 3h8l4 4v14H6z"); builder.CloseElement(); builder.OpenElement(5, "path"); builder.AddAttribute(6, "d", "M14 3v5h5M9 13h6M9 17h4"); builder.CloseElement(); builder.CloseElement();
    };

    private static RenderFragment FolderPlusIcon() => builder =>
    {
        builder.OpenElement(0, "svg"); builder.AddAttribute(1, "viewBox", "0 0 24 24"); builder.AddAttribute(2, "aria-hidden", "true"); builder.OpenElement(3, "path"); builder.AddAttribute(4, "d", "M3 7.5A2.5 2.5 0 0 1 5.5 5H10l2 2h6.5A2.5 2.5 0 0 1 21 9.5v7A2.5 2.5 0 0 1 18.5 19h-13A2.5 2.5 0 0 1 3 16.5z"); builder.CloseElement(); builder.OpenElement(5, "path"); builder.AddAttribute(6, "d", "M12 11v5M9.5 13.5h5"); builder.CloseElement(); builder.CloseElement();
    };

    private sealed class EmptyDossierPreview : ComponentBase
    {
        [Parameter] public ShadcnEmptyMediaVariant MediaVariant { get; set; } = ShadcnEmptyMediaVariant.Icon;

        private string Feedback { get; set; } = "Choose how you want to start.";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "section");
            builder.AddAttribute(1, "class", "showcase-empty-dossier");
            builder.AddAttribute(2, "aria-label", "Project workspace empty state");
            builder.OpenComponent<ShadcnEmpty>(3);
            builder.AddAttribute(4, nameof(ShadcnEmpty.ChildContent), EmptyContent());
            builder.CloseComponent();
            builder.CloseElement();
        }

        private RenderFragment EmptyContent() => builder =>
        {
            builder.OpenComponent<ShadcnEmptyHeader>(0);
            builder.AddAttribute(1, nameof(ShadcnEmptyHeader.ChildContent), (RenderFragment)(header =>
            {
                header.OpenComponent<ShadcnEmptyMedia>(0);
                header.AddAttribute(1, nameof(ShadcnEmptyMedia.Variant), MediaVariant);
                header.AddAttribute(2, nameof(ShadcnEmptyMedia.ChildContent), FolderPlusIcon());
                header.CloseComponent();
                header.OpenComponent<ShadcnEmptyTitle>(3);
                header.AddAttribute(4, nameof(ShadcnEmptyTitle.ChildContent), Text("No projects yet"));
                header.CloseComponent();
                header.OpenComponent<ShadcnEmptyDescription>(5);
                header.AddAttribute(6, nameof(ShadcnEmptyDescription.ChildContent), Text("Create your first project or import an existing project archive."));
                header.CloseComponent();
            }));
            builder.CloseComponent();
            builder.OpenComponent<ShadcnEmptyContent>(2);
            builder.AddAttribute(3, nameof(ShadcnEmptyContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenElement(0, "div");
                content.AddAttribute(1, "class", "showcase-empty-actions");
                AddAction(content, 2, "Create project", ShadcnButtonVariant.Default, "create", StartProject);
                AddAction(content, 10, "Import project", ShadcnButtonVariant.Outline, "import", ImportProject);
                content.CloseElement();
                content.OpenElement(20, "p");
                content.AddAttribute(21, "class", "showcase-empty-status");
                content.AddAttribute(22, "role", "status");
                content.AddAttribute(23, "aria-live", "polite");
                content.AddContent(24, Feedback);
                content.CloseElement();
            }));
            builder.CloseComponent();
        };

        private void AddAction(
            RenderTreeBuilder builder,
            int sequence,
            string label,
            ShadcnButtonVariant variant,
            string action,
            Action handler)
        {
            builder.OpenComponent<ShadcnButton>(sequence);
            builder.AddAttribute(sequence + 1, nameof(ShadcnButton.Variant), variant);
            builder.AddAttribute(sequence + 2, nameof(ShadcnButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, handler));
            builder.AddAttribute(sequence + 3, nameof(ShadcnButton.AdditionalAttributes), new Dictionary<string, object>
            {
                ["data-empty-action"] = action
            });
            builder.AddAttribute(sequence + 4, nameof(ShadcnButton.ChildContent), Text(label));
            builder.CloseComponent();
        }

        private void StartProject() => Feedback = "A new project workspace is ready.";

        private void ImportProject() =>
            Feedback = "Project import opened. Select a project archive to continue.";
    }

    private static void AddShortcut(RenderTreeBuilder builder, int sequence, string label, IReadOnlyList<string> keys)
    {
        builder.OpenElement(sequence, "div"); builder.AddAttribute(sequence + 1, "class", "showcase-kbd-row"); builder.OpenElement(sequence + 2, "span"); builder.AddContent(sequence + 3, label); builder.CloseElement(); builder.OpenComponent<ShadcnKbdGroup>(sequence + 4); builder.AddAttribute(sequence + 5, nameof(ShadcnKbdGroup.ChildContent), (RenderFragment)(group =>
        {
            var keySequence = 0;
            foreach (var key in keys)
            {
                if (keySequence > 0) group.AddContent(keySequence * 10, "+");
                group.OpenComponent<ShadcnKbd>(keySequence * 10 + 1); group.AddAttribute(keySequence * 10 + 2, nameof(ShadcnKbd.ChildContent), Text(key)); group.CloseComponent(); keySequence++;
            }
        })); builder.CloseComponent(); builder.CloseElement();
    }
}
