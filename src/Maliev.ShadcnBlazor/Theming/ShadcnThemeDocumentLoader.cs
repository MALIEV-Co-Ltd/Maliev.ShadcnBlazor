using System.Text;
using System.Text.Json;

namespace Maliev.ShadcnBlazor.Theming;

/// <summary>Loads bounded, dependency-free portable theme documents from streams.</summary>
public static class ShadcnThemeDocumentLoader
{
    /// <summary>Gets the maximum accepted document size in bytes.</summary>
    public const int MaxDocumentBytes = 1_048_576;

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    /// <summary>Loads a canonical or supported legacy theme document.</summary>
    /// <param name="stream">A readable stream positioned at the document start.</param>
    /// <returns>The validated canonical document.</returns>
    public static ShadcnThemeDocument Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Theme document stream must be readable.", nameof(stream));
        using var memory = new MemoryStream();
        CopyBounded(stream, memory);
        return Deserialize(memory.ToArray());
    }

    /// <summary>Asynchronously loads a canonical or supported legacy theme document.</summary>
    /// <param name="stream">A readable stream positioned at the document start.</param>
    /// <param name="cancellationToken">A token that cancels the read.</param>
    /// <returns>The validated canonical document.</returns>
    public static async ValueTask<ShadcnThemeDocument> LoadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("Theme document stream must be readable.", nameof(stream));
        using var memory = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
                break;
            if (memory.Length + read > MaxDocumentBytes)
                throw new InvalidDataException($"Theme document exceeds the {MaxDocumentBytes} byte limit.");
            await memory.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        return Deserialize(memory.ToArray());
    }

    private static void CopyBounded(Stream input, Stream output)
    {
        var buffer = new byte[81920];
        while (true)
        {
            var read = input.Read(buffer, 0, buffer.Length);
            if (read == 0)
                return;
            if (output.Length + read > MaxDocumentBytes)
                throw new InvalidDataException($"Theme document exceeds the {MaxDocumentBytes} byte limit.");
            output.Write(buffer, 0, read);
        }
    }

    private static ShadcnThemeDocument Deserialize(byte[] bytes)
    {
        try
        {
            _ = StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException("Theme document must contain valid UTF-8 text.", exception);
        }
        try
        {
            return ShadcnThemeDocumentSerializer.Deserialize(bytes);
        }
        catch (JsonException)
        {
            throw;
        }
    }
}
