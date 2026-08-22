namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Describes one file selection validation failure.</summary>
/// <param name="Code">The stable error category.</param>
/// <param name="Message">The localized-ready default message.</param>
/// <param name="FileName">The affected file name, when the error belongs to one file.</param>
public sealed record ShadcnDropzoneError(ShadcnDropzoneErrorCode Code, string Message, string? FileName = null);
