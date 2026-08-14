namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Describes an option in a Shadcn select.</summary>
public sealed record ShadcnSelectOption<TValue>(TValue Value, string Text, string? Group = null, bool Disabled = false);
