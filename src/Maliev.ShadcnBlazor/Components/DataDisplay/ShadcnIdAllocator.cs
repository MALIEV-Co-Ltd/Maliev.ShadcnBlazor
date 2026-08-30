using System.Runtime.CompilerServices;

namespace Maliev.ShadcnBlazor.Components.DataDisplay;

/// <summary>Allocates deterministic, sibling-unique component identifiers within a Blazor service scope.</summary>
public interface IShadcnIdAllocator
{
    /// <summary>Returns the next identifier for a safe component prefix.</summary>
    string NextId(string prefix);
}

/// <summary>Replays deterministic per-prefix sequences across equivalent prerender and interactive scopes.</summary>
public sealed class ShadcnIdAllocator : IShadcnIdAllocator
{
    private readonly Dictionary<string, long> _sequences = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    /// <inheritdoc />
    public string NextId(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
            throw new ArgumentException("An identifier prefix may contain only ASCII letters, digits, hyphens, and underscores.", nameof(prefix));
        lock (_gate)
        {
            _sequences.TryGetValue(prefix, out var sequence);
            sequence++;
            _sequences[prefix] = sequence;
            return $"{prefix}-{sequence:x}";
        }
    }
}

internal static class ShadcnIdAllocatorResolver
{
    private static readonly ConditionalWeakTable<IServiceProvider, IShadcnIdAllocator> FallbackAllocators = new();

    internal static string NextId(this IServiceProvider services, string prefix)
    {
        ArgumentNullException.ThrowIfNull(services);
        var allocator = services.GetService(typeof(IShadcnIdAllocator)) as IShadcnIdAllocator
            ?? FallbackAllocators.GetValue(services, static _ => new ShadcnIdAllocator());
        return allocator.NextId(prefix);
    }
}
