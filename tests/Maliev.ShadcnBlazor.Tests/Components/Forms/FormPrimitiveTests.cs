using System.ComponentModel.DataAnnotations;
using Bunit;
using Maliev.ShadcnBlazor.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Maliev.ShadcnBlazor.Tests.Components.Forms;

public sealed class FormPrimitiveTests : BunitContext
{
    public FormPrimitiveTests()
    {
        var module = JSInterop.SetupModule("./_content/Maliev.ShadcnBlazor/js/shadcn-forms.js");
        module.SetupVoid("restoreSelectValue", _ => true);
    }

    [Fact]
    public void InputRendersNativeMobileAndValidationContract()
    {
        var cut = Render<ShadcnInput<string>>(parameters => parameters
            .Add(component => component.Value, "ชิ้นส่วน")
            .Add(component => component.Name, "search")
            .Add(component => component.Type, "search")
            .Add(component => component.InputMode, "search")
            .Add(component => component.AutoComplete, "off")
            .Add(component => component.Required, true)
            .Add(component => component.Invalid, true)
            .AddUnmatched("dir", "rtl"));

        var input = cut.Find("input[data-slot='input']");
        Assert.Equal("ชิ้นส่วน", input.GetAttribute("value"));
        Assert.Equal("search", input.GetAttribute("name"));
        Assert.Equal("search", input.GetAttribute("type"));
        Assert.Equal("search", input.GetAttribute("inputmode"));
        Assert.Equal("off", input.GetAttribute("autocomplete"));
        Assert.True(input.HasAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("rtl", input.GetAttribute("dir"));
    }

    [Fact]
    public void FileInputDoesNotRenderAProgrammaticValue()
    {
        var cut = Render<ShadcnInput<string>>(parameters => parameters
            .Add(component => component.Type, "file")
            .Add(component => component.Value, "forbidden-path"));

        Assert.False(cut.Find("input").HasAttribute("value"));
        var parameters = typeof(ShadcnInput<string>).GetProperties().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("FilesChanged", parameters);
        Assert.Contains("Accept", parameters);
        Assert.Contains("Multiple", parameters);
        Assert.Contains("Capture", parameters);
    }

    [Fact]
    public void InputNotifiesEditContextAndParsesTypedValue()
    {
        var model = new FormModel { Quantity = 4 };
        var editContext = new EditContext(model);
        FieldIdentifier? changed = null;
        editContext.OnFieldChanged += (_, args) => changed = args.FieldIdentifier;
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnInput<int>>(input => input
                .Add(component => component.Value, model.Quantity)
                .Add(component => component.ValueExpression, () => model.Quantity)
                .Add(component => component.ValueChanged, value => model.Quantity = value)));

        cut.Find("input").Input("12");

        Assert.Equal(12, model.Quantity);
        Assert.Equal(nameof(FormModel.Quantity), changed?.FieldName);
    }

    [Fact]
    public void InvalidTypedInputAddsAndThenClearsEditContextParsingMessage()
    {
        var model = new FormModel { Quantity = 4 };
        var editContext = new EditContext(model);
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnInput<int>>(input => input
                .Add(component => component.Value, model.Quantity)
                .Add(component => component.ValueExpression, () => model.Quantity)
                .Add(component => component.ValueChanged, value => model.Quantity = value)));

