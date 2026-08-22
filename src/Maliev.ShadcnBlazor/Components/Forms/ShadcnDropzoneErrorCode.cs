namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Identifies a stable client-side dropzone validation failure.</summary>
public enum ShadcnDropzoneErrorCode
{
    /// <summary>The selection contains more files than allowed.</summary>
    TooManyFiles,
    /// <summary>A selected file exceeds the configured size limit.</summary>
    FileTooLarge,
    /// <summary>A selected file does not match the accepted extensions or media types.</summary>
    FileTypeNotAccepted
}
