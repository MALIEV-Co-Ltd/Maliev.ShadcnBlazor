namespace Maliev.ShadcnBlazor.Showcase.Documentation.Examples;

internal static class RazorSourceComposer
{
    internal static string Compose(string requiredNamespace, string exampleSource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredNamespace);
        ArgumentNullException.ThrowIfNull(exampleSource);

        var normalized = exampleSource.Replace("\r\n", "\n", StringComparison.Ordinal).Trim();
        var lines = normalized.Split('\n');
        var imports = new List<string> { $"@using {requiredNamespace}" };
        var bodyStart = 0;

        while (bodyStart < lines.Length)
        {
            var line = lines[bodyStart].Trim();
            if (line.Length == 0)
            {
                bodyStart++;
                continue;
            }

            if (!line.StartsWith("@using ", StringComparison.Ordinal))
                break;

            if (!imports.Contains(line, StringComparer.Ordinal))
                imports.Add(line);
            bodyStart++;
        }

        var body = string.Join('\n', lines[bodyStart..]).Trim();
        return body.Length == 0 ? string.Join('\n', imports) : $"{string.Join('\n', imports)}\n\n{body}";
    }
}
