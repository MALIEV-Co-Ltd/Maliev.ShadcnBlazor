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

    public ComponentParameterControl(
        string id,
        string label,
        ComponentParameterControlKind kind,
        string value,
        IReadOnlyList<string> options,
        Action<string> apply)
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
    }

    public string Id { get; }

    public string Label { get; }

    public ComponentParameterControlKind Kind { get; }

    public string Value { get; private set; }

    public IReadOnlyList<string> Options { get; }

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
    string RazorSource,
    RenderFragment Preview,
    IReadOnlyList<ComponentParameterControl> Controls,
    IReadOnlyList<string> StateTags);
