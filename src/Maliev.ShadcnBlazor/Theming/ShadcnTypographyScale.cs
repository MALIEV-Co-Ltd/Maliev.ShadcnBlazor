using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Identifies a semantic typography role.</summary>
public enum ShadcnTypographyRole
{
    /// <summary>Body copy.</summary>
    Body,
    /// <summary>First-level headings.</summary>
    Heading1,
    /// <summary>Second-level headings.</summary>
    Heading2,
    /// <summary>Third-level headings.</summary>
    Heading3,
    /// <summary>Fourth- through sixth-level headings.</summary>
    Heading4To6,
    /// <summary>Form and metadata labels.</summary>
    Label,
    /// <summary>Button labels.</summary>
    Button,
    /// <summary>Captions and supporting copy.</summary>
    Caption,
    /// <summary>Code and preformatted content.</summary>
    Code
}

/// <summary>Describes a portable font choice.</summary>
/// <param name="Family">The validated CSS font-family stack.</param>
/// <param name="Fallback">The fallback CSS font-family stack.</param>
/// <param name="GoogleFontsId">The optional checked-in Google Fonts catalog identifier.</param>
public sealed record ShadcnFontSelection(string Family, string Fallback, string? GoogleFontsId);

/// <summary>Describes one semantic typography role.</summary>
/// <param name="Weight">The CSS font weight.</param>
/// <param name="Scale">The size multiplier relative to body copy.</param>
/// <param name="LineHeight">The unitless line height.</param>
/// <param name="LetterSpacingEm">The letter spacing in em units.</param>
public sealed record ShadcnTypographyRoleStyle(int Weight, double Scale, double LineHeight, double LetterSpacingEm);

/// <summary>Describes body, Thai fallback, code, and semantic typography roles.</summary>
public sealed record ShadcnTypographyScale
{
    /// <summary>Creates a portable typography scale with a defensive role snapshot.</summary>
    /// <param name="body">The body font selection.</param>
    /// <param name="thaiFallback">The Thai fallback font selection.</param>
    /// <param name="code">The code font selection.</param>
    /// <param name="roles">The semantic typography role styles.</param>
    [JsonConstructor]
    public ShadcnTypographyScale(
        ShadcnFontSelection body,
        ShadcnFontSelection thaiFallback,
        ShadcnFontSelection code,
        IReadOnlyDictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        Body = body;
        ThaiFallback = thaiFallback;
        Code = code;
        Roles = new ReadOnlyDictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle>(
            new Dictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle>(roles));
    }

    /// <summary>Gets the body font selection.</summary>
    public ShadcnFontSelection Body { get; init; }

    /// <summary>Gets the Thai fallback font selection.</summary>
    public ShadcnFontSelection ThaiFallback { get; init; }

    /// <summary>Gets the code font selection.</summary>
    public ShadcnFontSelection Code { get; init; }

    /// <summary>Gets an immutable snapshot of semantic typography role styles.</summary>
    public IReadOnlyDictionary<ShadcnTypographyRole, ShadcnTypographyRoleStyle> Roles { get; }
}
