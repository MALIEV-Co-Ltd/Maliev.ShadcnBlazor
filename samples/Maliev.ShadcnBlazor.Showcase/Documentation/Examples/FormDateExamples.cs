using System.Globalization;
using Maliev.ShadcnBlazor.Components.Actions;
using Maliev.ShadcnBlazor.Components.Content;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
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
        "dropzone" => [Dropzone()],
        _ => []
    };

    private static ComponentExampleDefinition Dropzone()
    {
        var multiple = true; var loading = false;
        RenderFragment preview = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-dropzone-dossier");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "showcase-dropzone-dossier__heading");
            builder.OpenElement(4, "strong");
            builder.AddContent(5, "Production drawing package");
            builder.CloseElement();
            builder.OpenElement(6, "span");
            builder.AddContent(7, "Quotation Q-4218 · Revision C");
            builder.CloseElement();
            builder.CloseElement();
            builder.OpenComponent<ShadcnDropzone>(8);
            builder.AddAttribute(9, nameof(ShadcnDropzone.Class), "showcase-dropzone-dossier__control");
            builder.AddAttribute(10, nameof(ShadcnDropzone.Accept), ".step,.stp,.pdf");
            builder.AddAttribute(11, nameof(ShadcnDropzone.Multiple), multiple);
            builder.AddAttribute(12, nameof(ShadcnDropzone.MaxFiles), multiple ? 5 : 1);
            builder.AddAttribute(13, nameof(ShadcnDropzone.MaxFileSize), 20 * 1024 * 1024L);
            builder.AddAttribute(14, nameof(ShadcnDropzone.Loading), loading);
            builder.AddAttribute(15, nameof(ShadcnDropzone.Instructions), "Drop STEP or PDF drawings here, or choose files");
            builder.AddAttribute(16, nameof(ShadcnDropzone.Description), "Up to 20 MB per file. Files remain caller-owned until you upload them.");
            builder.CloseComponent();
            builder.CloseElement();
        };
        string Source() => $$"""
            @using Maliev.ShadcnBlazor.Components.Forms
            @using Microsoft.AspNetCore.Components.Forms

            <ShadcnDropzone Accept=".step,.stp,.pdf"
                            Multiple="{{multiple.ToString().ToLowerInvariant()}}"
                            MaxFiles="{{(multiple ? 5 : 1)}}"
                            MaxFileSize="@(20 * 1024 * 1024)"
                            Loading="{{loading.ToString().ToLowerInvariant()}}"
                            Instructions="Drop STEP or PDF drawings here, or choose files"
                            Description="Up to 20 MB per file. Files remain caller-owned until you upload them."
                            SelectionChanged="HandleSelection" />

            @code {
                private Task HandleSelection(ShadcnDropzoneSelection selection)
                {
                    // Read or upload selection.Files in caller-owned application code.
                    return Task.CompletedTask;
                }
            }
            """;
        return Example("dropzone", "Drawing package intake", "Select or drop production files through one accessible browser boundary, validate them locally, and keep upload behavior caller-owned.", Source(), preview,
            [
                Toggle("dropzone-multiple", "Multiple files", value => multiple = value, true),
                Toggle("dropzone-loading", "Processing", value => loading = value)
            ], ["click", "keyboard", "drop", "validation", "invalid", "multiple", "disabled", "loading", "rtl"]) with
        { RazorSourceProvider = Source };
    }

    private static ComponentExampleDefinition Input()
    {
        var invalid = false; var masked = true; var disabled = false; var readOnly = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<InputDossierPreview>(0);
            b.AddAttribute(1, nameof(InputDossierPreview.Invalid), invalid);
            b.AddAttribute(2, nameof(InputDossierPreview.Masked), masked);
            b.AddAttribute(3, nameof(InputDossierPreview.Disabled), disabled);
            b.AddAttribute(4, nameof(InputDossierPreview.ReadOnly), readOnly);
            b.CloseComponent();
        };
        string Source() => InputSource(invalid, masked, disabled, readOnly);
        return Example("input", "Production credentials", "Edit and upload integration credentials in a realistic, accessible form.", Source(), preview,
            [
                Toggle("input-invalid", "Invalid", v => invalid = v),
                Toggle("input-masked", "Mask key", v => masked = v, true),
                Toggle("input-disabled", "Disabled", v => disabled = v),
                Toggle("input-readonly", "Read only", v => readOnly = v)
            ], ["typed-binding", "required", "file", "invalid", "disabled", "read-only"]) with
        { RazorSourceProvider = Source };
    }

    private static string InputSource(bool invalid, bool masked, bool disabled, bool readOnly)
    {
        var invalidText = invalid.ToString().ToLowerInvariant();
        var disabledText = disabled.ToString().ToLowerInvariant();
        var readOnlyText = readOnly.ToString().ToLowerInvariant();
        var errorId = invalid ? " ErrorId=\"integration-key-error\"" : string.Empty;
        var error = invalid
            ? "\n            <ShadcnFieldError>Enter a valid integration key.</ShadcnFieldError>"
            : string.Empty;

        return $$"""
            @using Maliev.ShadcnBlazor.Components.Actions
            @using Maliev.ShadcnBlazor.Components.Content
            @using Maliev.ShadcnBlazor.Components.Forms
            @using Microsoft.AspNetCore.Components.Forms

            <ShadcnCard>
                <ShadcnCardHeader>
                    <ShadcnCardTitle>Production API credentials</ShadcnCardTitle>
                    <ShadcnCardDescription>Connect the CAD intake service without exposing the key.</ShadcnCardDescription>
                </ShadcnCardHeader>
                <ShadcnCardContent>
                    <ShadcnField Invalid="{{invalidText}}" Disabled="{{disabledText}}" DescriptionId="integration-key-help"{{errorId}}>
                        <ShadcnFieldLabel For="integration-key">Integration key</ShadcnFieldLabel>
                        <ShadcnInput TValue="string"
                                     id="integration-key"
                                     @bind-Value="ApiKey"
                                     Type="{{(masked ? "password" : "text")}}"
                                     Placeholder="api_live_demo_7hK2"
                                     InputMode="text"
                                     AutoComplete="off"
                                     Required="true"
                                     ReadOnly="{{readOnlyText}}" />
                        <ShadcnFieldDescription>Stored encrypted and never included in logs.</ShadcnFieldDescription>{{error}}
                    </ShadcnField>

                    <ShadcnField Disabled="{{disabledText}}" DescriptionId="credential-file-help">
                        <ShadcnFieldLabel For="credential-file">Credential file</ShadcnFieldLabel>
                        <ShadcnInput TValue="string"
                                     id="credential-file"
                                     Type="file"
                                     Accept=".json,.pem"
                                     FilesChanged="HandleCredentialFile" />
                        <ShadcnFieldDescription>Optional encrypted JSON or PEM credential.</ShadcnFieldDescription>
                    </ShadcnField>
                </ShadcnCardContent>
                <ShadcnCardFooter>
                    <ShadcnButton Disabled="{{(disabled || invalid).ToString().ToLowerInvariant()}}" OnClick="Save">Save credentials</ShadcnButton>
                    <p role="status">@Status</p>
                </ShadcnCardFooter>
            </ShadcnCard>

            @code {
                private string ApiKey { get; set; } = "api_live_demo_7hK2";
                private string Status { get; set; } = "Ready to save";

                private Task HandleCredentialFile(InputFileChangeEventArgs args)
                {
                    Status = $"Selected {args.File.Name}";
                    return Task.CompletedTask;
                }

                private void Save() => Status = "Credentials saved for this demo.";
            }
            """;
    }
    private static ComponentExampleDefinition Textarea()
    {
        var invalid = false; var rows = 3;
        RenderFragment preview = b =>
        {
            b.OpenComponent<TextareaDossierPreview>(0);
            b.AddAttribute(1, nameof(TextareaDossierPreview.Rows), rows);
            b.AddAttribute(2, nameof(TextareaDossierPreview.Invalid), invalid);
            b.CloseComponent();
        };
        string Source() => $$"""
            <ShadcnCard Class="showcase-textarea-dossier">
                <ShadcnCardHeader>
                    <ShadcnCardTitle dir="auto">Manufacturing note</ShadcnCardTitle>
                    <ShadcnCardDescription dir="auto">Bracket housing · Revision C</ShadcnCardDescription>
                </ShadcnCardHeader>
                <ShadcnCardContent>
                    <ShadcnField Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                                 DescriptionId="manufacturing-notes-description"
                                 ErrorId="manufacturing-notes-error">
                        <ShadcnFieldLabel For="manufacturing-notes" dir="auto">Manufacturing instructions</ShadcnFieldLabel>
                        <ShadcnTextarea TValue="string"
                                        id="manufacturing-notes"
                                        @bind-Value="Notes"
                                        Rows="{{rows}}"
                                        Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                                        Name="manufacturingNotes"
                                        Placeholder="Add setup, finish, or inspection notes"
                                        maxlength="500"
                                        dir="auto" />
                        <ShadcnFieldDescription Id="manufacturing-notes-description" dir="auto">
                            Include drawing callouts that the production team must verify.
                        </ShadcnFieldDescription>
            {{(invalid ? "            <ShadcnFieldError Id=\"manufacturing-notes-error\" dir=\"auto\">Add the critical manufacturing instructions before continuing.</ShadcnFieldError>" : string.Empty)}}
                    </ShadcnField>
                </ShadcnCardContent>
                <ShadcnCardFooter>
                    <span aria-live="polite" dir="auto">@Notes.Length / 500 characters</span>
                    <span dir="auto">Saved locally</span>
                </ShadcnCardFooter>
            </ShadcnCard>

            @code {
                private string Notes { get; set; } = "Deburr all edges and inspect the M6 thread before packing.";
            }
            """;
        return Example("textarea", "Manufacturing instructions", "Write a production note, compare useful row counts, and inspect native validation in a complete labeled field.", Source(), preview,
            [
                Toggle("textarea-invalid", "Invalid", v => invalid = v),
                Select("textarea-rows", "Rows", rows.ToString(), ["3", "5"], v => rows = int.Parse(v, CultureInfo.InvariantCulture))
            ], ["typed-binding", "rows", "invalid"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition NativeSelect()
    {
        var invalid = false; var readOnly = false; var compact = false;
        ShadcnControlSize Size() => compact ? ShadcnControlSize.Small : ShadcnControlSize.Default;
        RenderFragment preview = b =>
        {
            b.OpenComponent<NativeSelectDossierPreview>(0);
            b.AddAttribute(1, nameof(NativeSelectDossierPreview.Size), Size());
            b.AddAttribute(2, nameof(NativeSelectDossierPreview.Invalid), invalid);
            b.AddAttribute(3, nameof(NativeSelectDossierPreview.ReadOnly), readOnly);
            b.CloseComponent();
        };
        string Source() => $$"""
            <ShadcnCard>
                <ShadcnCardHeader>
                    <ShadcnCardTitle>Production priority</ShadcnCardTitle>
                    <ShadcnCardDescription>Route quotation Q-4189 through the planning queue.</ShadcnCardDescription>
                </ShadcnCardHeader>
                <ShadcnCardContent>
                    <ShadcnLabel For="production-priority">Priority and lead time</ShadcnLabel>
                    <ShadcnNativeSelect TValue="string"
                                        id="production-priority"
                                        Name="priority"
                                        @bind-Value="Priority"
                                        Size="ShadcnControlSize.{{Size()}}"
                                        Invalid="{{invalid.ToString().ToLowerInvariant()}}"
                                        ReadOnly="{{readOnly.ToString().ToLowerInvariant()}}"
                                        aria-describedby="production-priority-summary">
                        <ShadcnNativeSelectOptGroup Label="Production">
                            <ShadcnNativeSelectOption Value="standard">Standard · 5–7 business days</ShadcnNativeSelectOption>
                            <ShadcnNativeSelectOption Value="urgent">Urgent · 2–3 business days</ShadcnNativeSelectOption>
                        </ShadcnNativeSelectOptGroup>
                        <ShadcnNativeSelectOptGroup Label="Exceptions">
                            <ShadcnNativeSelectOption Value="hold" Disabled="true">Engineering hold</ShadcnNativeSelectOption>
                        </ShadcnNativeSelectOptGroup>
                    </ShadcnNativeSelect>
                    <p id="production-priority-summary" role="status">@LeadTime</p>
                </ShadcnCardContent>
                <ShadcnCardFooter>Native option groups remain available to keyboard and assistive technology.</ShadcnCardFooter>
            </ShadcnCard>

            @code {
                private string Priority { get; set; } = "standard";
                private string LeadTime => Priority == "urgent"
                    ? "Urgent queue · 2–3 business days"
                    : "Standard queue · 5–7 business days";
            }
            """;
        return Example("native-select", "Production routing", "Choose a real production priority with native option groups, disabled exceptions, validation, and focusable read-only restoration.", Source(), preview,
            [
                Toggle("native-select-compact", "Compact", v => compact = v),
                Toggle("native-select-invalid", "Invalid", v => invalid = v),
                Toggle("native-select-readonly", "Read only", v => readOnly = v)
            ], ["selected", "groups", "disabled", "read-only", "invalid", "sm"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition InputGroup()
    {
        var invalid = false; var alignment = ShadcnInputGroupAlignment.InlineEnd;
        RenderFragment preview = b =>
        {
            b.OpenComponent<InputGroupDossierPreview>(0);
            b.AddAttribute(1, nameof(InputGroupDossierPreview.Invalid), invalid);
            b.AddAttribute(2, nameof(InputGroupDossierPreview.Alignment), alignment);
            b.CloseComponent();
        };
        string Source() => $"@using System.Globalization\n@using Maliev.ShadcnBlazor.Components.Actions\n@using Maliev.ShadcnBlazor.Components.Content\n@using Maliev.ShadcnBlazor.Components.Forms\n\n<ShadcnCard Size=\"ShadcnCardSize.Small\">\n    <ShadcnCardHeader>\n        <ShadcnCardTitle>Part estimate</ShadcnCardTitle>\n        <ShadcnCardDescription>Update the unit price for 12 machined parts.</ShadcnCardDescription>\n    </ShadcnCardHeader>\n    <ShadcnCardContent>\n        <ShadcnLabel For=\"quote-unit-price\">Unit price</ShadcnLabel>\n        <ShadcnInputGroup aria-invalid=\"{invalid.ToString().ToLowerInvariant()}\">\n            <ShadcnInput TValue=\"decimal\" id=\"quote-unit-price\" @bind-Value=\"UnitPrice\" Type=\"number\" Invalid=\"{invalid.ToString().ToLowerInvariant()}\" inputmode=\"decimal\" min=\"0\" />\n            <ShadcnInputGroupAddon Alignment=\"ShadcnInputGroupAlignment.{alignment}\">\n                <ShadcnInputGroupText>THB / part</ShadcnInputGroupText>\n                <ShadcnInputGroupButton Variant=\"ShadcnButtonVariant.Ghost\" Size=\"ShadcnInputGroupButtonSize.IconExtraSmall\" OnClick=\"ResetUnitPrice\" aria-label=\"Reset unit price\">\n                    <svg viewBox=\"0 0 24 24\" aria-hidden=\"true\"><path d=\"M3 12a9 9 0 1 0 3-6.7M3 4v5h5\" /></svg>\n                </ShadcnInputGroupButton>\n            </ShadcnInputGroupAddon>\n        </ShadcnInputGroup>\n    </ShadcnCardContent>\n    <ShadcnCardFooter>\n        <span>Estimated subtotal</span>\n        <output for=\"quote-unit-price\">@((UnitPrice * Quantity).ToString(\"C0\", ThaiCulture))</output>\n    </ShadcnCardFooter>\n</ShadcnCard>\n\n@code {{\n    private const int Quantity = 12;\n    private static readonly CultureInfo ThaiCulture = CultureInfo.GetCultureInfo(\"th-TH\");\n    private decimal UnitPrice {{ get; set; }} = 1250m;\n    private void ResetUnitPrice() => UnitPrice = 1250m;\n}}";
        return Example("input-group", "Production price input", "Compose semantic text and an inline action around a live unit price without widening the form.", Source(), preview,
            [
                Toggle("input-group-invalid", "Invalid", v => invalid = v),
                EnumSelect("input-group-alignment", "Alignment", alignment, v => alignment = v)
            ], ["addons", "inline", "block", "button", "invalid", "rtl"]) with
        { RazorSourceProvider = Source };
    }
    private static ComponentExampleDefinition InputOtp()
    {
        var invalid = false; var numeric = true;
        RenderFragment preview = b =>
        {
            b.OpenComponent<InputOtpDossierPreview>(0);
            b.AddAttribute(1, nameof(InputOtpDossierPreview.Invalid), invalid);
            b.AddAttribute(2, nameof(InputOtpDossierPreview.Numeric), numeric);
            b.CloseComponent();
        };
        string Source() => InputOtpSource(numeric, invalid);
        return Example("input-otp", "Email verification", "Enter or paste a one-time code, inspect validation feedback, and complete a realistic verification flow.", Source(), preview,
            [
                Toggle("input-otp-invalid", "Invalid", v => invalid = v),
                Toggle("input-otp-numeric", "Numeric", v => numeric = v, true)
            ], ["one-input", "paste", "keyboard", "status", "graphemes", "numeric", "invalid"]) with
        { RazorSourceProvider = Source };
    }

    private static string InputOtpSource(bool numeric, bool invalid)
    {
        var pattern = numeric ? " Pattern=\"[0-9]\"" : string.Empty;
        var inputMode = numeric ? "numeric" : "text";
        var invalidValue = invalid.ToString().ToLowerInvariant();
        return $$"""
@using System.Globalization
@using Maliev.ShadcnBlazor.Components.Actions
@using Maliev.ShadcnBlazor.Components.Content
@using Maliev.ShadcnBlazor.Components.Forms

<ShadcnCard Class="otp-verification-card">
    <ShadcnCardHeader>
        <div>
            <ShadcnCardTitle>Verify your email</ShadcnCardTitle>
            <ShadcnCardDescription>
                Enter the 6-digit code sent to <strong>n•••@@maliev.example</strong>.
            </ShadcnCardDescription>
        </div>
        <ShadcnCardAction>
            <span aria-label="Secure verification">
                <svg aria-hidden="true" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z" />
                    <path d="m9 12 2 2 4-4" />
                </svg>
            </span>
        </ShadcnCardAction>
    </ShadcnCardHeader>
    <ShadcnCardContent>
        <label for="verification-code">Verification code</label>
        <ShadcnInputOtp id="verification-code" Value="@Code" ValueChanged="HandleCodeChanged" MaxLength="6"{{pattern}} InputMode="{{inputMode}}" Required="true" Invalid="{{invalidValue}}" aria-label="Verification code" aria-describedby="otp-status">
            <ShadcnInputOtpGroup>
                <ShadcnInputOtpSlot Index="0" />
                <ShadcnInputOtpSlot Index="1" />
                <ShadcnInputOtpSlot Index="2" />
            </ShadcnInputOtpGroup>
            <ShadcnInputOtpSeparator />
            <ShadcnInputOtpGroup>
                <ShadcnInputOtpSlot Index="3" />
                <ShadcnInputOtpSlot Index="4" />
                <ShadcnInputOtpSlot Index="5" />
            </ShadcnInputOtpGroup>
        </ShadcnInputOtp>
        <p id="otp-status" role="status" aria-live="polite">@StatusText</p>
        <ShadcnButton Disabled="@(!CanVerify)" OnClick="VerifyCode">Verify code</ShadcnButton>
    </ShadcnCardContent>
    <ShadcnCardFooter>
        <span>Didn't receive a code?</span>
        <ShadcnButton Variant="ShadcnButtonVariant.Link" OnClick="SendNewCode">Send a new code</ShadcnButton>
    </ShadcnCardFooter>
</ShadcnCard>

@code {
    private string Code = string.Empty;
    private bool Verified;
    private bool Resent;
    private bool IsInvalid => {{invalidValue}};
    private int CodeLength => StringInfo.ParseCombiningCharacters(Code).Length;
    private bool CanVerify => CodeLength == 6 && !Verified && !IsInvalid;
    private string StatusText => IsInvalid
        ? "That code is invalid. Check the email and try again."
        : Verified
            ? "Email verified. You can continue."
            : Resent
                ? "A new code was sent. Enter it below."
                : CodeLength == 6
                    ? "Code ready to verify."
                    : $"Enter {6 - CodeLength} more characters.";

    private Task HandleCodeChanged(string value)
    {
        Code = value;
        Verified = false;
        Resent = false;
        return Task.CompletedTask;
    }

    private Task VerifyCode()
    {
        if (CanVerify) Verified = true;
        return Task.CompletedTask;
    }

    private Task SendNewCode()
    {
        Code = string.Empty;
        Verified = false;
        Resent = true;
        return Task.CompletedTask;
    }
}
""";
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
        var invalid = false; var mode = ShadcnCalendarSelectionMode.Single; var captionLayout = ShadcnCalendarCaptionLayout.Label; var showWeekNumbers = false;
        RenderFragment preview = b =>
        {
            b.OpenComponent<CalendarDossierPreview>(0);
            b.AddAttribute(1, nameof(CalendarDossierPreview.Mode), mode);
            b.AddAttribute(2, nameof(CalendarDossierPreview.CaptionLayout), captionLayout);
            b.AddAttribute(3, nameof(CalendarDossierPreview.ShowWeekNumbers), showWeekNumbers);
            b.AddAttribute(4, nameof(CalendarDossierPreview.Invalid), invalid);
            b.CloseComponent();
        };
        string Source()
        {
            var binding = mode == ShadcnCalendarSelectionMode.Range ? "@bind-Range=\"InspectionWindow\"" : "@bind-Value=\"InspectionDate\"";
            var summary = mode == ShadcnCalendarSelectionMode.Range
                ? "    private string SelectionSummary => InspectionWindow switch\n    {\n        null => \"เลือกช่วงวันที่ตรวจรับ\",\n        { End: null } => $\"เริ่ม {InspectionWindow.Start.ToString(\"d MMM yyyy\", ThaiCulture)} · เลือกวันสิ้นสุด\",\n        _ => $\"{InspectionWindow.Start.ToString(\"d MMM\", ThaiCulture)} — {InspectionWindow.End!.Value.ToString(\"d MMM yyyy\", ThaiCulture)}\"\n    };"
                : "    private string SelectionSummary => InspectionDate is null\n        ? \"ยังไม่ได้เลือกวันที่\"\n        : $\"เลือกแล้ว · {InspectionDate.Value.ToString(\"d MMMM yyyy\", ThaiCulture)}\";";
            return $"@using System.Globalization\n@using Maliev.ShadcnBlazor.Components.Content\n@using Maliev.ShadcnBlazor.Components.Forms\n\n<ShadcnCard>\n    <ShadcnCardHeader>\n        <ShadcnCardTitle>กำหนดวันตรวจรับ</ShadcnCardTitle>\n        <ShadcnCardDescription>เลือกวันที่หรือช่วงเวลาสำหรับตรวจรับชิ้นงาน</ShadcnCardDescription>\n    </ShadcnCardHeader>\n    <ShadcnCardContent>\n        <ShadcnCalendar Mode=\"ShadcnCalendarSelectionMode.{mode}\"\n                        CaptionLayout=\"ShadcnCalendarCaptionLayout.{captionLayout}\"\n                        {binding}\n                        @bind-VisibleMonth=\"VisibleMonth\"\n                        Today=\"Today\"\n                        Culture=\"ThaiCulture\"\n                        ShowWeekNumbers=\"{showWeekNumbers.ToString().ToLowerInvariant()}\"\n                        Invalid=\"{invalid.ToString().ToLowerInvariant()}\"\n                        PreviousLabel=\"เดือนก่อนหน้า\"\n                        NextLabel=\"เดือนถัดไป\"\n                        WeekLabel=\"สัปดาห์\"\n                        MonthSelectLabel=\"เลือกเดือน\"\n                        YearSelectLabel=\"เลือกปี\"\n                        aria-label=\"ปฏิทินตรวจรับชิ้นงาน\" />\n    </ShadcnCardContent>\n    <ShadcnCardFooter>\n        <output aria-live=\"polite\">@SelectionSummary</output>\n    </ShadcnCardFooter>\n</ShadcnCard>\n\n@code {{\n    private CultureInfo ThaiCulture {{ get; }} = CultureInfo.GetCultureInfo(\"th-TH\");\n    private DateOnly Today {{ get; }} = new(2026, 8, 13);\n    private DateOnly VisibleMonth {{ get; set; }} = new(2026, 8, 1);\n    private DateOnly? InspectionDate {{ get; set; }} = new(2026, 8, 13);\n    private ShadcnDateRange? InspectionWindow {{ get; set; }} = new(new DateOnly(2026, 8, 10), new DateOnly(2026, 8, 13));\n{summary}\n}}";
        }
        return Example("calendar", "Inspection calendar", "Select a Thai-localized inspection date or connected date range with keyboard navigation and optional week numbers.", Source(), preview,
            [
                Toggle("calendar-invalid", "Invalid", v => invalid = v),
                EnumSelect("calendar-mode", "Mode", mode, v => mode = v),
                EnumSelect("calendar-caption-layout", "Caption", captionLayout, v => captionLayout = v),
                Toggle("calendar-week-numbers", "Week numbers", v => showWeekNumbers = v)
            ], ["single", "range", "culture", "keyboard", "week-numbers", "invalid"]) with
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

    private sealed class InputDossierPreview : ComponentBase
    {
        [Parameter] public bool Invalid { get; set; }
        [Parameter] public bool Masked { get; set; } = true;
        [Parameter] public bool Disabled { get; set; }
        [Parameter] public bool ReadOnly { get; set; }

        private string ApiKey { get; set; } = "api_live_demo_7hK2";
        private string Status { get; set; } = "Ready to save";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-input-dossier");
            builder.AddAttribute(2, "data-testid", "input-dossier-preview");
            builder.OpenComponent<ShadcnCard>(3);
            builder.AddAttribute(4, nameof(ShadcnCard.ChildContent), (RenderFragment)(card =>
            {
                card.OpenComponent<ShadcnCardHeader>(0);
                card.AddAttribute(1, nameof(ShadcnCardHeader.ChildContent), (RenderFragment)(header =>
                {
                    header.OpenComponent<ShadcnCardTitle>(0);
                    header.AddAttribute(1, nameof(ShadcnCardTitle.ChildContent), Text("Production API credentials"));
                    header.CloseComponent();
                    header.OpenComponent<ShadcnCardDescription>(2);
                    header.AddAttribute(3, nameof(ShadcnCardDescription.ChildContent), Text("Connect the CAD intake service without exposing the key."));
                    header.CloseComponent();
                }));
                card.CloseComponent();

                card.OpenComponent<ShadcnCardContent>(10);
                card.AddAttribute(11, nameof(ShadcnCardContent.ChildContent), (RenderFragment)(content =>
                {
                    AddKeyField(content);
                    AddFileField(content);
                }));
                card.CloseComponent();

                card.OpenComponent<ShadcnCardFooter>(20);
                card.AddAttribute(21, nameof(ShadcnCardFooter.ChildContent), (RenderFragment)(footer =>
                {
                    footer.OpenComponent<ShadcnButton>(0);
                    footer.AddAttribute(1, nameof(ShadcnButton.Disabled), Disabled || Invalid);
                    footer.AddAttribute(2, nameof(ShadcnButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, Save));
                    footer.AddAttribute(3, nameof(ShadcnButton.ChildContent), Text("Save credentials"));
                    footer.AddAttribute(4, nameof(ShadcnButton.AdditionalAttributes), new Dictionary<string, object> { ["data-testid"] = "forms-dossier-save" });
                    footer.CloseComponent();
                    footer.OpenElement(5, "p");
                    footer.AddAttribute(6, "role", "status");
                    footer.AddAttribute(7, "data-testid", "forms-dossier-status");
                    footer.AddContent(8, Status);
                    footer.CloseElement();
                }));
                card.CloseComponent();
            }));
            builder.CloseComponent();
            builder.CloseElement();
        }

        private void AddKeyField(RenderTreeBuilder builder)
        {
            builder.OpenComponent<ShadcnField>(0);
            builder.AddAttribute(1, nameof(ShadcnField.Invalid), Invalid);
            builder.AddAttribute(2, nameof(ShadcnField.Disabled), Disabled);
            builder.AddAttribute(3, nameof(ShadcnField.DescriptionId), "integration-key-help");
            builder.AddAttribute(4, nameof(ShadcnField.ErrorId), Invalid ? "integration-key-error" : null);
            builder.AddAttribute(5, nameof(ShadcnField.ChildContent), (RenderFragment)(field =>
            {
                field.OpenComponent<ShadcnFieldLabel>(0);
                field.AddAttribute(1, nameof(ShadcnFieldLabel.For), "integration-key");
                field.AddAttribute(2, nameof(ShadcnFieldLabel.ChildContent), Text("Integration key"));
                field.CloseComponent();
                field.OpenComponent<ShadcnInput<string>>(3);
                field.AddAttribute(4, nameof(ShadcnInput<string>.Value), ApiKey);
                field.AddAttribute(5, nameof(ShadcnInput<string>.ValueChanged), EventCallback.Factory.Create<string>(this, HandleKeyChanged));
                field.AddAttribute(6, nameof(ShadcnInput<string>.Type), Masked ? "password" : "text");
                field.AddAttribute(7, nameof(ShadcnInput<string>.Placeholder), "api_live_demo_7hK2");
                field.AddAttribute(8, nameof(ShadcnInput<string>.InputMode), "text");
                field.AddAttribute(9, nameof(ShadcnInput<string>.AutoComplete), "off");
                field.AddAttribute(10, nameof(ShadcnInput<string>.Required), true);
                field.AddAttribute(11, nameof(ShadcnInput<string>.ReadOnly), ReadOnly);
                field.AddAttribute(12, nameof(ShadcnInput<string>.AdditionalAttributes), new Dictionary<string, object>
                {
                    ["id"] = "integration-key",
                    ["data-testid"] = "forms-dossier-input"
                });
                field.CloseComponent();
                field.OpenComponent<ShadcnFieldDescription>(13);
                field.AddAttribute(14, nameof(ShadcnFieldDescription.ChildContent), Text("Stored encrypted and never included in logs."));
                field.CloseComponent();
                if (Invalid)
                {
                    field.OpenComponent<ShadcnFieldError>(15);
                    field.AddAttribute(16, nameof(ShadcnFieldError.ChildContent), Text("Enter a valid integration key."));
                    field.CloseComponent();
                }
            }));
            builder.CloseComponent();
        }

        private void AddFileField(RenderTreeBuilder builder)
        {
            builder.OpenComponent<ShadcnField>(20);
            builder.AddAttribute(21, nameof(ShadcnField.Disabled), Disabled);
            builder.AddAttribute(22, nameof(ShadcnField.DescriptionId), "credential-file-help");
            builder.AddAttribute(23, nameof(ShadcnField.ChildContent), (RenderFragment)(field =>
            {
                field.OpenComponent<ShadcnFieldLabel>(0);
                field.AddAttribute(1, nameof(ShadcnFieldLabel.For), "credential-file");
                field.AddAttribute(2, nameof(ShadcnFieldLabel.ChildContent), Text("Credential file"));
                field.CloseComponent();
                field.OpenComponent<ShadcnInput<string>>(3);
                field.AddAttribute(4, nameof(ShadcnInput<string>.Type), "file");
                field.AddAttribute(5, nameof(ShadcnInput<string>.Accept), ".json,.pem");
                field.AddAttribute(6, nameof(ShadcnInput<string>.FilesChanged), EventCallback.Factory.Create<InputFileChangeEventArgs>(this, HandleCredentialFile));
                field.AddAttribute(7, nameof(ShadcnInput<string>.AdditionalAttributes), new Dictionary<string, object>
                {
                    ["id"] = "credential-file",
                    ["data-testid"] = "forms-dossier-file"
                });
                field.CloseComponent();
                field.OpenComponent<ShadcnFieldDescription>(8);
                field.AddAttribute(9, nameof(ShadcnFieldDescription.ChildContent), Text("Optional encrypted JSON or PEM credential."));
                field.CloseComponent();
            }));
            builder.CloseComponent();
        }

        private Task HandleKeyChanged(string value)
        {
            ApiKey = value;
            Status = "Unsaved changes";
            return Task.CompletedTask;
        }

        private Task HandleCredentialFile(InputFileChangeEventArgs args)
        {
            Status = $"Selected {args.File.Name}";
            return Task.CompletedTask;
        }

        private void Save() => Status = "Credentials saved for this demo.";
    }

    private sealed class InputGroupDossierPreview : ComponentBase
    {
        [Parameter] public bool Invalid { get; set; }
        [Parameter] public ShadcnInputGroupAlignment Alignment { get; set; } = ShadcnInputGroupAlignment.InlineEnd;

        private const int Quantity = 12;
        private decimal UnitPrice { get; set; } = 1250m;
        private static readonly CultureInfo ThaiCulture = CultureInfo.GetCultureInfo("th-TH");

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-input-group-dossier");
            builder.AddAttribute(2, "data-testid", "forms-dossier-input-group-demo");
            builder.OpenComponent<ShadcnCard>(3);
            builder.AddAttribute(4, nameof(ShadcnCard.Size), ShadcnCardSize.Small);
            builder.AddAttribute(5, nameof(ShadcnCard.Class), "showcase-input-group-card");
            builder.AddAttribute(6, nameof(ShadcnCard.ChildContent), (RenderFragment)(card =>
            {
                card.OpenComponent<ShadcnCardHeader>(0);
                card.AddAttribute(1, nameof(ShadcnCardHeader.ChildContent), (RenderFragment)(header =>
                {
                    header.OpenComponent<ShadcnCardTitle>(0);
                    header.AddAttribute(1, nameof(ShadcnCardTitle.ChildContent), Text("Part estimate"));
                    header.CloseComponent();
                    header.OpenComponent<ShadcnCardDescription>(2);
                    header.AddAttribute(3, nameof(ShadcnCardDescription.ChildContent), Text("Update the unit price for 12 machined parts."));
                    header.CloseComponent();
                }));
                card.CloseComponent();

                card.OpenComponent<ShadcnCardContent>(10);
                card.AddAttribute(11, nameof(ShadcnCardContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenElement(0, "div");
                    content.AddAttribute(1, "class", "showcase-input-group-field");
                    content.OpenComponent<ShadcnLabel>(2);
                    content.AddAttribute(3, nameof(ShadcnLabel.For), "quote-unit-price");
                    content.AddAttribute(4, nameof(ShadcnLabel.ChildContent), Text("Unit price"));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnInputGroup>(5);
                    content.AddAttribute(6, nameof(ShadcnInputGroup.AdditionalAttributes), new Dictionary<string, object>
                    {
                        ["data-testid"] = "forms-dossier-input-group",
                        ["aria-label"] = "Unit price",
                        ["aria-invalid"] = Invalid ? "true" : "false"
                    });
                    content.AddAttribute(7, nameof(ShadcnInputGroup.ChildContent), (RenderFragment)(group =>
                    {
                        group.OpenComponent<ShadcnInput<decimal>>(0);
                        group.AddAttribute(1, nameof(ShadcnInput<decimal>.Value), UnitPrice);
                        group.AddAttribute(2, nameof(ShadcnInput<decimal>.ValueChanged), EventCallback.Factory.Create<decimal>(this, HandleUnitPriceChanged));
                        group.AddAttribute(3, nameof(ShadcnInput<decimal>.Type), "number");
                        group.AddAttribute(4, nameof(ShadcnInput<decimal>.Invalid), Invalid);
                        group.AddAttribute(5, nameof(ShadcnInput<decimal>.AdditionalAttributes), new Dictionary<string, object>
                        {
                            ["id"] = "quote-unit-price",
                            ["aria-label"] = "Unit price",
                            ["inputmode"] = "decimal",
                            ["min"] = "0",
                            ["step"] = "0.01"
                        });
                        group.CloseComponent();
                        group.OpenComponent<ShadcnInputGroupAddon>(10);
                        group.AddAttribute(11, nameof(ShadcnInputGroupAddon.Alignment), Alignment);
                        group.AddAttribute(12, nameof(ShadcnInputGroupAddon.ChildContent), (RenderFragment)(addon =>
                        {
                            addon.OpenComponent<ShadcnInputGroupText>(0);
                            addon.AddAttribute(1, nameof(ShadcnInputGroupText.ChildContent), Text("THB / part"));
                            addon.CloseComponent();
                            addon.OpenComponent<ShadcnInputGroupButton>(2);
                            addon.AddAttribute(3, nameof(ShadcnInputGroupButton.Variant), ShadcnButtonVariant.Ghost);
                            addon.AddAttribute(4, nameof(ShadcnInputGroupButton.Size), ShadcnInputGroupButtonSize.IconExtraSmall);
                            addon.AddAttribute(5, nameof(ShadcnInputGroupButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, ResetUnitPrice));
                            addon.AddAttribute(6, nameof(ShadcnInputGroupButton.AdditionalAttributes), new Dictionary<string, object>
                            {
                                ["aria-label"] = "Reset unit price",
                                ["data-testid"] = "input-group-reset"
                            });
                            addon.AddAttribute(7, nameof(ShadcnInputGroupButton.ChildContent), ResetIcon());
                            addon.CloseComponent();
                        }));
                        group.CloseComponent();
                    }));
                    content.CloseComponent();
                    content.CloseElement();
                }));
                card.CloseComponent();

                card.OpenComponent<ShadcnCardFooter>(20);
                card.AddAttribute(21, nameof(ShadcnCardFooter.Class), "showcase-input-group-summary");
                card.AddAttribute(22, nameof(ShadcnCardFooter.ChildContent), (RenderFragment)(footer =>
                {
                    footer.OpenElement(0, "span");
                    footer.AddContent(1, "Estimated subtotal");
                    footer.CloseElement();
                    footer.OpenElement(2, "output");
                    footer.AddAttribute(3, "data-testid", "input-group-subtotal");
                    footer.AddAttribute(4, "for", "quote-unit-price");
                    footer.AddContent(5, (UnitPrice * Quantity).ToString("C0", ThaiCulture));
                    footer.CloseElement();
                }));
                card.CloseComponent();
            }));
            builder.CloseComponent();
            builder.CloseElement();
        }

        private void HandleUnitPriceChanged(decimal value) => UnitPrice = Math.Max(0, value);
        private void ResetUnitPrice() => UnitPrice = 1250m;

        private static RenderFragment ResetIcon() => icon =>
        {
            icon.OpenElement(0, "svg");
            icon.AddAttribute(1, "viewBox", "0 0 24 24");
            icon.AddAttribute(2, "fill", "none");
            icon.AddAttribute(3, "stroke", "currentColor");
            icon.AddAttribute(4, "stroke-width", "2");
            icon.AddAttribute(5, "stroke-linecap", "round");
            icon.AddAttribute(6, "stroke-linejoin", "round");
            icon.AddAttribute(7, "aria-hidden", "true");
            icon.OpenElement(8, "path");
            icon.AddAttribute(9, "d", "M3 12a9 9 0 1 0 3-6.7M3 4v5h5");
            icon.CloseElement();
            icon.CloseElement();
        };
    }

    private sealed class NativeSelectDossierPreview : ComponentBase
    {
        [Parameter] public ShadcnControlSize Size { get; set; } = ShadcnControlSize.Default;
        [Parameter] public bool Invalid { get; set; }
        [Parameter] public bool ReadOnly { get; set; }

        private string Priority { get; set; } = "standard";
        private string LeadTime => Priority == "urgent"
            ? "Urgent queue · 2–3 business days"
            : "Standard queue · 5–7 business days";

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "showcase-native-select-dossier");
            builder.AddAttribute(2, "data-testid", "native-select-dossier-preview");
            builder.OpenComponent<ShadcnCard>(3);
            builder.AddAttribute(4, nameof(ShadcnCard.ChildContent), (RenderFragment)(card =>
            {
                card.OpenComponent<ShadcnCardHeader>(0);
                card.AddAttribute(1, nameof(ShadcnCardHeader.ChildContent), (RenderFragment)(header =>
                {
                    header.OpenComponent<ShadcnCardTitle>(0);
                    header.AddAttribute(1, nameof(ShadcnCardTitle.ChildContent), Text("Production priority"));
                    header.CloseComponent();
                    header.OpenComponent<ShadcnCardDescription>(2);
                    header.AddAttribute(3, nameof(ShadcnCardDescription.ChildContent), Text("Route quotation Q-4189 through the planning queue."));
                    header.CloseComponent();
                }));
                card.CloseComponent();

                card.OpenComponent<ShadcnCardContent>(2);
                card.AddAttribute(3, nameof(ShadcnCardContent.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenElement(0, "div");
                    content.AddAttribute(1, "class", "showcase-native-select-field");
                    content.OpenComponent<ShadcnLabel>(2);
                    content.AddAttribute(3, nameof(ShadcnLabel.For), "production-priority");
                    content.AddAttribute(4, nameof(ShadcnLabel.ChildContent), Text("Priority and lead time"));
                    content.CloseComponent();
                    content.OpenComponent<ShadcnNativeSelect<string>>(5);
                    content.AddAttribute(6, nameof(ShadcnNativeSelect<string>.Value), Priority);
                    content.AddAttribute(7, nameof(ShadcnNativeSelect<string>.ValueChanged), EventCallback.Factory.Create<string>(this, HandlePriorityChanged));
                    content.AddAttribute(8, nameof(ShadcnNativeSelect<string>.Name), "priority");
                    content.AddAttribute(9, nameof(ShadcnNativeSelect<string>.Size), Size);
                    content.AddAttribute(10, nameof(ShadcnNativeSelect<string>.Invalid), Invalid);
                    content.AddAttribute(11, nameof(ShadcnNativeSelect<string>.ReadOnly), ReadOnly);
                    content.AddAttribute(12, nameof(ShadcnNativeSelect<string>.AdditionalAttributes), new Dictionary<string, object>
                    {
                        ["id"] = "production-priority",
                        ["data-testid"] = "forms-dossier-native-select",
                        ["aria-label"] = "Production priority",
                        ["aria-describedby"] = "production-priority-summary"
                    });
                    content.AddAttribute(13, nameof(ShadcnNativeSelect<string>.ChildContent), (RenderFragment)(select =>
                    {
                        AddNativeGroup(select, 0, "Production",
                        [
                            ("standard", "Standard · 5–7 business days", false),
                            ("urgent", "Urgent · 2–3 business days", false)
                        ]);
                        AddNativeGroup(select, 20, "Exceptions",
                        [
                            ("hold", "Engineering hold", true)
                        ]);
                    }));
                    content.CloseComponent();
                    content.OpenElement(14, "p");
                    content.AddAttribute(15, "id", "production-priority-summary");
                    content.AddAttribute(16, "class", "showcase-native-select-summary");
                    content.AddAttribute(17, "role", "status");
                    content.AddAttribute(18, "aria-live", "polite");
                    content.AddAttribute(19, "data-testid", "native-select-lead-time");
                    content.AddContent(20, LeadTime);
                    content.CloseElement();
                    content.CloseElement();
                }));
                card.CloseComponent();

                card.OpenComponent<ShadcnCardFooter>(4);
                card.AddAttribute(5, nameof(ShadcnCardFooter.ChildContent), Text("Native option groups remain available to keyboard and assistive technology."));
                card.CloseComponent();
            }));
            builder.CloseComponent();
            builder.CloseElement();
        }

        private Task HandlePriorityChanged(string value)
        {
            Priority = value;
            return Task.CompletedTask;
        }

        private static void AddNativeGroup(
            RenderTreeBuilder builder,
            int sequence,
            string label,
            IReadOnlyList<(string Value, string Text, bool Disabled)> options)
        {
            builder.OpenComponent<ShadcnNativeSelectOptGroup>(sequence);
            builder.AddAttribute(sequence + 1, nameof(ShadcnNativeSelectOptGroup.Label), label);
            builder.AddAttribute(sequence + 2, nameof(ShadcnNativeSelectOptGroup.ChildContent), (RenderFragment)(group =>
            {
                var optionSequence = 0;
                foreach (var option in options)
                {
                    group.OpenComponent<ShadcnNativeSelectOption<string>>(optionSequence++);
                    group.AddAttribute(optionSequence++, nameof(ShadcnNativeSelectOption<string>.Value), option.Value);
                    group.AddAttribute(optionSequence++, nameof(ShadcnNativeSelectOption<string>.Disabled), option.Disabled);
                    group.AddAttribute(optionSequence++, nameof(ShadcnNativeSelectOption<string>.ChildContent), Text(option.Text));
                    group.CloseComponent();
                }
            }));
            builder.CloseComponent();
        }
    }

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
