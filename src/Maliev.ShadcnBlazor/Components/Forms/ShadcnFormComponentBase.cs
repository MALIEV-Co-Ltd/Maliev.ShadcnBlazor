using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Provides the shared controlled binding, validation, and attribute contract for form controls.</summary>
public abstract class ShadcnFormComponentBase<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter] protected ShadcnFieldContext? ShadcnField { get; set; }
    [CascadingParameter] protected EditContext? EditContext { get; set; }

    [Parameter] public TValue Value { get; set; } = default!;
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }
    [Parameter] public Expression<Func<TValue>>? ValueExpression { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Invalid { get; set; }

    protected bool EffectiveDisabled => Disabled || ShadcnField?.Disabled == true;
    protected bool EffectiveInvalid => Invalid || ShadcnField?.Invalid == true || HasValidationMessages();
    protected string? DescribedBy => MergeIds(ShadcnField?.AriaDescribedBy, AttributeText("aria-describedby"));
    protected string CurrentValueAsString => FormatValue(Value);
    protected static string FormatFormValue(TValue value) => FormatValue(value);
    private ValidationMessageStore? ParsingMessages;
    private EditContext? SubscribedEditContext;

    protected string MergeClass(string frameworkClass, string? callerClass = null)
    {
        var values = new[] { frameworkClass, callerClass ?? Class, callerClass is null ? AttributeText("class") : null };
        return string.Join(' ', values.Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Distinct(StringComparer.Ordinal));
    }

    protected string? MergeStyle(string? callerStyle = null)
    {
        var values = new[] { callerStyle ?? Style, callerStyle is null ? AttributeText("style") : null }
            .Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!.Trim().TrimEnd(';')).ToArray();
        return values.Length == 0 ? null : string.Join("; ", values);
    }

    protected IReadOnlyDictionary<string, object> AttributesExcept(params string[] owned)
    {
        if (AdditionalAttributes is null) return new Dictionary<string, object>();
        var excluded = new HashSet<string>(owned, StringComparer.OrdinalIgnoreCase) { "class", "style" };
        return AdditionalAttributes.Where(pair => !excluded.Contains(pair.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    protected async Task SetCurrentValueFromStringAsync(string? value)
    {
        if (EffectiveDisabled || ReadOnly) return;
        EnsureValidationSubscription();
        var field = ValueExpression is null ? default : FieldIdentifier.Create(ValueExpression);
        if (ValueExpression is not null) ParsingMessages?.Clear(field);
        if (!TryParse(value, out var parsed))
        {
            if (EditContext is not null && ValueExpression is not null)
            {
                ParsingMessages ??= new ValidationMessageStore(EditContext);
                ParsingMessages.Add(field, $"The {field.FieldName} field is not valid.");
                EditContext.NotifyFieldChanged(field);
                EditContext.NotifyValidationStateChanged();
            }
            return;
        }
        await ValueChanged.InvokeAsync(parsed);
        if (EditContext is not null && ValueExpression is not null)
        {
            EditContext.NotifyFieldChanged(field);
            EditContext.NotifyValidationStateChanged();
        }
    }

    protected async Task SetCurrentValueAsync(TValue value)
    {
        if (EffectiveDisabled || ReadOnly) return;
        EnsureValidationSubscription();
        if (ValueExpression is not null) ParsingMessages?.Clear(FieldIdentifier.Create(ValueExpression));
        await ValueChanged.InvokeAsync(value);
        if (EditContext is not null && ValueExpression is not null)
        {
            EditContext.NotifyFieldChanged(FieldIdentifier.Create(ValueExpression));
            EditContext.NotifyValidationStateChanged();
        }
    }

    private bool HasValidationMessages() => EditContext is not null && ValueExpression is not null &&
        EditContext.GetValidationMessages(FieldIdentifier.Create(ValueExpression)).Any();

    private static bool TryParse(string? value, out TValue result)
    {
        if (typeof(TValue) == typeof(string))
        {
            result = (TValue)(object)(value ?? string.Empty);
            return true;
        }
        return BindConverter.TryConvertTo(value, CultureInfo.InvariantCulture, out result!);
    }

    protected override void OnParametersSet() { EnsureValidationSubscription(); }

    private void EnsureValidationSubscription()
    {
        if (ReferenceEquals(SubscribedEditContext, EditContext)) return;
        if (SubscribedEditContext is not null)
            SubscribedEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        ParsingMessages?.Clear();
        ParsingMessages = EditContext is null ? null : new ValidationMessageStore(EditContext);
        SubscribedEditContext = EditContext;
        if (SubscribedEditContext is not null)
            SubscribedEditContext.OnValidationStateChanged += HandleValidationStateChanged;
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs args) =>
        _ = InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        if (SubscribedEditContext is not null)
            SubscribedEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
        ParsingMessages?.Clear();
    }

    private static string FormatValue(TValue value) => value switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    private string? AttributeText(string name) => AdditionalAttributes?
        .FirstOrDefault(pair => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)).Value?.ToString();

    private static string? MergeIds(params string?[] values)
    {
        var ids = values.Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Distinct(StringComparer.Ordinal).ToArray();
        return ids.Length == 0 ? null : string.Join(' ', ids);
    }
}
