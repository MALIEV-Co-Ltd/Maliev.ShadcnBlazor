using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

public enum ComponentParameterControlKind
{
    Select,
    Number,
    Toggle
}

public sealed class ComponentParameterControl
{
    private readonly Action<string> _apply;
    private readonly Func<bool> _isEnabled;

    public ComponentParameterControl(
        string id,
        string label,
        ComponentParameterControlKind kind,
        string value,
        IReadOnlyList<string> options,
        Action<string> apply,
        Func<bool>? isEnabled = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(apply);

        Id = id;
        Label = label;
        Kind = kind;
        Value = value;
        Options = options;
        _apply = apply;
        _isEnabled = isEnabled ?? (() => true);
    }

    public string Id { get; }

    public string Label { get; }

    public ComponentParameterControlKind Kind { get; }

    public string Value { get; private set; }

    public IReadOnlyList<string> Options { get; }

    public bool IsEnabled => _isEnabled();

    public void Apply(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
        _apply(value);
    }
}

public sealed record ComponentExampleDefinition(
    string Id,
    string Title,
    string Description,
    string InitialRazorSource,
    RenderFragment Preview,
    IReadOnlyList<ComponentParameterControl> Controls,
    IReadOnlyList<string> StateTags)
{
    /// <summary>
    /// Optional state-aware source renderer used by interactive documentation examples.
    /// </summary>
    public Func<string>? RazorSourceProvider { get; init; }

    /// <summary>
    /// Returns the source for the current preview state, falling back to the initial source.
    /// </summary>
    public string RazorSource => RazorSourceProvider?.Invoke() ?? InitialRazorSource;
}
