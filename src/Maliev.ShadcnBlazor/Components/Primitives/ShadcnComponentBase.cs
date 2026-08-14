using Microsoft.AspNetCore.Components;

namespace Maliev.ShadcnBlazor.Components.Primitives;

/// <summary>
/// Provides the shared presentation and unmatched-attribute contract for Shadcn components.
/// </summary>
public abstract class ShadcnComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets caller-provided layout classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets or sets caller-provided inline layout styles.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets unmatched HTML attributes forwarded to the rendered root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Combines framework and caller class tokens while preserving first-seen order.
    /// </summary>
    protected string MergeClass(string frameworkClass)
    {
        var tokens = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        AddClassTokens(frameworkClass, tokens, seen);
        AddClassTokens(Class, tokens, seen);
        AddClassTokens(GetAdditionalAttribute("class"), tokens, seen);

        return string.Join(' ', tokens);
    }

    /// <summary>
    /// Combines framework and caller style declarations while preserving their precedence order.
    /// </summary>
    protected string? MergeStyle(string? frameworkStyle)
    {
        var declarations = new[]
            {
                frameworkStyle,
                Style,
                GetAdditionalAttribute("style")
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim().TrimEnd(';'))
            .ToArray();

        return declarations.Length == 0 ? null : string.Join("; ", declarations);
    }

    /// <summary>
    /// Returns caller attributes excluding class, style, and component-owned attribute names.
    /// </summary>
    protected IReadOnlyDictionary<string, object> AttributesExcept(params string[] protectedNames)
    {
        if (AdditionalAttributes is null || AdditionalAttributes.Count == 0)
            return new Dictionary<string, object>();

        var excluded = new HashSet<string>(protectedNames, StringComparer.OrdinalIgnoreCase)
        {
            "class",
            "style"
        };
        var attributes = new Dictionary<string, object>();

        foreach (var attribute in AdditionalAttributes)
        {
            if (!excluded.Contains(attribute.Key))
                attributes.Add(attribute.Key, attribute.Value);
        }

        return attributes;
    }

    private string? GetAdditionalAttribute(string name) => AdditionalAttributes?
        .FirstOrDefault(attribute => string.Equals(attribute.Key, name, StringComparison.OrdinalIgnoreCase))
        .Value?.ToString();

    private static void AddClassTokens(
        string? value,
        ICollection<string> tokens,
        ISet<string> seen)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        foreach (var token in value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            if (seen.Add(token))
                tokens.Add(token);
        }
    }
}
