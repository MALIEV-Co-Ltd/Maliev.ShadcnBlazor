namespace Maliev.ShadcnBlazor.Components.DataDisplay;

internal enum ShadcnTableCompositionKind { Table, Section, Row }
internal sealed record ShadcnTableCompositionContext(ShadcnTableCompositionKind Kind);
