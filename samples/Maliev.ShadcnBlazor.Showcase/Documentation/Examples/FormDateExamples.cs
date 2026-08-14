using System.Globalization;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

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
        var invalid = false; var type = "text";
        RenderFragment preview = b => { b.OpenComponent<ShadcnInput<string>>(0); b.AddAttribute(1, "Value", "ชิ้นส่วน"); b.AddAttribute(2, "Type", type); b.AddAttribute(3, "Invalid", invalid); b.AddAttribute(4, "Required", true); b.AddAttribute(5, "AdditionalAttributes", Attr("forms-dossier-input", "Part name")); b.CloseComponent(); };
        return Example("input", "Typed input", "Exercise typed binding, native input modes, required and invalid states.", "<ShadcnInput TValue=\"string\" @bind-Value=\"PartName\" Required=\"true\" />", preview,
            [Toggle("input-invalid", "Invalid", v => invalid = v), Select("input-type", "Type", type, ["text", "search", "file"], v => type = v)], ["typed-binding", "required", "file", "invalid"]);
    }
    private static ComponentExampleDefinition Textarea()
    {
        var invalid = false; var rows = 3;
        RenderFragment preview = b => { b.OpenComponent<ShadcnTextarea<string>>(0); b.AddAttribute(1, "Value", "รายละเอียดการผลิต"); b.AddAttribute(2, "Rows", rows); b.AddAttribute(3, "Invalid", invalid); b.AddAttribute(4, "AdditionalAttributes", Attr("forms-dossier-textarea", "Manufacturing notes")); b.CloseComponent(); };
        return Example("textarea", "Multiline typed input", "Resize a native multiline control while preserving validation and form binding.", "<ShadcnTextarea TValue=\"string\" @bind-Value=\"Notes\" Rows=\"3\" />", preview,
            [Toggle("textarea-invalid", "Invalid", v => invalid = v), Select("textarea-rows", "Rows", rows.ToString(), ["3", "5"], v => rows = int.Parse(v, CultureInfo.InvariantCulture))], ["typed-binding", "rows", "invalid"]);
    }
    private static ComponentExampleDefinition NativeSelect()
    {
        var invalid = false; var readOnly = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnNativeSelect<string>>(0); b.AddAttribute(1, "Value", "standard"); b.AddAttribute(2, "Invalid", invalid); b.AddAttribute(3, "ReadOnly", readOnly); b.AddAttribute(4, "AdditionalAttributes", Attr("forms-dossier-native-select", "Priority")); b.AddAttribute(5, "ChildContent", (RenderFragment)(c => { AddNativeOption(c, 0, "standard", "Standard"); AddNativeOption(c, 10, "urgent", "Urgent"); })); b.CloseComponent(); };
        return Example("native-select", "Native select", "Compare compact size, real option semantics, and focusable read-only restoration.", "<ShadcnNativeSelect TValue=\"string\" @bind-Value=\"Priority\"><ShadcnNativeSelectOption Value=\"standard\">Standard</ShadcnNativeSelectOption></ShadcnNativeSelect>", preview,
            [Toggle("native-select-invalid", "Invalid", v => invalid = v), Toggle("native-select-readonly", "Read only", v => readOnly = v)], ["selected", "read-only", "invalid"]);
    }
    private static ComponentExampleDefinition InputGroup()
    {
        var invalid = false; var alignment = ShadcnInputGroupAlignment.InlineStart;
        RenderFragment preview = b => { b.OpenComponent<ShadcnInputGroup>(0); b.AddAttribute(1, "AdditionalAttributes", invalid ? new Dictionary<string, object> { ["data-testid"] = "forms-dossier-input-group", ["aria-label"] = "Budget", ["aria-invalid"] = "true" } : Attr("forms-dossier-input-group", "Budget")); b.AddAttribute(2, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnInputGroupAddon>(0); c.AddAttribute(1, "Alignment", alignment); c.AddAttribute(2, "ChildContent", Text("THB")); c.CloseComponent(); c.OpenComponent<ShadcnInput<decimal>>(10); c.AddAttribute(11, "Value", 1250m); c.AddAttribute(12, "Type", "number"); c.AddAttribute(13, "AdditionalAttributes", new Dictionary<string, object> { ["aria-label"] = "Budget" }); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("input-group", "Input group composition", "Move semantic text and actions across logical addon positions.", "<ShadcnInputGroup><ShadcnInputGroupAddon>THB</ShadcnInputGroupAddon><ShadcnInput TValue=\"decimal\" /></ShadcnInputGroup>", preview,
            [Toggle("input-group-invalid", "Invalid", v => invalid = v), EnumSelect("input-group-alignment", "Alignment", alignment, v => alignment = v)], ["addons", "inline", "block", "invalid"]);
    }
    private static ComponentExampleDefinition InputOtp()
    {
        var invalid = false; var numeric = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnInputOtp>(0); b.AddAttribute(1, "Value", numeric ? "123" : "ก้ข"); b.AddAttribute(2, "MaxLength", 6); b.AddAttribute(3, "Pattern", numeric ? "[0-9]" : null); b.AddAttribute(4, "InputMode", numeric ? "numeric" : "text"); b.AddAttribute(5, "Invalid", invalid); b.AddAttribute(6, "AdditionalAttributes", Attr("forms-dossier-input-otp", "Verification code")); b.AddAttribute(7, "ChildContent", (RenderFragment)(c => { c.OpenComponent<ShadcnInputOtpGroup>(0); c.AddAttribute(1, "ChildContent", (RenderFragment)(s => { for (var i = 0; i < 6; i++) { s.OpenComponent<ShadcnInputOtpSlot>(i * 2); s.AddAttribute(i * 2 + 1, "Index", i); s.CloseComponent(); } })); c.CloseComponent(); })); b.CloseComponent(); };
        return Example("input-otp", "One-input OTP", "Verify paste filtering, caret-driven slots, Thai graphemes, and mobile input modes.", "<ShadcnInputOtp @bind-Value=\"Code\" MaxLength=\"6\" Pattern=\"[0-9]\">...</ShadcnInputOtp>", preview,
            [Toggle("input-otp-invalid", "Invalid", v => invalid = v), Toggle("input-otp-numeric", "Numeric", v => numeric = v, true)], ["one-input", "graphemes", "numeric", "invalid"]);
    }
    private static ComponentExampleDefinition Select()
    {
        var invalid = false; var open = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnSelect<string>>(0); b.AddAttribute(1, "Value", "cnc"); b.AddAttribute(2, "Options", Processes); b.AddAttribute(3, "Open", open); b.AddAttribute(4, "Invalid", invalid); b.AddAttribute(5, "Clearable", true); b.AddAttribute(6, "AdditionalAttributes", Attr("forms-dossier-select", "Process")); b.CloseComponent(); };
        return Example("select", "Typed select", "Open a grouped listbox with keyboard navigation and clearable typed value.", "<ShadcnSelect TValue=\"string\" @bind-Value=\"Process\" Options=\"ProcessOptions\" />", preview,
            [Toggle("select-invalid", "Invalid", v => invalid = v), Toggle("select-open", "Open", v => open = v)], ["selected", "groups", "clearable", "open", "invalid"]);
    }
    private static ComponentExampleDefinition Combobox()
    {
        var invalid = false; var multiple = false;
        RenderFragment preview = b => { b.OpenComponent<ShadcnCombobox<string>>(0); b.AddAttribute(1, "Value", "al6061"); b.AddAttribute(2, "Values", new[] { "al6061", "ss316" }); b.AddAttribute(3, "Options", Materials); b.AddAttribute(4, "Multiple", multiple); b.AddAttribute(5, "Open", true); b.AddAttribute(6, "Invalid", invalid); b.AddAttribute(7, "AdditionalAttributes", Attr("forms-dossier-combobox", "Material")); b.CloseComponent(); };
        return Example("combobox", "Searchable typed combobox", "Filter grouped options or switch to multiple chips and clear behavior.", "<ShadcnCombobox TValue=\"string\" @bind-Value=\"Material\" Options=\"MaterialOptions\" />", preview,
            [Toggle("combobox-invalid", "Invalid", v => invalid = v), Toggle("combobox-multiple", "Multiple", v => multiple = v)], ["selected", "multiple", "chips", "open", "invalid"]);
    }
    private static ComponentExampleDefinition Calendar()
    {
        var invalid = false; var mode = ShadcnCalendarSelectionMode.Single;
        RenderFragment preview = b => { b.OpenComponent<ShadcnCalendar>(0); b.AddAttribute(1, "Mode", mode); b.AddAttribute(2, "Value", new DateOnly(2026, 8, 13)); b.AddAttribute(3, "Range", new ShadcnDateRange(new(2026, 8, 10), new(2026, 8, 13))); b.AddAttribute(4, "VisibleMonth", new DateOnly(2026, 8, 1)); b.AddAttribute(5, "Today", new DateOnly(2026, 8, 13)); b.AddAttribute(6, "Culture", CultureInfo.GetCultureInfo("th-TH")); b.AddAttribute(7, "Invalid", invalid); b.AddAttribute(8, "AdditionalAttributes", Attr("forms-dossier-calendar", "Delivery calendar")); b.CloseComponent(); };
        return Example("calendar", "DateOnly calendar", "Switch single/range selection in a deterministic Thai-localized keyboard grid.", "<ShadcnCalendar Mode=\"ShadcnCalendarSelectionMode.Range\" @bind-Range=\"Window\" Culture=\"ThaiCulture\" />", preview,
            [Toggle("calendar-invalid", "Invalid", v => invalid = v), EnumSelect("calendar-mode", "Mode", mode, v => mode = v)], ["single", "range", "culture", "invalid"]);
    }
    private static ComponentExampleDefinition DatePicker()
    {
        var invalid = false; var clearable = true;
        RenderFragment preview = b => { b.OpenComponent<ShadcnDatePicker>(0); b.AddAttribute(1, "Value", new DateOnly(2026, 8, 13)); b.AddAttribute(2, "VisibleMonth", new DateOnly(2026, 8, 1)); b.AddAttribute(3, "Culture", CultureInfo.GetCultureInfo("th-TH")); b.AddAttribute(4, "AllowTextInput", true); b.AddAttribute(5, "Clearable", clearable); b.AddAttribute(6, "Invalid", invalid); b.AddAttribute(7, "AdditionalAttributes", Attr("forms-dossier-date-picker", "Delivery date")); b.CloseComponent(); };
        return Example("date-picker", "Date picker composition", "Use culture-exact text entry, clearable trigger, DateOnly payload, and calendar popup.", "<ShadcnDatePicker @bind-Value=\"DeliveryDate\" AllowTextInput=\"true\" Culture=\"ThaiCulture\" />", preview,
            [Toggle("date-picker-invalid", "Invalid", v => invalid = v), Toggle("date-picker-clearable", "Clearable", v => clearable = v, true)], ["single", "text-input", "culture", "clearable", "invalid"]);
    }

    private static ComponentExampleDefinition Example(string slug, string title, string description, string source, RenderFragment preview, IReadOnlyList<ComponentParameterControl> controls, IReadOnlyList<string> tags) => new($"{slug}-primary", title, description, source, preview, controls, tags);
    private static ComponentParameterControl Toggle(string id, string label, Action<bool> apply, bool initial = false) => new(id, label, ComponentParameterControlKind.Toggle, initial.ToString(), [], value => apply(bool.Parse(value)));
    private static ComponentParameterControl Select(string id, string label, string value, IReadOnlyList<string> options, Action<string> apply) => new(id, label, ComponentParameterControlKind.Select, value, options, apply);
    private static ComponentParameterControl EnumSelect<T>(string id, string label, T value, Action<T> apply) where T : struct, Enum => new(id, label, ComponentParameterControlKind.Select, value.ToString(), Enum.GetNames<T>(), text => apply(Enum.Parse<T>(text)));
    private static IReadOnlyDictionary<string, object> Attr(string testId, string label) => new Dictionary<string, object> { ["data-testid"] = testId, ["aria-label"] = label };
    private static RenderFragment Text(string value) => b => b.AddContent(0, value);
    private static void AddNativeOption(RenderTreeBuilder b, int sequence, string value, string text) { b.OpenComponent<ShadcnNativeSelectOption<string>>(sequence); b.AddAttribute(sequence + 1, "Value", value); b.AddAttribute(sequence + 2, "ChildContent", Text(text)); b.CloseComponent(); }
}
