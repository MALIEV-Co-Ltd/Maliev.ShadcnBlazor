namespace Maliev.ShadcnBlazor.Components.Feedback;

internal sealed class ShadcnProgressContext(string labelId, Action labelChanged)
{
    private int _labelCount;
    public string LabelId { get; } = labelId;
    public double? ClampedValue { get; private set; }
    public double? Percent { get; private set; }
    public string FormattedValue { get; private set; } = string.Empty;
    public string State => Percent is null ? "indeterminate" : "determinate";
    public bool HasLabel => _labelCount > 0;
    public void RegisterLabel() { _labelCount++; labelChanged(); }
    public void UnregisterLabel() { if (_labelCount > 0) { _labelCount--; labelChanged(); } }

    public void Update(double? clampedValue, double? percent, string formattedValue)
    {
        ClampedValue = clampedValue;
        Percent = percent;
        FormattedValue = formattedValue;
    }
}
