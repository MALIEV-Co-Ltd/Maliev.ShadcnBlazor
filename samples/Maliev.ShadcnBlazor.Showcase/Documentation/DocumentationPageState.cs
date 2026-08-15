namespace Maliev.ShadcnBlazor.Showcase.Documentation;

public sealed record DocumentationSection(string Id, string Label);

public sealed class DocumentationPageState
{
    private IReadOnlyList<DocumentationSection> _sections = [];

    public event EventHandler? Changed;

    public IReadOnlyList<DocumentationSection> Sections => _sections;

    public void SetSections(IEnumerable<DocumentationSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);
        var next = sections
            .Select(section => new DocumentationSection(
                Required(section.Id, nameof(sections), "Section IDs are required."),
                Required(section.Label, nameof(sections), "Section labels are required.")))
            .DistinctBy(section => section.Id, StringComparer.Ordinal)
            .ToArray();

        if (_sections.SequenceEqual(next))
            return;

        _sections = next;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        if (_sections.Count == 0)
            return;

        _sections = [];
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static string Required(string value, string parameterName, string message) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException(message, parameterName) : value.Trim();
}
