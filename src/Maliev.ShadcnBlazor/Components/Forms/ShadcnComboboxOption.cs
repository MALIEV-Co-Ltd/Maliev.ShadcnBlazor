namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Describes an option in a Shadcn combobox.</summary>
public sealed record ShadcnComboboxOption<TValue>(TValue Value, string Text, string? Group = null, bool Disabled = false);
