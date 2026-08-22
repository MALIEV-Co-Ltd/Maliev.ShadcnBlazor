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
    {
        ArgumentNullException.ThrowIfNull(files);
        if (maxFiles <= 0) throw new ArgumentOutOfRangeException(nameof(maxFiles));
        if (maxFileSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxFileSize));

        var errors = new List<ShadcnDropzoneError>();
        var allowedCount = multiple ? maxFiles : 1;
        if (files.Count > allowedCount)
            errors.Add(new(ShadcnDropzoneErrorCode.TooManyFiles, $"Select no more than {allowedCount} file{(allowedCount == 1 ? string.Empty : "s")}."));

        var accepted = ParseAccept(accept);
        foreach (var file in files)
        {
            if (file.Size > maxFileSize)
                errors.Add(new(ShadcnDropzoneErrorCode.FileTooLarge, $"{file.Name} exceeds the {maxFileSize} byte limit.", file.Name));
            if (accepted.Count > 0 && !accepted.Any(value => Accepts(value, file)))
                errors.Add(new(ShadcnDropzoneErrorCode.FileTypeNotAccepted, $"{file.Name} is not an accepted file type.", file.Name));
        }

        return new(files, errors);
    }

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
