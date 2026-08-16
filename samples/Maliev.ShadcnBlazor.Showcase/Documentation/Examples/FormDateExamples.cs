using System.Globalization;
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
        return Example("input", "Typed input", "Exercise typed binding, native input modes, required and invalid states.", "<ShadcnInput TValue=\"string\" @bind-Value=\"PartName\" Required=\"true\" />", preview,
            [Toggle("input-invalid", "Invalid", v => invalid = v), Select("input-type", "Type", type, ["text", "search", "file"], v => type = v)], ["typed-binding", "required", "file", "invalid"]);
    }
    private static ComponentExampleDefinition Textarea()
    {
        var invalid = false; var rows = 3; var value = "รายละเอียดการผลิต";
        RenderFragment preview = b => { b.OpenComponent<ShadcnTextarea<string>>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => value = next)); b.AddAttribute(3, "Rows", rows); b.AddAttribute(4, "Invalid", invalid); b.AddAttribute(5, "AdditionalAttributes", Attr("forms-dossier-textarea", "Manufacturing notes")); b.CloseComponent(); };
        return Example("textarea", "Multiline typed input", "Resize a native multiline control while preserving validation and form binding.", "<ShadcnTextarea TValue=\"string\" @bind-Value=\"Notes\" Rows=\"3\" />", preview,
            [Toggle("textarea-invalid", "Invalid", v => invalid = v), Select("textarea-rows", "Rows", rows.ToString(), ["3", "5"], v => rows = int.Parse(v, CultureInfo.InvariantCulture))], ["typed-binding", "rows", "invalid"]);
    }
    private static ComponentExampleDefinition NativeSelect()
    {
        var invalid = false; var readOnly = false; var value = "standard";
        RenderFragment preview = b => { b.OpenComponent<ShadcnNativeSelect<string>>(0); b.AddAttribute(1, "Value", value); b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => value = next)); b.AddAttribute(3, "Invalid", invalid); b.AddAttribute(4, "ReadOnly", readOnly); b.AddAttribute(5, "AdditionalAttributes", Attr("forms-dossier-native-select", "Priority")); b.AddAttribute(6, "ChildContent", (RenderFragment)(c => { AddNativeOption(c, 0, "standard", "Standard"); AddNativeOption(c, 10, "urgent", "Urgent"); })); b.CloseComponent(); };
        return Example("native-select", "Native select", "Compare compact size, real option semantics, and focusable read-only restoration.", "<ShadcnNativeSelect TValue=\"string\" @bind-Value=\"Priority\"><ShadcnNativeSelectOption Value=\"standard\">Standard</ShadcnNativeSelectOption></ShadcnNativeSelect>", preview,
            [Toggle("native-select-invalid", "Invalid", v => invalid = v), Toggle("native-select-readonly", "Read only", v => readOnly = v)], ["selected", "read-only", "invalid"]);
    }
    private static ComponentExampleDefinition InputGroup()
    {
        var invalid = false; var alignment = ShadcnInputGroupAlignment.InlineStart; var amount = 1250m;
        RenderFragment preview = b => { b.OpenComponent<ShadcnInputGroup>(0); b.AddAttribute(1, "AdditionalAttributes", invalid ? new Dictionary<string, object> { ["data-testid"] = "forms-dossier-input-group", ["aria-label"] = "Budget", ["aria-invalid"] = "true" } : Attr("forms-dossier-input-group", "Budget")); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnInputGroupAddon>(0); c.AddAttribute(1, "Alignment", alignment); c.AddAttribute(2, "ChildContent", Text("THB")); c.CloseComponent(); c.OpenComponent<ShadcnInput<decimal>>(10); c.AddAttribute(11, "Value", amount); c.AddAttribute(12, "ValueChanged", EventCallback.Factory.Create<decimal>(new object(), next => amount = next)); c.AddAttribute(13, "Type", "number"); c.AddAttribute(14, "AdditionalAttributes", new Dictionary<string, object> { ["aria-label"] = "Budget" }); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("input-group", "Input group composition", "Move semantic text and actions across logical addon positions.", "<ShadcnInputGroup><ShadcnInputGroupAddon>THB</ShadcnInputGroupAddon><ShadcnInput TValue=\"decimal\" /></ShadcnInputGroup>", preview,
            [Toggle("input-group-invalid", "Invalid", v => invalid = v), EnumSelect("input-group-alignment", "Alignment", alignment, v => alignment = v)], ["addons", "inline", "block", "invalid"]);
    }
    private static ComponentExampleDefinition InputOtp()
    {
        var invalid = false; var numeric = true; var code = "123";
        RenderFragment preview = b => { b.OpenComponent<ShadcnInputOtp>(0); b.AddAttribute(1, "Value", code); b.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create<string>(new object(), next => code = next)); b.AddAttribute(3, "MaxLength", 6); b.AddAttribute(4, "Pattern", numeric ? "[0-9]" : null); b.AddAttribute(5, "InputMode", numeric ? "numeric" : "text"); b.AddAttribute(6, "Invalid", invalid); b.AddAttribute(7, "AdditionalAttributes", Attr("forms-dossier-input-otp", "Verification code")); b.AddAttribute(8, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnInputOtpGroup>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(s => { for (var i = 0; i < 6; i++) { s.OpenComponent<ShadcnInputOtpSlot>(i * 2); s.AddAttribute(i * 2 + 1, "Index", i); s.CloseComponent(); } })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("input-otp", "One-input OTP", "Verify paste filtering, caret-driven slots, Thai graphemes, and mobile input modes.", "<ShadcnInputOtp @bind-Value=\"Code\" MaxLength=\"6\" Pattern=\"[0-9]\"><ShadcnInputOtpGroup><ShadcnInputOtpSlot Index=\"0\" /><ShadcnInputOtpSlot Index=\"1\" /><ShadcnInputOtpSlot Index=\"2\" /><ShadcnInputOtpSlot Index=\"3\" /><ShadcnInputOtpSlot Index=\"4\" /><ShadcnInputOtpSlot Index=\"5\" /></ShadcnInputOtpGroup></ShadcnInputOtp>", preview,
            [Toggle("input-otp-invalid", "Invalid", v => invalid = v), Toggle("input-otp-numeric", "Numeric", v => { numeric = v; code = numeric ? "123" : "ก้ข"; }, true)], ["one-input", "graphemes", "numeric", "invalid"]);
    }
    private static ComponentExampleDefinition Select()
    {
        var invalid = false; var open = false;
        RenderFragment preview = b => { b.OpenComponent<SelectDossierPreview>(0); b.AddAttribute(1, nameof(SelectDossierPreview.Invalid), invalid); b.AddAttribute(2, nameof(SelectDossierPreview.Open), open); b.CloseComponent(); };
        return Example("select", "Grouped select", "Choose a manufacturing process from a keyboard-friendly grouped listbox, then clear or change the selection directly in the preview.", "<ShadcnSelect TValue=\"string\" @bind-Value=\"Process\" @bind-Open=\"IsOpen\" Options=\"ProcessOptions\" Clearable=\"true\" Invalid=\"HasError\" />", preview,
            [Toggle("select-invalid", "Invalid", v => invalid = v), Toggle("select-open", "Open", v => open = v)], ["selected", "groups", "clearable", "open", "invalid"]);
    }
    private static ComponentExampleDefinition Combobox()
    {
        var invalid = false; var multiple = false;
        RenderFragment preview = b => { b.OpenComponent<ComboboxDossierPreview>(0); b.AddAttribute(1, "Multiple", multiple); b.AddAttribute(2, "Invalid", invalid); b.CloseComponent(); };
        return Example("combobox", "Searchable typed combobox", "Search grouped materials, select with the pointer or keyboard, and try the clear and multi-select states in the field itself.", "<ShadcnCombobox TValue=\"string\" @bind-Value=\"SelectedMaterial\" @bind-Values=\"SelectedMaterials\" @bind-Open=\"IsOpen\" @bind-Query=\"Query\" Options=\"MaterialOptions\" Multiple=\"@AllowMultiple\" ShowClear=\"true\" ShowTrigger=\"true\" Invalid=\"@HasError\" Placeholder=\"Select a material\" />", preview,
            [Toggle("combobox-invalid", "Invalid", v => invalid = v), Toggle("combobox-multiple", "Multiple", v => multiple = v)], ["selected", "multiple", "chips", "open", "invalid"]);
    }
    private static ComponentExampleDefinition Calendar()
    {
        var invalid = false; var mode = ShadcnCalendarSelectionMode.Single;
        RenderFragment preview = b => { b.OpenComponent<CalendarDossierPreview>(0); b.AddAttribute(1, "Mode", mode); b.AddAttribute(2, "Invalid", invalid); b.CloseComponent(); };
        return Example("calendar", "DateOnly calendar", "Switch single/range selection in a deterministic Thai-localized keyboard grid.", "<ShadcnCalendar Mode=\"ShadcnCalendarSelectionMode.Range\" @bind-Range=\"Window\" Culture=\"ThaiCulture\" />", preview,
            [Toggle("calendar-invalid", "Invalid", v => invalid = v), EnumSelect("calendar-mode", "Mode", mode, v => mode = v)], ["single", "range", "culture", "invalid"]);
    }
    private static ComponentExampleDefinition DatePicker()
    {
        var invalid = false; var clearable = true;
        RenderFragment preview = b => { b.OpenComponent<DatePickerDossierPreview>(0); b.AddAttribute(1, "Clearable", clearable); b.AddAttribute(2, "Invalid", invalid); b.CloseComponent(); };
        return Example("date-picker", "Date picker composition", "Use culture-exact text entry, clearable trigger, DateOnly payload, and calendar popup.", "<ShadcnDatePicker @bind-Value=\"DeliveryDate\" AllowTextInput=\"true\" Culture=\"ThaiCulture\" />", preview,
            [Toggle("date-picker-invalid", "Invalid", v => invalid = v), Toggle("date-picker-clearable", "Clearable", v => clearable = v, true)], ["single", "calendar", "culture", "clearable", "invalid"]);
    }

    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Select(string id, string label, string value, IReadOnlyList<string> options, Action<string> apply) => new(id, label, ComponentParameterControlKind.Select, value, options, apply);
    private static ComponentParameterControl EnumSelect<T>(string id, string label, T value, Action<T> apply) where T : struct, Enum => new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<T>(), text => apply(Enum.Parse<T>(text)));
    private static IReadOnlyDictionary<string, object> Attr(string testId, string label) => new Dictionary<string, object> { ["data-testid"] = testId, ["aria-label"] = label };
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddNativeOption(RenderTreeBuilder b, int sequence, string value, string text) { b.OpenComponent<ShadcnNativeSelectOption<string>>(sequence); b.AddAttribute(sequence + 1, "Value", value); b.AddAttribute(sequence + 2, "ChildContent", Text(text)); b.CloseComponent(); }

    private sealed class SelectDossierPreview : ComponentBase
    {
        [Parameter] public bool Invalid { get; set; }
        [Parameter] public bool Open { get; set; }

        private string Value { get; set; } = "cnc";
        private bool EffectiveOpen { get; set; }
        private bool OpenInitialized { get; set; }
        private bool LastOpenParameter { get; set; }

        protected override void OnParametersSet()
        {
            if (!OpenInitialized || Open != LastOpenParameter)
            {
                EffectiveOpen = Open;
                LastOpenParameter = Open;
                OpenInitialized = true;
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-select-dossier");
            builder.OpenComponent<ShadcnSelect<string>>(2);
            builder.AddAttribute(3, "Value", Value);
            builder.AddAttribute(4, "ValueChanged", EventCallback.Factory.Create<string>(this, HandleValueChanged));
            builder.AddAttribute(5, "Options", Processes);
            builder.AddAttribute(6, "Open", EffectiveOpen);
            builder.AddAttribute(7, "OpenChanged", EventCallback.Factory.Create<bool>(this, HandleOpenChanged));
            builder.AddAttribute(8, "Invalid", Invalid);
            builder.AddAttribute(9, "Clearable", true);
            builder.AddAttribute(10, "AdditionalAttributes", Attr("forms-dossier-select", "Process"));
            builder.CloseComponent();
            builder.CloseElement();
        }

        private Task HandleValueChanged(string value)
        {
            Value = value;
            return Task.CompletedTask;
        }

        private Task HandleOpenChanged(bool open)
        {
            EffectiveOpen = open;
            return Task.CompletedTask;
        }
    }
}
