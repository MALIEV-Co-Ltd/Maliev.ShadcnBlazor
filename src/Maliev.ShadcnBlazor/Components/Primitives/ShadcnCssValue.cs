namespace Maliev.ShadcnBlazor.Components.Primitives;

internal static class ShadcnCssValue
{
    public static string? OptionalSingleDeclarationValue(string? value, string parameterName)
    {
        if (value is null)
            return null;

        var normalized = value.Trim();
        if (normalized.Length == 0 || normalized.IndexOfAny([';', '{', '}', '\r', '\n']) >= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "The CSS value must be one value without additional declarations.");

        return normalized;
    }
}
