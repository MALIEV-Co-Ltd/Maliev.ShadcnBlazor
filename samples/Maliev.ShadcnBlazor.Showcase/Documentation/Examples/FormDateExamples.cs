using System.Globalization;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Maliev.ShadcnBlazor.Showcase.Components.Documentation;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class FormDateExamples
{
    private static readonly IReadOnlyList<ShadcnSelectOption<string>> Processes =
    [new("cnc", "CNC machining", "Subtractive"), new("slm", "Metal 3D printing", "Additive"), new("disabled", "Unavailable", "Other", true)];
    private static readonly IReadOnlyList<ShadcnComboboxOption<string>> Materials =
    [new("al6061", "Aluminum 6061", "Metals"), new("ss316", "Stainless 316L", "Metals"), new("peek", "PEEK", "Polymers")];

    public static IReadOnlyList<ComponentExampleDefinition> Create(string slug) => slug switch
    {
        "input" => [Input()],
        "textarea" => [Textarea()],
        "native-select" => [NativeSelect()],
        "input-group" => [InputGroup()],
        "input-otp" => [InputOtp()],
        "select" => [Select()],
        "combobox" => [Combobox()],
        "calendar" => [Calendar()],
        "date-picker" => [DatePicker()],
        _ => []
    };

    private static ComponentExampleDefinition Input()
    {
        var invalid = false; var type = "text"; var value = "ชิ้นส่วน";
        RenderFragment preview = b => { b.OpenComponent<ShadcnInput<string>>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => value = next)); b.AddAttribute(3, "Type", type); b.AddAttribute(4, "Invalid", invalid); b.AddAttribute(5, "Required", true); b.AddAttribute(6, "AdditionalAttributes", Attr("forms-dossier-input", "Part name")); b.CloseComponent(); };
        string Source() => $"<ShadcnInput TValue=\"string\" @bind-Value=\"PartName\" Type=\"{type}\" Required=\"true\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" />";
        return Example("input", "Typed input", "Exercise typed binding, native input modes, required and invalid states.", Source(), preview,
            [
                Toggle("input-invalid", "Invalid", v => invalid = v),
                Select("input-type", "Type", type, ["text", "search", "file"], v => type = v)
            ], ["typed-binding", "required", "file", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition Textarea()
    {
        var invalid = false; var rows = 3; var value = "รายละเอียดการผลิต";
        RenderFragment preview = b => { b.OpenComponent<ShadcnTextarea<string>>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => value = next)); b.AddAttribute(3, "Rows", rows); b.AddAttribute(4, "Invalid", invalid); b.AddAttribute(5, "AdditionalAttributes", Attr("forms-dossier-textarea", "Manufacturing notes")); b.CloseComponent(); };
        string Source() => $"<ShadcnTextarea TValue=\"string\" @bind-Value=\"Notes\" Rows=\"{rows}\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" />";
        return Example("textarea", "Multiline typed input", "Resize a native multiline control while preserving validation and form binding.", Source(), preview,
            [
                Toggle("textarea-invalid", "Invalid", v => invalid = v),
                Select("textarea-rows", "Rows", rows.ToString(), ["3", "5"], v => rows = int.Parse(v, CultureInfo.InvariantCulture))
            ], ["typed-binding", "rows", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition NativeSelect()
    {
        var invalid = false; var readOnly = false; var value = "standard";
        RenderFragment preview = b => { b.OpenComponent<ShadcnNativeSelect<string>>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => value = next)); b.AddAttribute(3, "Invalid", invalid); b.AddAttribute(4, "ReadOnly", readOnly); b.AddAttribute(5, "AdditionalAttributes", Attr("forms-dossier-native-select", "Priority")); b.AddAttribute(6, "ChildContent", (RenderFragment)(c => { AddNativeOption(c, 0, "standard", "Standard"); AddNativeOption(c, 10, "urgent", "Urgent"); })); b.CloseComponent(); };
        string Source() => $"<ShadcnNativeSelect TValue=\"string\" @bind-Value=\"Priority\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" ReadOnly=\"{readOnly.ToString().ToLowerInvariant()}\">\n    <ShadcnNativeSelectOption Value=\"standard\">Standard</ShadcnNativeSelectOption>\n    <ShadcnNativeSelectOption Value=\"urgent\">Urgent</ShadcnNativeSelectOption>\n</ShadcnNativeSelect>";
        return Example("native-select", "Native select", "Compare compact size, real option semantics, and focusable read-only restoration.", Source(), preview,
            [
                Toggle("native-select-invalid", "Invalid", v => invalid = v),
                Toggle("native-select-readonly", "Read only", v => readOnly = v)
            ], ["selected", "read-only", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition InputGroup()
    {
        var invalid = false; var alignment = ShadcnInputGroupAlignment.InlineStart; var amount = 1250m;
        RenderFragment preview = b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "showcase-input-group-dossier");
            b.OpenComponent<ShadcnInputGroup>(2);
            b.AddAttribute(3, "AdditionalAttributes", invalid ? new Dictionary<string, object> { ["data-testid"] = "forms-dossier-input-group", ["aria-label"] = "Budget", ["aria-invalid"] = "true" } : Attr("forms-dossier-input-group", "Budget"));
            b.AddAttribute(4, "ChildContent", (RenderFragment)(c =>
            {
                c.OpenComponent<ShadcnInputGroupAddon>(0);
                c.AddAttribute(1, "Alignment", alignment);
                c.AddAttribute(2, "ChildContent", Text("THB"));
                c.CloseComponent();
                c.OpenComponent<ShadcnInput<decimal>>(10);
                c.AddAttribute(11, "Value", amount);
                c.AddAttribute(12, "ValueChanged", EventCallback.Factory.Create<decimal>(new object(), next => amount = next));
                c.AddAttribute(13, "Type", "number");
                c.AddAttribute(14, "Invalid", invalid);
                c.AddAttribute(15, "AdditionalAttributes", new Dictionary<string, object> { ["aria-label"] = "Budget" });
                c.CloseComponent();
            }));
            b.CloseComponent();
            b.CloseElement();
        };
        string Source() => $"<ShadcnInputGroup>\n    <ShadcnInputGroupAddon Alignment=\"{alignment}\">THB</ShadcnInputGroupAddon>\n    <ShadcnInput TValue=\"decimal\" @bind-Value=\"Budget\" Type=\"number\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" aria-label=\"Budget\" />\n</ShadcnInputGroup>";
        return Example("input-group", "Input group composition", "Move semantic text and actions across logical addon positions.", Source(), preview,
            [
                Toggle("input-group-invalid", "Invalid", v => invalid = v),
                EnumSelect("input-group-alignment", "Alignment", alignment, v => alignment = v)
            ], ["addons", "inline", "block", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition InputOtp()
    {
        var invalid = false; var numeric = true; var code = "123";
        RenderFragment preview = b =>
        {
            b.OpenComponent<ShadcnCard>(0);
            b.AddAttribute(1, "Class", "showcase-otp-card");
            b.AddAttribute(2, "ChildContent", (RenderFragment)(card =>
            {
                card.OpenComponent<ShadcnCardHeader>(0);
                card.AddAttribute(1, "ChildContent", (RenderFragment)(header =>
                {
                    header.OpenComponent<ShadcnCardTitle>(0);
                    header.AddAttribute(1, "ChildContent", Text("Verify your email"));
                    header.CloseComponent();
                    header.OpenComponent<ShadcnCardDescription>(2);
                    header.AddAttribute(3, "ChildContent", Text("Enter the 6-digit code we sent to your inbox."));
                    header.CloseComponent();
                }));
                card.CloseComponent();
                card.OpenComponent<ShadcnCardContent>(10);
                card.AddAttribute(11, "ChildContent", (RenderFragment)(content =>
                {
                    content.OpenComponent<ShadcnInputOtp>(0);
                    content.AddAttribute(1, "Value", code);
                    content.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => code = next));
                    content.AddAttribute(3, "MaxLength", 6);
                    content.AddAttribute(4, "Pattern", numeric ? "[0-9]" : null);
                    content.AddAttribute(5, "InputMode", numeric ? "numeric" : "text");
                    content.AddAttribute(6, "Invalid", invalid);
                    content.AddAttribute(7, "AdditionalAttributes", Attr("forms-dossier-input-otp", "Verification code"));
                    content.AddAttribute(8, "ChildContent", OtpSlots());
                    content.CloseComponent();
                }));
                card.CloseComponent();
                card.OpenComponent<ShadcnCardFooter>(20);
                card.AddAttribute(21, "ChildContent", Text("Code expires in 10 minutes"));
                card.CloseComponent();
            }));
            b.CloseComponent();
        };
        string Source() => $"<ShadcnCard>\n    <ShadcnCardHeader>\n        <ShadcnCardTitle>Verify your email</ShadcnCardTitle>\n        <ShadcnCardDescription>Enter the 6-digit code we sent to your inbox.</ShadcnCardDescription>\n    </ShadcnCardHeader>\n    <ShadcnCardContent>\n        <ShadcnInputOtp @bind-Value=\"Code\" MaxLength=\"6\" Pattern=\"{(numeric ? "[0-9]" : "[\\p{{L}}]")}\" InputMode=\"{(numeric ? "numeric" : "text")}\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\">\n            <ShadcnInputOtpGroup>\n                <ShadcnInputOtpSlot Index=\"0\" />\n                <ShadcnInputOtpSlot Index=\"1\" />\n                <ShadcnInputOtpSlot Index=\"2\" />\n                <ShadcnInputOtpSlot Index=\"3\" />\n                <ShadcnInputOtpSlot Index=\"4\" />\n                <ShadcnInputOtpSlot Index=\"5\" />\n            </ShadcnInputOtpGroup>\n        </ShadcnInputOtp>\n    </ShadcnCardContent>\n    <ShadcnCardFooter>Code expires in 10 minutes</ShadcnCardFooter>\n</ShadcnCard>";
        return Example("input-otp", "One-input OTP", "Verify paste filtering, caret-driven slots, Thai graphemes, and mobile input modes.", Source(), preview,
            [
                Toggle("input-otp-invalid", "Invalid", v => invalid = v),
                Toggle("input-otp-numeric", "Numeric", v => { numeric = v; code = numeric ? "123" : "ก้ข"; }, true)
            ], ["one-input", "graphemes", "numeric", "invalid"]) with
        { RazorSourceProvider = Source };

        RenderFragment OtpSlots() => c =>
        {
            c.OpenComponent<ShadcnInputOtpGroup>(0);
            c.AddAttribute(1, "ChildContent", (RenderFragment)(slots =>
            {
                for (var i = 0; i < 6; i++)
                {
                    slots.OpenComponent<ShadcnInputOtpSlot>(i * 2);
                    slots.AddAttribute(i * 2 + 1, "Index", i);
                    slots.CloseComponent();
                }
            }));
            c.CloseComponent();
        };
    }
    private static ComponentExampleDefinition Select()
    {
        var invalid = false;
        RenderFragment preview = b => { b.OpenComponent<SelectDossierPreview>(0); b.AddAttribute(1, nameof(SelectDossierPreview.Invalid), invalid); b.CloseComponent(); };
        string Source() => $$"""
            <ShadcnSelect TValue="string"
                          @bind-Value="Process"
                          Options="ProcessOptions"
                          Placeholder="Select a process"
                          Clearable="true"
                          Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                          aria-label="Manufacturing process" />

            @code {
                private string Process { get; set; } = "cnc";

                private static readonly IReadOnlyList<ShadcnSelectOption<string>> ProcessOptions =
                [
                    new("cnc", "CNC machining", "Subtractive"),
                    new("slm", "Metal 3D printing", "Additive"),
                    new("disabled", "Unavailable", "Other", Disabled: true)
                ];
            }
            """;
        return Example("select", "Grouped select", "Choose a manufacturing process from a keyboard-friendly grouped listbox, then clear or change the selection directly in the preview.", Source(), preview,
            [
                Toggle("select-invalid", "Invalid", v => invalid = v)
            ], ["selected", "groups", "clearable", "open", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition Combobox()
    {
        var invalid = false; var multiple = false;
        RenderFragment preview = b => { b.OpenComponent<ComboboxDossierPreview>(0); b.AddAttribute(1, "Multiple", multiple); b.AddAttribute(2, "Invalid", invalid); b.CloseComponent(); };
        string Source() => $$"""
            @code {
                private static readonly IReadOnlyList<ShadcnComboboxOption<string>> MaterialOptions =
                [
                    new("al6061", "Aluminum 6061", "Metals"),
                    new("ss316", "Stainless 316L", "Metals"),
                    new("peek", "PEEK", "Polymers")
                ];
            }

            <ShadcnCombobox TValue="string"
                {{(multiple ? "@bind-Values=\"SelectedMaterials\"" : "@bind-Value=\"SelectedMaterial\"")}}
                @bind-Open="IsOpen"
                @bind-Query="Query"
                Options="MaterialOptions"
                Multiple="{{multiple.ToString().ToLowerInvariant()}}"
                ShowClear="true"
                ShowTrigger="true"
                Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                Placeholder="Select a material"
                aria-label="Material" />
            """;
        return Example("combobox", "Searchable typed combobox", "Search grouped materials, select with the pointer or keyboard, and try the clear and multi-select states in the field itself.", Source(), preview,
            [
                Toggle("combobox-invalid", "Invalid", v => invalid = v),
                Toggle("combobox-multiple", "Multiple", v => multiple = v)
            ], ["selected", "multiple", "chips", "open", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition Calendar()
    {
        var invalid = false; var mode = ShadcnCalendarSelectionMode.Single;
        RenderFragment preview = b => { b.OpenComponent<CalendarDossierPreview>(0); b.AddAttribute(1, "Mode", mode); b.AddAttribute(2, "Invalid", invalid); b.CloseComponent(); };
        string Source() => mode == ShadcnCalendarSelectionMode.Range
            ? $"<ShadcnCalendar Mode=\"ShadcnCalendarSelectionMode.Range\" @bind-Range=\"Window\" Culture=\"ThaiCulture\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" />"
            : $"<ShadcnCalendar Mode=\"ShadcnCalendarSelectionMode.Single\" @bind-Value=\"SelectedDate\" Culture=\"ThaiCulture\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" />";
        return Example("calendar", "DateOnly calendar", "Switch single/range selection in a deterministic Thai-localized keyboard grid.", Source(), preview,
            [
                Toggle("calendar-invalid", "Invalid", v => invalid = v),
                EnumSelect("calendar-mode", "Mode", mode, v => mode = v)
            ], ["single", "range", "culture", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition DatePicker()
    {
        var invalid = false; var clearable = true; var mode = ShadcnCalendarSelectionMode.Range;
        RenderFragment preview = b => { b.OpenComponent<CompactDatePickerDossierPreview>(0); b.AddAttribute(1, nameof(CompactDatePickerDossierPreview.Mode), mode); b.AddAttribute(2, nameof(CompactDatePickerDossierPreview.Clearable), clearable); b.AddAttribute(3, nameof(CompactDatePickerDossierPreview.Invalid), invalid); b.CloseComponent(); };
        string Source() => DatePickerSource(mode, clearable, invalid);
        return Example("date-picker", "Delivery date picker", "Open one field to choose a delivery date or connected date range, then clear the selection without a duplicate text control.", Source(), preview,
            [
                EnumSelect("date-picker-mode", "Selection", mode, v => mode = v),
                Toggle("date-picker-invalid", "Invalid", v => invalid = v),
                Toggle("date-picker-clearable", "Clearable", v => clearable = v, true)
            ], ["single", "range", "calendar", "culture", "clearable", "invalid"]) with
        { RazorSourceProvider = Source };
    }

    private static string DatePickerSource(ShadcnCalendarSelectionMode mode, bool clearable, bool invalid)
    {
        var binding = mode == ShadcnCalendarSelectionMode.Range ? "@bind-Range=\"DeliveryWindow\"" : "@bind-Value=\"SelectedDate\"";
        var placeholder = mode == ShadcnCalendarSelectionMode.Range ? "Pick a delivery window" : "Pick a delivery date";
        return $$"""
            @using System.Globalization
            @using Maliev.ShadcnBlazor.Components.Forms

            <ShadcnDatePicker Mode="ShadcnCalendarSelectionMode.{{mode}}"
                              {{binding}}
                              @bind-Open="DatePickerOpen"
                              @bind-VisibleMonth="VisibleMonth"
                              Culture="ThaiCulture"
                              Today="Today"
                              Placeholder="{{placeholder}}"
                              Clearable="{{clearable.ToString().ToLowerInvariant()}}"
                              Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                              aria-label="Delivery date" />

            @code {
                private DateOnly? SelectedDate { get; set; } = new(2026, 8, 13);
                private ShadcnDateRange? DeliveryWindow { get; set; } = new(new(2026, 8, 10), new(2026, 8, 13));
                private DateOnly VisibleMonth { get; set; } = new(2026, 8, 1);
                private bool DatePickerOpen { get; set; }
                private static readonly DateOnly Today = new(2026, 8, 13);
                private static readonly CultureInfo ThaiCulture = CultureInfo.GetCultureInfo("th-TH");
            }
            """;
    }

    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Select(string id, string label, string value, IReadOnlyList<string> options, Action<string> apply) => new(id, label, ComponentParameterControlKind.Select, value, options, apply);
    private static ComponentParameterControl EnumSelect<T>(string id, string label, T value, Action<T> apply) where T : struct, Enum => new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<T>(), text => apply(Enum.Parse<T>(text)));
    private static IReadOnlyDictionary<string, object> Attr(string testId, string label) => new Dictionary<string, object> { ["data-testid"] = testId, ["aria-label"] = label };
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddNativeOption(RenderTreeBuilder b, int sequence, string value, string text) { b.OpenComponent<ShadcnNativeSelectOption<string>>(sequence); b.AddAttribute(sequence + 1, "Value", value); b.AddAttribute(sequence + 2, "ChildContent", Text(text)); b.CloseComponent(); }

    private sealed class CompactDatePickerDossierPreview : ComponentBase
    {
        [Parameter] public ShadcnCalendarSelectionMode Mode { get; set; } = ShadcnCalendarSelectionMode.Range;
        [Parameter] public bool Invalid { get; set; }
        [Parameter] public bool Clearable { get; set; } = true;

        private DateOnly? SelectedDate { get; set; } = new(2026, 8, 13);
        private ShadcnDateRange? Range { get; set; } = new(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13));
        private DateOnly VisibleMonth { get; set; } = new(2026, 8, 1);
        private bool Open { get; set; }
        private static readonly DateOnly Today = new(2026, 8, 13);
        private static readonly CultureInfo ThaiCulture = CultureInfo.GetCultureInfo("th-TH");

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-date-picker-dossier");
            builder.OpenComponent<ShadcnDatePicker>(2);
            builder.AddAttribute(3, nameof(ShadcnDatePicker.Mode), Mode);
            if (Mode == ShadcnCalendarSelectionMode.Range)
            {
                builder.AddAttribute(4, nameof(ShadcnDatePicker.Range), Range);
                builder.AddAttribute(5, nameof(ShadcnDatePicker.RangeChanged), EventCallback.Factory.Create<ShadcnDateRange?>(this, HandleRangeChanged));
            }
            else
            {
                builder.AddAttribute(4, nameof(ShadcnDatePicker.Value), SelectedDate);
                builder.AddAttribute(5, nameof(ShadcnDatePicker.ValueChanged), EventCallback.Factory.Create<DateOnly?>(this, HandleValueChanged));
            }
            builder.AddAttribute(6, nameof(ShadcnDatePicker.Open), Open);
            builder.AddAttribute(7, nameof(ShadcnDatePicker.OpenChanged), EventCallback.Factory.Create<bool>(this, HandleOpenChanged));
            builder.AddAttribute(8, nameof(ShadcnDatePicker.VisibleMonth), VisibleMonth);
            builder.AddAttribute(9, nameof(ShadcnDatePicker.VisibleMonthChanged), EventCallback.Factory.Create<DateOnly>(this, HandleMonthChanged));
            builder.AddAttribute(10, nameof(ShadcnDatePicker.Culture), ThaiCulture);
            builder.AddAttribute(11, nameof(ShadcnDatePicker.Today), Today);
            builder.AddAttribute(12, nameof(ShadcnDatePicker.Clearable), Clearable);
            builder.AddAttribute(13, nameof(ShadcnDatePicker.Invalid), Invalid);
            builder.AddAttribute(14, nameof(ShadcnDatePicker.AdditionalAttributes), Attr("forms-dossier-date-picker", "Delivery date"));
            builder.AddAttribute(15, nameof(ShadcnDatePicker.Placeholder), Mode == ShadcnCalendarSelectionMode.Range ? "Pick a delivery window" : "Pick a delivery date");
            builder.CloseComponent();
            builder.CloseElement();
        }

        private Task HandleRangeChanged(ShadcnDateRange? value)
        {
            Range = value;
            return Task.CompletedTask;
        }

        private Task HandleValueChanged(DateOnly? value)
        {
            SelectedDate = value;
            return Task.CompletedTask;
        }

        private Task HandleOpenChanged(bool value)
        {
            Open = value;
            return Task.CompletedTask;
        }

        private Task HandleMonthChanged(DateOnly value)
        {
            VisibleMonth = value;
            return Task.CompletedTask;
        }
    }

    private sealed class SelectDossierPreview : ComponentBase
    {
        [Parameter] public bool Invalid { get; set; }

        private string Value { get; set; } = "cnc";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-select-dossier");
            builder.OpenComponent<ShadcnSelect<string>>(2);
            builder.AddAttribute(3, "Value", Value);
            builder.AddAttribute(4, "ValueChanged", EventCallback.Factory.Create<string>(this, HandleValueChanged));
            builder.AddAttribute(5, "Options", Processes);
            builder.AddAttribute(6, "Invalid", Invalid);
            builder.AddAttribute(7, "Clearable", true);
            builder.AddAttribute(8, "Placeholder", "Select a process");
            builder.AddAttribute(9, "AdditionalAttributes", Attr("forms-dossier-select", "Manufacturing process"));
            builder.CloseComponent();
            builder.CloseElement();
        }

        private Task HandleValueChanged(string value)
        {
            Value = value;
            return Task.CompletedTask;
        }

    }
}
