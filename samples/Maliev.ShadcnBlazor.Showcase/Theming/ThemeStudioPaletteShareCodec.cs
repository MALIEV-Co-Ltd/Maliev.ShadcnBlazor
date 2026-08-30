using System.Text;
using Maliev.ShadcnBlazor.Theming;

namespace Maliev.ShadcnBlazor.Showcase.Theming;

public static class ThemeStudioPaletteShareCodec
{
    private const string Prefix = "palette-v1:";

    public static string Encode(ShadcnThemeDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var bytes = Encoding.UTF8.GetBytes(ShadcnThemeDocumentSerializer.Serialize(document));
        return Prefix + Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    public static ShadcnThemeDocument Decode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > 1_000_000)
            throw new FormatException("Palette share value is too large.");
        if (!value.StartsWith(Prefix, StringComparison.Ordinal))
            throw new FormatException("Palette share value has an unsupported version.");
        var payload = value[Prefix.Length..].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        return ShadcnThemeDocumentSerializer.Deserialize(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
    }
}
