namespace Maliev.ShadcnBlazor.Components.Actions;

internal static class ShadcnToggleValues
{
    internal static string Variant(ShadcnToggleVariant value) => value switch
    {
        ShadcnToggleVariant.Default => "default",
        ShadcnToggleVariant.Outline => "outline",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown Shadcn toggle variant.")
    };

    internal static string Size(ShadcnToggleSize value) => value switch
    {
        ShadcnToggleSize.Default => "default",
        ShadcnToggleSize.Small => "sm",
        ShadcnToggleSize.Large => "lg",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown Shadcn toggle size.")
    };
}
