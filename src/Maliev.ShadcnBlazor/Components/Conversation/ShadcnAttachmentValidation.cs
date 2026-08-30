namespace Maliev.ShadcnBlazor.Components.Conversation;

public sealed record ShadcnAttachmentFile(string Name, long Size, string ContentType);

public sealed record ShadcnAttachmentValidationOptions
{
    public IReadOnlySet<string> AcceptedTypes { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<string> AcceptedExtensions { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public long? MaximumFileSize { get; init; }
    public int? MaximumFileCount { get; init; }
    public Func<ShadcnAttachmentFile, string?>? ValidateFile { get; init; }
    public Func<int, string> MaximumFileCountMessage { get; init; } = maximum => $"At most {maximum} files are allowed.";
    public Func<ShadcnAttachmentFile, string> UnsupportedFileTypeMessage { get; init; } = file => $"{file.Name} has an unsupported file type.";
    public Func<ShadcnAttachmentFile, long, string> MaximumFileSizeMessage { get; init; } = (file, _) => $"{file.Name} exceeds the maximum file size.";
}

public sealed record ShadcnAttachmentValidationError(string FileName, string Code, string Message);
public sealed record ShadcnAttachmentValidationResult(IReadOnlyList<ShadcnAttachmentFile> Accepted, IReadOnlyList<ShadcnAttachmentValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public static class ShadcnAttachmentValidator
{
    public static ShadcnAttachmentValidationResult Validate(IReadOnlyList<ShadcnAttachmentFile> files, ShadcnAttachmentValidationOptions options)
    {
        ArgumentNullException.ThrowIfNull(files); ArgumentNullException.ThrowIfNull(options);
        if (options.MaximumFileSize is <= 0) throw new ArgumentOutOfRangeException(nameof(options.MaximumFileSize));
        if (options.MaximumFileCount is <= 0) throw new ArgumentOutOfRangeException(nameof(options.MaximumFileCount));
        var errors = new List<ShadcnAttachmentValidationError>();
        if (options.MaximumFileCount is { } maximum && files.Count > maximum)
            errors.Add(new("*", "count", options.MaximumFileCountMessage(maximum)));
        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.Name) || file.Size < 0 || string.IsNullOrWhiteSpace(file.ContentType)) throw new ArgumentException("Attachment files require a name, non-negative size, and content type.", nameof(files));
            var extension = Path.GetExtension(file.Name);
            if (options.AcceptedTypes.Count > 0 && !options.AcceptedTypes.Contains(file.ContentType) &&
                (options.AcceptedExtensions.Count == 0 || !options.AcceptedExtensions.Contains(extension)))
                errors.Add(new(file.Name, "type", options.UnsupportedFileTypeMessage(file)));
            else if (options.AcceptedExtensions.Count > 0 && options.AcceptedTypes.Count == 0 && !options.AcceptedExtensions.Contains(extension))
                errors.Add(new(file.Name, "type", options.UnsupportedFileTypeMessage(file)));
            if (options.MaximumFileSize is { } maximumSize && file.Size > maximumSize)
                errors.Add(new(file.Name, "size", options.MaximumFileSizeMessage(file, maximumSize)));
            var callerError = options.ValidateFile?.Invoke(file);
            if (!string.IsNullOrWhiteSpace(callerError)) errors.Add(new(file.Name, "custom", callerError));
        }
        var rejected = errors.Select(error => error.FileName).ToHashSet(StringComparer.Ordinal);
        return new(files.Where(file => !rejected.Contains(file.Name) && !rejected.Contains("*")).ToArray(), errors);
    }
}
