namespace Maliev.ShadcnBlazor.Components.DataDisplay;

internal enum ShadcnTableCompositionKind { Table, Section, Row }
internal sealed class ShadcnTableCompositionContext(ShadcnTableCompositionKind kind)
{
    internal ShadcnTableCompositionKind Kind { get; } = kind;
    internal int RenderedColumnCount { get; private set; }

    internal void ResetColumns() => RenderedColumnCount = 0;

    internal void RegisterColumns(int count) => RenderedColumnCount += count;
}
