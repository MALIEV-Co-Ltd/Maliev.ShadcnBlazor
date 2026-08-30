using Microsoft.AspNetCore.Components.Forms;

namespace Maliev.ShadcnBlazor.Components.Forms;

/// <summary>Validates dropzone selections without reading or uploading file content.</summary>
public static class ShadcnDropzoneValidation
{
    /// <summary>Validates count, size, extension, and media-type constraints.</summary>
    public static ShadcnDropzoneSelection Validate(
        IReadOnlyList<IBrowserFile> files,
        string? accept = null,
        bool multiple = false,
        int maxFiles = 1,
        long maxFileSize = 10 * 1024 * 1024)
        => Validate(files, accept, multiple, maxFiles, maxFileSize, DefaultMessage);

    /// <summary>Validates count, size, extension, and media-type constraints with caller-provided error messages.</summary>
    public static ShadcnDropzoneSelection Validate(
        IReadOnlyList<IBrowserFile> files,
        string? accept,
        bool multiple,
        int maxFiles,
        long maxFileSize,
        Func<ShadcnDropzoneErrorCode, string?, long, string> messageFormatter)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(messageFormatter);
        if (maxFiles <= 0) throw new ArgumentOutOfRangeException(nameof(maxFiles));
        if (maxFileSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxFileSize));

        var errors = new List<ShadcnDropzoneError>();
        var allowedCount = multiple ? maxFiles : 1;
        if (files.Count > allowedCount)
            errors.Add(new(ShadcnDropzoneErrorCode.TooManyFiles, messageFormatter(ShadcnDropzoneErrorCode.TooManyFiles, null, allowedCount)));

        var accepted = ParseAccept(accept);
        foreach (var file in files)
        {
            if (file.Size > maxFileSize)
                errors.Add(new(ShadcnDropzoneErrorCode.FileTooLarge, messageFormatter(ShadcnDropzoneErrorCode.FileTooLarge, file.Name, maxFileSize), file.Name));
            if (accepted.Count > 0 && !accepted.Any(value => Accepts(value, file)))
                errors.Add(new(ShadcnDropzoneErrorCode.FileTypeNotAccepted, messageFormatter(ShadcnDropzoneErrorCode.FileTypeNotAccepted, file.Name, 0), file.Name));
        }

        return new(files, errors);
    }

    private static string DefaultMessage(ShadcnDropzoneErrorCode code, string? fileName, long limit) => code switch
    {
        ShadcnDropzoneErrorCode.TooManyFiles => $"Select no more than {limit} file{(limit == 1 ? string.Empty : "s")}.",
        ShadcnDropzoneErrorCode.FileTooLarge => $"{fileName} exceeds the {limit} byte limit.",
        ShadcnDropzoneErrorCode.FileTypeNotAccepted => $"{fileName} is not an accepted file type.",
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown dropzone error code.")
    };

    private static IReadOnlyList<string> ParseAccept(string? accept) => string.IsNullOrWhiteSpace(accept)
        ? []
        : accept.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => value.Length > 0)
            .ToArray();

    private static bool Accepts(string accepted, IBrowserFile file)
    {
        if (accepted.StartsWith(".", StringComparison.Ordinal))
            return file.Name.EndsWith(accepted, StringComparison.OrdinalIgnoreCase);
        if (accepted.EndsWith("/*", StringComparison.Ordinal))
            return file.ContentType.StartsWith(accepted[..^1], StringComparison.OrdinalIgnoreCase);
        return string.Equals(file.ContentType, accepted, StringComparison.OrdinalIgnoreCase);
    }
}
