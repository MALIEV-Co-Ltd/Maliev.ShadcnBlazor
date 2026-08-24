using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Direction;
using Maliev.ShadcnBlazor.Components.Forms;
using Maliev.ShadcnBlazor.Components.Layout;
using Maliev.ShadcnBlazor.Components.Typography;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;
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
        "bento-grid" => BentoGrid(),
        "visual-style-scope" => VisualStyleScopeExamples.Create(),
        "code-block" => CodeBlock(),
        "typography" => [Typography()],
        "label" => [Label()],
        "field" => [Field()],
        "item" => [Item()],
        "kbd" => [Kbd()],
        "separator" => [Separator()],
        "empty" => [Empty()],
        _ => []
    };

    private static IReadOnlyList<ComponentExampleDefinition> CodeBlock()
    {
        const string razor = """
@using Maliev.ShadcnBlazor.Components.Actions

<ShadcnButton Disabled="@isSaving" aria-label="บันทึก @project.Name">
    บันทึกงาน
</ShadcnButton>

@code {
    private bool isSaving;
    private Project project = new("Bangkok line");
}
""";
        const string csharp = """
public async Task SaveAsync(CancellationToken cancellationToken)
{
    var completed = await repository.SaveAsync(project, cancellationToken);
    status = completed ? "Saved" : "Retry";
}
""";
        var sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["razor"] = razor,
            ["csharp"] = csharp
        };
        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-code-block-dossier");
            builder.OpenComponent<ShadcnCodeBlock>(2);
            builder.AddAttribute(3, nameof(ShadcnCodeBlock.Source), razor);
            builder.AddAttribute(4, nameof(ShadcnCodeBlock.Language), "razor");
            builder.AddAttribute(5, nameof(ShadcnCodeBlock.Sources), sources);
            builder.AddAttribute(6, nameof(ShadcnCodeBlock.AdditionalAttributes), new Dictionary<string, object> { ["aria-label"] = "Razor and C# production example" });
            builder.CloseComponent();
            builder.OpenComponent<ShadcnCodeBlock>(7);
            builder.AddAttribute(8, nameof(ShadcnCodeBlock.Source), csharp);
            builder.AddAttribute(9, nameof(ShadcnCodeBlock.Language), "csharp");
            builder.CloseComponent();
            builder.OpenComponent<ShadcnCodeBlock>(10);
            builder.AddAttribute(11, nameof(ShadcnCodeBlock.Source), "<ShadcnButton aria-describedby=\"production-work-order-with-a-long-accessible-description\">เปิดใบสั่งผลิตหมายเลข MALIEV-BKK-2026-000184</ShadcnButton>");
            builder.AddAttribute(12, nameof(ShadcnCodeBlock.Language), "razor");
            builder.CloseComponent();
            builder.CloseElement();
        };

        return
        [
            Example("code-block", "Razor and C# workspace", "Compare three realistic source scenarios, switch languages, inspect semantic editor tokens, and copy the exact source.", """"
<ShadcnCodeBlock Source="@razorSource"
                 Language="razor"
                 Sources="@sources" />
<ShadcnCodeBlock Source="@csharpSource" Language="csharp" />
<ShadcnCodeBlock Source="@longRazorSource" Language="razor" />

@code {
    private const string razorSource = """
@using Maliev.ShadcnBlazor.Components.Actions

<ShadcnButton Disabled="@isSaving" aria-label="บันทึก @project.Name">
    บันทึกงาน
</ShadcnButton>

@code {
    private bool isSaving;
    private Project project = new("Bangkok line");
}
""";
    private const string csharpSource = """
public async Task SaveAsync(CancellationToken cancellationToken)
{
    var completed = await repository.SaveAsync(project, cancellationToken);
    status = completed ? "Saved" : "Retry";
}
""";
    private const string longRazorSource = "<ShadcnButton aria-describedby=\"production-work-order-with-a-long-accessible-description\">เปิดใบสั่งผลิตหมายเลข MALIEV-BKK-2026-000184</ShadcnButton>";
    private readonly IReadOnlyDictionary<string, string> sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["razor"] = razorSource,
        ["csharp"] = csharpSource
    };
}
"""", preview, [], ["razor", "csharp", "language", "copy", "overflow", "mobile", "RTL", "Thai"])
        ];
    }

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

    private static IReadOnlyList<ComponentExampleDefinition> BentoGrid()
    {
        return
        [
            BentoExample(
                "bento-grid-featured",
                "Production overview",
                "Give the primary production summary two tracks while supporting work stays compact.",
                4,
                2,
                [("Capacity plan", 2, 1), ("Quality holds", 1, 1), ("Dispatch queue", 1, 1), ("Machine status", 1, 1)]),
            BentoExample(
                "bento-grid-mixed",
                "Mixed operational spans",
                "Combine standard, wide, and tall regions without changing their source order.",
                4,
                2,
                [("Inspection results", 2, 1), ("Reviewer activity", 1, 2), ("Revision notes", 1, 1), ("Release checklist", 2, 1)],
                masonry: true),
            BentoExample(
                "bento-grid-reflow",
                "Narrow container reflow",
                "Let a two-track handoff layout collapse safely when its container becomes narrow.",
                2,
                1,
                [("Delivery address", 2, 1), ("Carrier", 1, 1), ("Collection window", 1, 1)])
        ];
    }

    private static ComponentExampleDefinition BentoExample(
        string id,
        string title,
        string description,
        int columns,
        int mediumColumns,
        IReadOnlyList<(string Title, int ColumnSpan, int RowSpan)> items,
        bool masonry = false)
    {
        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-bento-example");
            builder.OpenComponent<ShadcnBentoGrid>(2);
            builder.AddAttribute(3, nameof(ShadcnBentoGrid.Columns), columns);
            builder.AddAttribute(4, nameof(ShadcnBentoGrid.MediumColumns), mediumColumns);
            builder.AddAttribute(5, nameof(ShadcnBentoGrid.Masonry), masonry);
            builder.AddAttribute(6, nameof(ShadcnBentoGrid.ChildContent), (RenderFragment)(content =>
            {
                var sequence = 0;
                foreach (var item in items)
                {
                    content.OpenComponent<ShadcnBentoItem>(sequence++);
                    content.AddAttribute(sequence++, nameof(ShadcnBentoItem.ColumnSpan), item.ColumnSpan);
                    content.AddAttribute(sequence++, nameof(ShadcnBentoItem.RowSpan), item.RowSpan);
                    content.AddAttribute(sequence++, nameof(ShadcnBentoItem.ChildContent), BentoExampleCard(item.Title));
                    content.CloseComponent();
                }
            }));
            builder.CloseComponent();
            builder.CloseElement();
        };
        var itemSource = string.Join(Environment.NewLine, items.Select(item =>
            $"    <ShadcnBentoItem ColumnSpan=\"{item.ColumnSpan}\" RowSpan=\"{item.RowSpan}\"><article>{item.Title}</article></ShadcnBentoItem>"));
        var masonryAttribute = masonry ? " Masonry=\"true\"" : string.Empty;
        var source = $"""
<ShadcnBentoGrid Columns="{columns}" MediumColumns="{mediumColumns}"{masonryAttribute}>
{itemSource}
</ShadcnBentoGrid>
""";
        return new(id, title, description, source, preview, [], masonry
            ? ["responsive", "container-query", "spans", "source-order", "masonry"]
            : ["responsive", "container-query", "spans", "source-order"]);
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
        string Source() => $"""
<ShadcnTypeset Tag="{tag}" Size="{size}" Leading="{leading}" Flow="{flow}" MaxWidth="{maxWidth}">
    <div class="showcase-typography-preview">
        <div class="showcase-typography-preview__selected">
            <span>Selected treatment</span>
            {TypographySelectedSource(variant)}
        </div>
        <ShadcnTypography Variant="ShadcnTypographyVariant.H1">Quotation handoff</ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.Lead">CNC enclosure · Revision C</ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.H2">Review before production</ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.H3">Release checklist</ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.H4">Source file</ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.Paragraph">
            Confirm <ShadcnTypography Variant="ShadcnTypographyVariant.InlineCode">Q-2026-0814-R3.step</ShadcnTypography>
            against the approved drawing before releasing the work order.
        </ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.Blockquote">Keep tolerances and material notes beside the decision they support.</ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.UnorderedList">
            <li>Confirm the drawing revision</li>
            <li>Verify 6061-T6 material availability</li>
            <li>Record the inspection owner</li>
        </ShadcnTypography>
        <ShadcnTypography Variant="ShadcnTypographyVariant.Muted">Updated today · Ready for review</ShadcnTypography>
    </div>
</ShadcnTypeset>
""";
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
            Source(),
            preview,
            controls,
            options.Select(name => name.ToLowerInvariant())
                .Concat(["typeset-div", "typeset-article", "typeset-section", "typeset-rhythm"])
                .ToArray()) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Label()
    {
        var disabled = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<LabelDossierPreview>(0);
            builder.AddAttribute(1, nameof(LabelDossierPreview.Disabled), disabled);
            builder.CloseComponent();
        };
        var example = Example(
            "label",
            "Project naming field",
            "Use a visible, associated label with the package input in a realistic project setup flow.",
            LabelSource(disabled),
            preview,
            [new ComponentParameterControl(
                "label-disabled",
                "Disabled",
                ComponentParameterControlKind.Toggle,
                "false",
                [],
                value => disabled = bool.Parse(value))],
            ["associated", "required", "interactive", "enabled", "disabled"]);
        return example with { RazorSourceProvider = () => LabelSource(disabled) };
    }

    private static string LabelSource(bool disabled)
    {
        var state = disabled.ToString().ToLowerInvariant();
        return $$"""
            <section class="showcase-label-dossier" data-disabled="{{state}}" aria-labelledby="project-form-title">
                <header>
                    <h3 id="project-form-title">Create a production project</h3>
                    <p>Give the quotation workspace a name your team can recognize at a glance.</p>
                </header>
                <div>
                    <ShadcnLabel For="project-name">
                        Project name <span>Required</span>
                    </ShadcnLabel>
                    <ShadcnInput TValue="string"
                                 id="project-name"
                                 @bind-Value="ProjectName"
                                 Name="project-name"
                                 Placeholder="e.g. Fixture inspection · Revision C"
                                 Disabled="{{state}}"
                                 Required="true"
                                 aria-label="Project name"
                                 aria-describedby="project-name-help" />
                    <p id="project-name-help">Shown to engineering, quality, and purchasing throughout the quotation.</p>
                </div>
                <output for="project-name" aria-live="polite">
                    <span>Workspace preview</span>
                    <strong>@DisplayName</strong>
                    <code>Production / @DisplayName</code>
                </output>
            </section>

            @code {
                private string ProjectName { get; set; } = "Fixture inspection · Revision C";
                private string DisplayName => string.IsNullOrWhiteSpace(ProjectName) ? "Untitled project" : ProjectName.Trim();
            }
            """;
    }

    private static ComponentExampleDefinition Field()
    {
        var orientation = ShadcnFieldOrientation.Vertical;
        var legendVariant = ShadcnFieldLegendVariant.Legend;
        var invalid = false;
        var disabled = false;
        RenderFragment preview = builder =>
        {
            builder.OpenComponent<FieldDossierPreview>(0);
            builder.AddAttribute(1, nameof(FieldDossierPreview.Orientation), orientation);
            builder.AddAttribute(2, nameof(FieldDossierPreview.LegendVariant), legendVariant);
            builder.AddAttribute(3, nameof(FieldDossierPreview.Invalid), invalid);
            builder.AddAttribute(4, nameof(FieldDossierPreview.Disabled), disabled);
            builder.CloseComponent();
        };

        string Source() => $$"""
            <form @onsubmit="HandleSubmit" @onsubmit:preventDefault>
                <ShadcnFieldSet Disabled="{{disabled.ToString().ToLowerInvariant()}}">
                    <ShadcnFieldLegend Variant="ShadcnFieldLegendVariant.{{legendVariant}}">Card details</ShadcnFieldLegend>
                    <ShadcnFieldGroup>
                        <div class="payment-card-row">
                            <ShadcnField Orientation="ShadcnFieldOrientation.{{orientation}}" Disabled="{{disabled.ToString().ToLowerInvariant()}}" DescriptionId="cardholder-help">
                                <ShadcnFieldLabel For="cardholder">Name on card</ShadcnFieldLabel>
                                <ShadcnInput TValue="string" id="cardholder" Name="cardholder" AutoComplete="cc-name" @bind-Value="cardholder" />
                                <ShadcnFieldDescription Id="cardholder-help">Enter the name exactly as it appears on the card.</ShadcnFieldDescription>
                            </ShadcnField>

                            <ShadcnField Orientation="ShadcnFieldOrientation.{{orientation}}" Invalid="{{invalid.ToString().ToLowerInvariant()}}" Disabled="{{disabled.ToString().ToLowerInvariant()}}" DescriptionId="card-number-help" ErrorId="card-number-error">
                                <ShadcnFieldLabel For="card-number">Card number</ShadcnFieldLabel>
                                <ShadcnInput TValue="string" id="card-number" Name="card-number" InputMode="numeric" AutoComplete="cc-number" @bind-Value="cardNumber" />
                                <ShadcnFieldDescription Id="card-number-help" dir="auto">Use the 16-digit number printed on the card.</ShadcnFieldDescription>
                                @if (invalid)
                                {
                                    <ShadcnFieldError Id="card-number-error">Check the card number and try again.</ShadcnFieldError>
                                }
                            </ShadcnField>
                        </div>

                        <div class="payment-security-row">
                            <ShadcnField>
                                <ShadcnFieldLabel For="expiry-month">Month</ShadcnFieldLabel>
                                <ShadcnSelect TValue="string" id="expiry-month" aria-label="Expiry month" Name="expiry-month" Placeholder="MM" Options="months" @bind-Value="month" />
                            </ShadcnField>
                            <ShadcnField>
                                <ShadcnFieldLabel For="expiry-year">Year</ShadcnFieldLabel>
                                <ShadcnSelect TValue="string" id="expiry-year" aria-label="Expiry year" Name="expiry-year" Placeholder="YYYY" Options="years" @bind-Value="year" />
                            </ShadcnField>
                            <ShadcnField DescriptionId="cvv-help">
                                <ShadcnFieldLabel For="cvv">CVV</ShadcnFieldLabel>
                                <ShadcnInput TValue="string" id="cvv" Name="cvv" InputMode="numeric" AutoComplete="cc-csc" @bind-Value="cvv" />
                                <ShadcnFieldDescription Id="cvv-help">3 digits</ShadcnFieldDescription>
                            </ShadcnField>
                        </div>

                        <ShadcnFieldSeparator>Billing</ShadcnFieldSeparator>
                        <ShadcnField Orientation="ShadcnFieldOrientation.Horizontal">
                            <ShadcnCheckbox id="same-address" @bind-Value="sameAsShipping" />
                            <ShadcnFieldContent>
                                <ShadcnFieldLabel For="same-address">Same as shipping address</ShadcnFieldLabel>
                                <ShadcnFieldDescription dir="auto">Use the delivery address for this payment.</ShadcnFieldDescription>
                            </ShadcnFieldContent>
                        </ShadcnField>
                        <ShadcnField DescriptionId="comments-help">
                            <ShadcnFieldLabel For="comments">Comments</ShadcnFieldLabel>
                            <ShadcnTextarea TValue="string" id="comments" Name="comments" Rows="3" Placeholder="Add a note for this payment" @bind-Value="comments" />
                            <ShadcnFieldDescription Id="comments-help" dir="auto">Optional notes are included with the billing record.</ShadcnFieldDescription>
                        </ShadcnField>
                    </ShadcnFieldGroup>
                </ShadcnFieldSet>
                <ShadcnButton ButtonType="ShadcnButtonType.Submit" Disabled="{{disabled.ToString().ToLowerInvariant()}}">Review payment</ShadcnButton>
                <ShadcnButton Variant="ShadcnButtonVariant.Outline" Disabled="{{disabled.ToString().ToLowerInvariant()}}">Cancel</ShadcnButton>
                <p role="status" aria-live="polite">@status</p>
            </form>

            @code {
                private string cardholder = "Suda Chantarangsu";
                private string cardNumber = "4242 4242 4242 4242";
                private string month = "08";
                private string year = "2029";
                private string cvv = "123";
                private string comments = string.Empty;
                private bool? sameAsShipping = true;
                private bool invalid = {{invalid.ToString().ToLowerInvariant()}};
                private string status = string.Empty;
                private readonly IReadOnlyList<ShadcnSelectOption<string>> months = Enumerable.Range(1, 12).Select(value => new ShadcnSelectOption<string>(value.ToString("00"), value.ToString("00"))).ToArray();
                private readonly IReadOnlyList<ShadcnSelectOption<string>> years = Enumerable.Range(2026, 8).Select(value => new ShadcnSelectOption<string>(value.ToString(), value.ToString())).ToArray();

                private void HandleSubmit() => status = invalid
                    ? "Review the highlighted card number before continuing."
                    : $"Payment details for {cardholder} are ready to review.";
            }
            """;
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
                "false",
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
        var example = Example(
            "field",
            "Payment form composition",
            "Compose a complete payment form with library inputs, descriptions, validation, grouped controls, and actions.",
            Source(),
            preview,
            controls,
            ["vertical", "horizontal", "responsive", "valid", "invalid", "enabled", "disabled", "legend", "label"]);
        return example with { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Item()
    {
        var variant = ShadcnItemVariant.Outline;
        var size = ShadcnItemSize.Default;
        var mediaVariant = ShadcnItemMediaVariant.Icon;
        var link = false;
        RenderFragment preview = builder => builder.AddContent(0, ItemDossier(variant, size, mediaVariant, link));
        string Source() => BuildItemSource(variant, size, mediaVariant, link);
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
            "Production file queue",
            "Review uploaded project files with real media, status, compact sizing, and optional link behavior.",
            Source(),
            preview,
            controls,
            ["default", "outline", "muted", "small", "link", "media-default", "media-icon", "media-image"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Kbd()
    {
        var platform = "Windows";
        RenderFragment preview = builder => builder.AddContent(0, KeyboardReference(platform));
        var control = new ComponentParameterControl(
            "kbd-platform",
            "Platform",
            ComponentParameterControlKind.Select,
            platform,
            ["Windows", "macOS"],
            value => platform = value);
        string Source() => KeyboardReferenceSource(platform);
        return Example(
            "kbd",
            "Command shortcuts",
            "Show a compact command reference with one-, two-, and three-key combinations for each platform.",
            Source(),
            preview,
            [control],
            ["single-key", "two-key", "three-key", "windows", "macos", "rtl", "forced-colors"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Separator()
    {
        var orientation = ShadcnSeparatorOrientation.Horizontal;
        var decorative = false;
        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "section");
            builder.AddAttribute(1, "class", orientation == ShadcnSeparatorOrientation.Vertical ? "showcase-separator-demo showcase-separator-demo--vertical" : "showcase-separator-demo");
            builder.AddAttribute(2, "aria-label", "Quotation summary");
            builder.OpenElement(3, "header"); builder.AddAttribute(4, "class", "showcase-separator-demo__header"); builder.OpenElement(5, "strong"); builder.OpenElement(6, "bdi"); builder.AddContent(7, "Quotation #Q-4189"); builder.CloseElement(); builder.CloseElement(); builder.OpenElement(8, "span"); builder.OpenElement(9, "bdi"); builder.AddContent(10, "CNC enclosure · Revision C"); builder.CloseElement(); builder.CloseElement(); builder.CloseElement();
            builder.OpenElement(10, "div"); builder.AddAttribute(11, "class", "showcase-separator-demo__content");
            AddSeparatorDetail(builder, 20, "Material", "Aluminium 6061", "Clear anodized finish");
            builder.OpenComponent<ShadcnSeparator>(30); builder.AddAttribute(31, nameof(ShadcnSeparator.Orientation), orientation); builder.AddAttribute(32, nameof(ShadcnSeparator.Decorative), decorative); if (!decorative) builder.AddAttribute(33, "aria-label", "Production and delivery details"); builder.CloseComponent();
            AddSeparatorDetail(builder, 40, "Delivery", "Ready in 8 business days", "24 parts · Bangkok");
            builder.CloseElement();
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
        string Source()
        {
            var semanticName = decorative ? string.Empty : " aria-label=\"Production and delivery details\"";
            return $"""
@using Maliev.ShadcnBlazor.Components.Content

<section class="showcase-separator-demo{(orientation == ShadcnSeparatorOrientation.Vertical ? " showcase-separator-demo--vertical" : string.Empty)}" aria-label="Quotation summary">
    <header class="showcase-separator-demo__header">
        <strong><bdi>Quotation #Q-4189</bdi></strong>
        <span><bdi>CNC enclosure · Revision C</bdi></span>
    </header>
    <div class="showcase-separator-demo__content">
        <div class="showcase-separator-demo__section">
            <span>Material</span>
            <strong><bdi>Aluminium 6061</bdi></strong>
            <small><bdi>Clear anodized finish</bdi></small>
        </div>
        <ShadcnSeparator Orientation="ShadcnSeparatorOrientation.{orientation}" Decorative="{decorative.ToString().ToLowerInvariant()}"{semanticName} />
        <div class="showcase-separator-demo__section">
            <span>Delivery</span>
            <strong><bdi>Ready in 8 business days</bdi></strong>
            <small><bdi>24 parts · Bangkok</bdi></small>
        </div>
    </div>
</section>
""";
        }
        return Example(
            "separator",
            "Semantic section separator",
            "Show a meaningful boundary between quotation sections, or switch to a decorative rule when the relationship is only visual.",
            Source(),
            preview,
            controls,
            ["horizontal", "vertical", "decorative", "semantic", "rtl", "forced-colors"]) with
        { RazorSourceProvider = Source };
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

<section class="showcase-empty-dossier" aria-label="Project workspace empty state" dir="auto">
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

    private static RenderFragment BentoExampleCard(string title) => builder =>
    {
        builder.OpenElement(0, "article");
        builder.AddAttribute(1, "class", "showcase-bento-example__card");
        builder.OpenElement(2, "strong");
        builder.AddContent(3, title);
        builder.CloseElement();
        builder.OpenElement(4, "span");
        builder.AddContent(5, "Live package layout item");
        builder.CloseElement();
        builder.CloseElement();
    };

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
        builder.OpenElement(2, "div"); builder.AddAttribute(3, "class", "showcase-typography-preview__selected"); builder.OpenElement(4, "span"); builder.AddContent(5, "Selected treatment"); builder.CloseElement(); builder.OpenComponent<ShadcnTypography>(6); builder.AddAttribute(7, nameof(ShadcnTypography.Variant), selected); builder.AddAttribute(8, nameof(ShadcnTypography.ChildContent), TypographySelectedContent(selected)); builder.CloseComponent(); builder.CloseElement();
        AddTypography(builder, 20, ShadcnTypographyVariant.H1, "Quotation handoff");
        AddTypography(builder, 30, ShadcnTypographyVariant.Lead, "CNC enclosure · Revision C");
        AddTypography(builder, 40, ShadcnTypographyVariant.H2, "Review before production");
        AddTypography(builder, 50, ShadcnTypographyVariant.H3, "Release checklist");
        AddTypography(builder, 60, ShadcnTypographyVariant.H4, "Source file");
        builder.OpenComponent<ShadcnTypography>(70); builder.AddAttribute(71, nameof(ShadcnTypography.Variant), ShadcnTypographyVariant.Paragraph); builder.AddAttribute(72, nameof(ShadcnTypography.ChildContent), (RenderFragment)(content => { content.AddContent(0, "Confirm "); content.OpenComponent<ShadcnTypography>(1); content.AddAttribute(2, nameof(ShadcnTypography.Variant), ShadcnTypographyVariant.InlineCode); content.AddAttribute(3, nameof(ShadcnTypography.ChildContent), Text("Q-2026-0814-R3.step")); content.CloseComponent(); content.AddContent(4, " against the approved drawing before releasing the work order."); })); builder.CloseComponent();
        AddTypography(builder, 80, ShadcnTypographyVariant.Blockquote, "Keep tolerances and material notes beside the decision they support.");
        builder.OpenComponent<ShadcnTypography>(90); builder.AddAttribute(91, nameof(ShadcnTypography.Variant), ShadcnTypographyVariant.UnorderedList); builder.AddAttribute(92, nameof(ShadcnTypography.ChildContent), TypographyListContent()); builder.CloseComponent();
        AddTypography(builder, 100, ShadcnTypographyVariant.Muted, "Updated today · Ready for review");
        builder.CloseElement();
    };

    private static RenderFragment TypographySelectedContent(ShadcnTypographyVariant variant) =>
        variant is ShadcnTypographyVariant.UnorderedList or ShadcnTypographyVariant.OrderedList
            ? TypographyListContent()
            : Text("CNC enclosure · Revision C");

    private static RenderFragment TypographyListContent() => list =>
    {
        list.OpenElement(0, "li"); list.AddContent(1, "Confirm the drawing revision"); list.CloseElement();
        list.OpenElement(2, "li"); list.AddContent(3, "Verify 6061-T6 material availability"); list.CloseElement();
        list.OpenElement(4, "li"); list.AddContent(5, "Record the inspection owner"); list.CloseElement();
    };

    private static string TypographySelectedSource(ShadcnTypographyVariant variant) =>
        variant is ShadcnTypographyVariant.UnorderedList or ShadcnTypographyVariant.OrderedList
            ? $"<ShadcnTypography Variant=\"ShadcnTypographyVariant.{variant}\">\n                <li>Confirm the drawing revision</li>\n                <li>Verify 6061-T6 material availability</li>\n                <li>Record the inspection owner</li>\n            </ShadcnTypography>"
            : $"<ShadcnTypography Variant=\"ShadcnTypographyVariant.{variant}\">CNC enclosure · Revision C</ShadcnTypography>";

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

    private static RenderFragment ItemDossier(
        ShadcnItemVariant variant,
        ShadcnItemSize size,
        ShadcnItemMediaVariant mediaVariant,
        bool link) => builder =>
    {
        builder.OpenElement(0, "section");
        builder.AddAttribute(1, "class", "showcase-item-dossier");
        builder.AddAttribute(2, "aria-labelledby", "showcase-item-title");
        builder.OpenElement(3, "header");
        builder.AddAttribute(4, "class", "showcase-item-dossier__header");
        builder.OpenElement(5, "div");
        builder.OpenElement(6, "h3");
        builder.AddAttribute(7, "id", "showcase-item-title");
        builder.AddContent(8, "Production files");
        builder.CloseElement();
        builder.OpenElement(9, "p");
        builder.AddContent(10, "Review the latest references before releasing the drawing package.");
        builder.CloseElement();
        builder.CloseElement();
        builder.OpenElement(11, "span");
        builder.AddContent(12, $"{ItemFiles.Length} files");
        builder.CloseElement();
        builder.CloseElement();
        builder.OpenComponent<ShadcnItemGroup>(13);
        builder.AddAttribute(14, nameof(ShadcnItemGroup.Class), "showcase-item-list");
        builder.AddAttribute(15, nameof(ShadcnItemGroup.ChildContent), (RenderFragment)(group =>
        {
            for (var index = 0; index < ItemFiles.Length; index++)
                AddItem(group, index * 20, ItemFiles[index], variant, size, mediaVariant, link);
        }));
        builder.CloseComponent();
        builder.CloseElement();
    };

    private static void AddItem(
        RenderTreeBuilder builder,
        int sequence,
        ItemExampleFile file,
        ShadcnItemVariant variant,
        ShadcnItemSize size,
        ShadcnItemMediaVariant mediaVariant,
        bool link)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "id", $"item-{file.Id}");
        builder.AddAttribute(sequence + 2, "role", "listitem");
        builder.OpenComponent<ShadcnItem>(sequence + 3);
        builder.AddAttribute(sequence + 4, nameof(ShadcnItem.Variant), variant);
        builder.AddAttribute(sequence + 5, nameof(ShadcnItem.Size), size);
        builder.AddAttribute(sequence + 6, nameof(ShadcnItem.Href), link ? $"#item-{file.Id}" : null);
        builder.AddAttribute(sequence + 7, nameof(ShadcnItem.ChildContent), ItemContent(file, mediaVariant));
        builder.CloseComponent();
        builder.CloseElement();
    }

    private static RenderFragment ItemContent(ItemExampleFile file, ShadcnItemMediaVariant mediaVariant) => builder =>
    {
        builder.OpenComponent<ShadcnItemMedia>(0);
        builder.AddAttribute(1, nameof(ShadcnItemMedia.Variant), mediaVariant);
        builder.AddAttribute(2, nameof(ShadcnItemMedia.ChildContent), ItemMedia(file, mediaVariant));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnItemContent>(3);
        builder.AddAttribute(4, nameof(ShadcnItemContent.ChildContent), (RenderFragment)(content =>
        {
            content.OpenComponent<ShadcnItemTitle>(0);
            content.AddAttribute(1, nameof(ShadcnItemTitle.ChildContent), Text(file.Name));
            content.CloseComponent();
            content.OpenComponent<ShadcnItemDescription>(2);
            content.AddAttribute(3, nameof(ShadcnItemDescription.ChildContent), Text(file.Description));
            content.CloseComponent();
        }));
        builder.CloseComponent();
        builder.OpenComponent<ShadcnItemActions>(5);
        builder.AddAttribute(6, nameof(ShadcnItemActions.ChildContent), (RenderFragment)(actions =>
        {
            actions.OpenComponent<ShadcnBadge>(0);
            actions.AddAttribute(1, nameof(ShadcnBadge.Variant), file.BadgeVariant);
            actions.AddAttribute(2, nameof(ShadcnBadge.ChildContent), Text(file.Status));
            actions.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static RenderFragment ItemMedia(ItemExampleFile file, ShadcnItemMediaVariant mediaVariant) => builder =>
    {
        if (mediaVariant == ShadcnItemMediaVariant.Image)
        {
            builder.OpenElement(0, "img");
            builder.AddAttribute(1, "src", file.ImageSource);
            builder.AddAttribute(2, "alt", file.ImageAlt);
            builder.AddAttribute(3, "loading", "lazy");
            builder.CloseElement();
            return;
        }

        builder.AddContent(0, FileIcon());
    };

    private static RenderFragment KeyboardReference(string platform) => builder =>
    {
        var modifier = platform == "macOS" ? "⌘" : "Ctrl";
        var modifierName = platform == "macOS" ? "Command" : "Control";

        builder.OpenComponent<ShadcnCard>(0);
        builder.AddAttribute(1, nameof(ShadcnCard.Size), ShadcnCardSize.Small);
        builder.AddAttribute(2, nameof(ShadcnCard.ChildContent), (RenderFragment)(card =>
        {
            card.OpenComponent<ShadcnCardHeader>(0);
            card.AddAttribute(1, nameof(ShadcnCardHeader.ChildContent), (RenderFragment)(header =>
            {
                header.OpenComponent<ShadcnCardTitle>(0);
                header.AddAttribute(1, nameof(ShadcnCardTitle.ChildContent), Text("Command shortcuts"));
                header.AddAttribute(2, "dir", "auto");
                header.CloseComponent();
                header.OpenComponent<ShadcnCardDescription>(3);
                header.AddAttribute(4, nameof(ShadcnCardDescription.ChildContent), Text($"Common shortcuts for {platform}."));
                header.AddAttribute(5, "dir", "auto");
                header.CloseComponent();
            }));
            card.CloseComponent();

            card.OpenComponent<ShadcnCardContent>(2);
            card.AddAttribute(3, nameof(ShadcnCardContent.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<ShadcnItemGroup>(0);
                content.AddAttribute(1, nameof(ShadcnItemGroup.ChildContent), (RenderFragment)(items =>
                {
                    AddShortcut(items, 0, "Close current dialog", ["Esc"], "Escape");
                    AddShortcut(items, 10, "Open command palette", [modifier, "K"], $"{modifierName} K");
                    AddShortcut(items, 20, "Search commands", [modifier, "Shift", "P"], $"{modifierName} Shift P");
                }));
                content.CloseComponent();
            }));
            card.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static string KeyboardReferenceSource(string platform)
    {
        var modifier = platform == "macOS" ? "⌘" : "Ctrl";
        var modifierName = platform == "macOS" ? "Command" : "Control";
        return $"""
<ShadcnCard Size="ShadcnCardSize.Small">
    <ShadcnCardHeader>
        <ShadcnCardTitle dir="auto">Command shortcuts</ShadcnCardTitle>
        <ShadcnCardDescription dir="auto">Common shortcuts for {platform}.</ShadcnCardDescription>
    </ShadcnCardHeader>
    <ShadcnCardContent>
        <ShadcnItemGroup>
            <ShadcnItem Size="ShadcnItemSize.Small">
                <ShadcnItemContent>
                    <ShadcnItemTitle dir="auto">Close current dialog</ShadcnItemTitle>
                </ShadcnItemContent>
                <ShadcnItemActions>
                    <ShadcnKbdGroup aria-label="Escape">
                        <ShadcnKbd>Esc</ShadcnKbd>
                    </ShadcnKbdGroup>
                </ShadcnItemActions>
            </ShadcnItem>
            <ShadcnItem Size="ShadcnItemSize.Small">
                <ShadcnItemContent>
                    <ShadcnItemTitle dir="auto">Open command palette</ShadcnItemTitle>
                </ShadcnItemContent>
                <ShadcnItemActions>
                    <ShadcnKbdGroup aria-label="{modifierName} K">
                        <ShadcnKbd>{modifier}</ShadcnKbd><span aria-hidden="true">+</span><ShadcnKbd>K</ShadcnKbd>
                    </ShadcnKbdGroup>
                </ShadcnItemActions>
            </ShadcnItem>
            <ShadcnItem Size="ShadcnItemSize.Small">
                <ShadcnItemContent>
                    <ShadcnItemTitle dir="auto">Search commands</ShadcnItemTitle>
                </ShadcnItemContent>
                <ShadcnItemActions>
                    <ShadcnKbdGroup aria-label="{modifierName} Shift P">
                        <ShadcnKbd>{modifier}</ShadcnKbd><span aria-hidden="true">+</span><ShadcnKbd>Shift</ShadcnKbd><span aria-hidden="true">+</span><ShadcnKbd>P</ShadcnKbd>
                    </ShadcnKbdGroup>
                </ShadcnItemActions>
            </ShadcnItem>
        </ShadcnItemGroup>
    </ShadcnCardContent>
</ShadcnCard>
""";
    }

    private static RenderFragment FileIcon() => builder =>
    {
        builder.OpenElement(0, "svg"); builder.AddAttribute(1, "viewBox", "0 0 24 24"); builder.AddAttribute(2, "aria-hidden", "true"); builder.OpenElement(3, "path"); builder.AddAttribute(4, "d", "M6 3h8l4 4v14H6z"); builder.CloseElement(); builder.OpenElement(5, "path"); builder.AddAttribute(6, "d", "M14 3v5h5M9 13h6M9 17h4"); builder.CloseElement(); builder.CloseElement();
    };

    private static string BuildItemSource(
        ShadcnItemVariant variant,
        ShadcnItemSize size,
        ShadcnItemMediaVariant mediaVariant,
        bool link)
    {
        var items = string.Join(
            Environment.NewLine,
            ItemFiles.Select(file => BuildItemSource(file, variant, size, mediaVariant, link)));

        return $"""
<section class="showcase-item-dossier" aria-labelledby="showcase-item-title">
    <header class="showcase-item-dossier__header">
        <div>
            <h3 id="showcase-item-title">Production files</h3>
            <p>Review the latest references before releasing the drawing package.</p>
        </div>
        <span>{ItemFiles.Length} files</span>
    </header>
    <ShadcnItemGroup Class="showcase-item-list">
{items}
    </ShadcnItemGroup>
</section>
""";
    }

    private static string BuildItemSource(
        ItemExampleFile file,
        ShadcnItemVariant variant,
        ShadcnItemSize size,
        ShadcnItemMediaVariant mediaVariant,
        bool link)
    {
        var href = link ? $" Href=\"#item-{file.Id}\"" : string.Empty;
        var media = mediaVariant == ShadcnItemMediaVariant.Image
            ? $"<img src=\"{file.ImageSource}\" alt=\"{file.ImageAlt}\" loading=\"lazy\" />"
            : "<svg aria-hidden=\"true\" viewBox=\"0 0 24 24\"><path d=\"M6 3h8l4 4v14H6z\" /><path d=\"M14 3v5h5M9 13h6M9 17h4\" /></svg>";

        return $"""
        <div id="item-{file.Id}" role="listitem">
            <ShadcnItem Variant="ShadcnItemVariant.{variant}" Size="ShadcnItemSize.{size}"{href}>
                <ShadcnItemMedia Variant="ShadcnItemMediaVariant.{mediaVariant}">
                    {media}
                </ShadcnItemMedia>
                <ShadcnItemContent>
                    <ShadcnItemTitle>{file.Name}</ShadcnItemTitle>
                    <ShadcnItemDescription>{file.Description}</ShadcnItemDescription>
                </ShadcnItemContent>
                <ShadcnItemActions>
                    <ShadcnBadge Variant="ShadcnBadgeVariant.{file.BadgeVariant}">{file.Status}</ShadcnBadge>
                </ShadcnItemActions>
            </ShadcnItem>
        </div>
""";
    }

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
            builder.AddAttribute(3, "dir", "auto");
            builder.OpenComponent<ShadcnEmpty>(4);
            builder.AddAttribute(5, nameof(ShadcnEmpty.ChildContent), EmptyContent());
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

    private static void AddShortcut(
        RenderTreeBuilder builder,
        int sequence,
        string label,
        IReadOnlyList<string> keys,
        string accessibleName)
    {
        builder.OpenComponent<ShadcnItem>(sequence);
        builder.AddAttribute(sequence + 1, nameof(ShadcnItem.Size), ShadcnItemSize.Small);
        builder.AddAttribute(sequence + 2, nameof(ShadcnItem.ChildContent), (RenderFragment)(item =>
        {
            item.OpenComponent<ShadcnItemContent>(0);
            item.AddAttribute(1, nameof(ShadcnItemContent.ChildContent), (RenderFragment)(itemContent =>
            {
                itemContent.OpenComponent<ShadcnItemTitle>(0);
                itemContent.AddAttribute(1, nameof(ShadcnItemTitle.ChildContent), Text(label));
                itemContent.AddAttribute(2, "dir", "auto");
                itemContent.CloseComponent();
            }));
            item.CloseComponent();

            item.OpenComponent<ShadcnItemActions>(2);
            item.AddAttribute(3, nameof(ShadcnItemActions.ChildContent), (RenderFragment)(actions =>
            {
                actions.OpenComponent<ShadcnKbdGroup>(0);
                actions.AddAttribute(1, "aria-label", accessibleName);
                actions.AddAttribute(2, nameof(ShadcnKbdGroup.ChildContent), (RenderFragment)(group =>
                {
                    for (var index = 0; index < keys.Count; index++)
                    {
                        if (index > 0)
                        {
                            group.OpenElement(index * 10, "span");
                            group.AddAttribute(index * 10 + 1, "aria-hidden", "true");
                            group.AddContent(index * 10 + 2, "+");
                            group.CloseElement();
                        }

                        group.OpenComponent<ShadcnKbd>(index * 10 + 3);
                        group.AddAttribute(index * 10 + 4, nameof(ShadcnKbd.ChildContent), Text(keys[index]));
                        group.CloseComponent();
                    }
                }));
                actions.CloseComponent();
            }));
            item.CloseComponent();
        }));
        builder.CloseComponent();
    }

    private static readonly ItemExampleFile[] ItemFiles =
    [
        new(
            "workspace-plan",
            "workspace-plan.png",
            "Workspace layout · Revision C · 2.0 MB",
            "images/attachments/workspace-plan.png",
            "Preview of the workspace layout",
            "Approved",
            ShadcnBadgeVariant.Secondary),
        new(
            "desk-reference",
            "desk-reference.png",
            "Reference image · Reviewed 8 minutes ago · 2.0 MB",
            "images/attachments/desk-reference.png",
            "Preview of the desk reference",
            "Reviewed",
            ShadcnBadgeVariant.Outline),
        new(
            "office-reference",
            "office-reference.png",
            "Reference image · Awaiting review · 1.8 MB",
            "images/attachments/office-reference.png",
            "Preview of the office reference",
            "Pending",
            ShadcnBadgeVariant.Ghost)
    ];

    private sealed record ItemExampleFile(
        string Id,
        string Name,
        string Description,
        string ImageSource,
        string ImageAlt,
        string Status,
        ShadcnBadgeVariant BadgeVariant);
    private static void AddSeparatorDetail(
        RenderTreeBuilder builder,
        int sequence,
        string label,
        string value,
        string detail)
    {
        builder.OpenElement(sequence, "div");
        builder.AddAttribute(sequence + 1, "class", "showcase-separator-demo__section");
        builder.OpenElement(sequence + 2, "span");
        builder.AddContent(sequence + 3, label);
        builder.CloseElement();
        builder.OpenElement(sequence + 4, "strong");
        builder.OpenElement(sequence + 5, "bdi");
        builder.AddContent(sequence + 6, value);
        builder.CloseElement();
        builder.CloseElement();
        builder.OpenElement(sequence + 7, "small");
        builder.OpenElement(sequence + 8, "bdi");
        builder.AddContent(sequence + 9, detail);
        builder.CloseElement();
        builder.CloseElement();
        builder.CloseElement();
    }
}
