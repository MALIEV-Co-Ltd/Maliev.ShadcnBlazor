namespace Maliev.ShadcnBlazor.Components.Forms;

public readonly record struct ShadcnFieldContext(
    bool Invalid,
    bool Disabled,
    string? DescriptionId,
    string? ErrorId)
{
    public string? AriaDescribedBy
    {
        get
        {
            var ids = new[] { DescriptionId, Invalid ? ErrorId : null }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            return ids.Length == 0 ? null : string.Join(' ', ids);
        }
    }
}
