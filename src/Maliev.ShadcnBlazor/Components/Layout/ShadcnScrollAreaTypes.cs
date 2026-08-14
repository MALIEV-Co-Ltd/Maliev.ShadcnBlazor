namespace Maliev.ShadcnBlazor.Components.Layout;
/// <summary>Specifies when custom scrollbars are visible.</summary>
public enum ShadcnScrollAreaType { Hover, Scroll, Auto, Always }
/// <summary>Specifies a custom scrollbar axis.</summary>
public enum ShadcnScrollAreaOrientation { Vertical, Horizontal }
internal sealed record ShadcnScrollAreaContext(ShadcnScrollArea Owner);
