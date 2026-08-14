namespace Maliev.ShadcnBlazor.Components.Selection;

/// <summary>Configures the native form and accessible-name attributes for one slider thumb.</summary>
public sealed record ShadcnSliderThumbAttributes
{
    /// <summary>Gets the stable DOM identifier override.</summary>
    public string? Id { get; init; }

    /// <summary>Gets the submitted form field name override.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the external form owner identifier override.</summary>
    public string? Form { get; init; }

    /// <summary>Gets whether this thumb participates in native required validation.</summary>
    public bool Required { get; init; }

    /// <summary>Gets the accessible label override.</summary>
    public string? AriaLabel { get; init; }

    /// <summary>Gets the identifiers of elements that label this thumb.</summary>
    public string? AriaLabelledBy { get; init; }

    /// <summary>Gets additional native input attributes for this thumb.</summary>
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; init; }
}
