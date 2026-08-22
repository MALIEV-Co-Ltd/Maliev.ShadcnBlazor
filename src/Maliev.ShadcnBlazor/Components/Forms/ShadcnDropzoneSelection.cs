using Microsoft.AspNetCore.Components.Forms;

namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Contains caller-owned files and package validation results without starting an upload.</summary>
/// <param name="Files">The files selected by the browser.</param>
/// <param name="Errors">Validation failures for the selection.</param>
public sealed record ShadcnDropzoneSelection(IReadOnlyList<IBrowserFile> Files, IReadOnlyList<ShadcnDropzoneError> Errors)
{
    /// <summary>Gets whether the selection satisfies every configured constraint.</summary>
    public bool IsValid => Errors.Count == 0;
}