        cut.Find("input").Input("not-a-number");
        Assert.Equal(4, model.Quantity);
        Assert.Contains(editContext.GetValidationMessages(), message => message.Contains(nameof(FormModel.Quantity), StringComparison.Ordinal));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));

        cut.Find("input").Input("12");
        Assert.Empty(editContext.GetValidationMessages());
        Assert.Equal(12, model.Quantity);
    }

    [Fact]
    public void TypedParseMessageSurvivesValidationAndRerenderUntilInputIsCorrected()
    {
        var model = new FormModel { Quantity = 4 };
        var editContext = new EditContext(model);
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnInput<int>>(input => input
                .Add(component => component.Value, model.Quantity)
                .Add(component => component.ValueExpression, () => model.Quantity)
                .Add(component => component.ValueChanged, value => model.Quantity = value)));

        cut.Find("input").Input("bad");
        editContext.Validate();
        cut.Render(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnInput<int>>(input => input
                .Add(component => component.Value, model.Quantity)
                .Add(component => component.ValueExpression, () => model.Quantity)
                .Add(component => component.ValueChanged, value => model.Quantity = value)));

        Assert.Contains(editContext.GetValidationMessages(), message => message.Contains(nameof(FormModel.Quantity), StringComparison.Ordinal));
        Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid"));
    }

    [Fact]
    public void ExternalValidationStateChangeAutomaticallyRerendersInputInvalidState()
    {
        var model = new FormModel { Quantity = 4 };
        var editContext = new EditContext(model);
        var messages = new ValidationMessageStore(editContext);
        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(cascade => cascade.Value, editContext)
            .AddChildContent<ShadcnInput<int>>(input => input
                .Add(component => component.Value, model.Quantity)
                .Add(component => component.ValueExpression, () => model.Quantity)));

        messages.Add(new FieldIdentifier(model, nameof(FormModel.Quantity)), "External error");
        editContext.NotifyValidationStateChanged();

        cut.WaitForAssertion(() => Assert.Equal("true", cut.Find("input").GetAttribute("aria-invalid")));
    }

    [Fact]
    public void ReadOnlyAndDisabledInputSuppressValueCallbacks()
    {
        var calls = 0;
        var readOnly = Render<ShadcnInput<string>>(parameters => parameters
            .Add(component => component.Value, "locked")
            .Add(component => component.ReadOnly, true)
            .Add(component => component.ValueChanged, _ => calls++));
        readOnly.Find("input").Input("changed");

        var disabled = Render<ShadcnInput<string>>(parameters => parameters
            .Add(component => component.Value, "locked")
            .Add(component => component.Disabled, true)
            .Add(component => component.ValueChanged, _ => calls++));
        disabled.Find("input").Input("changed");

        Assert.Equal(0, calls);
        Assert.True(readOnly.Find("input").HasAttribute("readonly"));
        Assert.True(disabled.Find("input").HasAttribute("disabled"));
    }

    [Fact]
    public void TextareaPreservesTypedBindingAndNativeRows()
    {
        var value = "รายละเอียด";
        var cut = Render<ShadcnTextarea<string>>(parameters => parameters
            .Add(component => component.Value, value)
            .Add(component => component.Rows, 5)
            .Add(component => component.Placeholder, "อธิบาย")
            .Add(component => component.ValueChanged, next => value = next));

        var textarea = cut.Find("textarea[data-slot='textarea']");
        Assert.Equal("5", textarea.GetAttribute("rows"));
        Assert.Equal("อธิบาย", textarea.GetAttribute("placeholder"));
        textarea.Input("อัปเดต");
        Assert.Equal("อัปเดต", value);
    }

    [Fact]
    public void NativeSelectRequestsTypedValueAndRendersGroups()
    {
        var selected = 2;
        var cut = Render<ShadcnNativeSelect<int>>(parameters => parameters
            .Add(component => component.Value, selected)
            .Add(component => component.Name, "priority")
            .Add(component => component.Size, ShadcnControlSize.Small)
            .Add(component => component.ValueChanged, value => selected = value)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnNativeSelectOptGroup>(0);
                builder.AddAttribute(1, nameof(ShadcnNativeSelectOptGroup.Label), "Priority");
                builder.AddAttribute(2, nameof(ShadcnNativeSelectOptGroup.ChildContent), (RenderFragment)(group =>
                {
                    group.OpenComponent<ShadcnNativeSelectOption<int>>(0);
                    group.AddAttribute(1, nameof(ShadcnNativeSelectOption<int>.Value), 1);
                    group.AddAttribute(2, nameof(ShadcnNativeSelectOption<int>.ChildContent), (RenderFragment)(content => content.AddContent(0, "Low")));
                    group.CloseComponent();
                    group.OpenComponent<ShadcnNativeSelectOption<int>>(3);
                    group.AddAttribute(4, nameof(ShadcnNativeSelectOption<int>.Value), 2);
                    group.AddAttribute(5, nameof(ShadcnNativeSelectOption<int>.ChildContent), (RenderFragment)(content => content.AddContent(0, "High")));
                    group.CloseComponent();
                }));
                builder.CloseComponent();
            }));

        var select = cut.Find("select[data-slot='native-select']");
        Assert.Equal("sm", select.GetAttribute("data-size"));
        Assert.Equal("priority", select.GetAttribute("name"));
        Assert.Equal("Priority", cut.Find("optgroup").GetAttribute("label"));
        Assert.True(cut.FindAll("option")[1].HasAttribute("selected"));
        select.Change("1");
        Assert.Equal(1, selected);
    }

    [Fact]
    public void ReadOnlyNativeSelectRestoresControlledSelectionWithoutCallback()
    {
        var calls = 0;
        var cut = Render<ShadcnNativeSelect<int>>(parameters => parameters
            .Add(component => component.Value, 2)
            .Add(component => component.ReadOnly, true)
            .Add(component => component.ValueChanged, _ => calls++)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<ShadcnNativeSelectOption<int>>(0);
                builder.AddAttribute(1, nameof(ShadcnNativeSelectOption<int>.Value), 1);
                builder.AddAttribute(2, nameof(ShadcnNativeSelectOption<int>.ChildContent), (RenderFragment)(content => content.AddContent(0, "One")));
                builder.CloseComponent();
                builder.OpenComponent<ShadcnNativeSelectOption<int>>(3);
                builder.AddAttribute(4, nameof(ShadcnNativeSelectOption<int>.Value), 2);
                builder.AddAttribute(5, nameof(ShadcnNativeSelectOption<int>.ChildContent), (RenderFragment)(content => content.AddContent(0, "Two")));
                builder.CloseComponent();
            }));

        cut.Find("select").Change("1");

        Assert.Equal(0, calls);
        Assert.True(cut.FindAll("option")[1].HasAttribute("selected"));
        Assert.Equal("true", cut.Find("select").GetAttribute("aria-readonly"));
    }

    [Fact]
    public void UnknownControlSizeIsRejected()
    {
        Assert.ThrowsAny<Exception>(() => Render<ShadcnNativeSelect<string>>(parameters => parameters
            .Add(component => component.Size, (ShadcnControlSize)999)));
    }

    private sealed class FormModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
